using Log4NetDemo.Appender.ErrorHandler;
using Log4NetDemo.Context;
using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using Log4NetDemo.Util.TextWriters;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Log4NetDemo.Appender
{
    public class RollingFileAppender : FileAppender
    {
        public enum RollingMode
        {
            /// <summary>
            /// Roll files once per program execution
            /// </summary>
            /// <remarks>
            /// <para>
            /// Roll files once per program execution.
            /// Well really once each time this appender is
            /// configured.
            /// </para>
            /// <para>
            /// Setting this option also sets <c>AppendToFile</c> to
            /// <c>false</c> on the <c>RollingFileAppender</c>, otherwise
            /// this appender would just be a normal file appender.
            /// </para>
            /// </remarks>
            Once = 0,

            /// <summary>
            /// Roll files based only on the size of the file
            /// </summary>
            Size = 1,

            /// <summary>
            /// Roll files based only on the date
            /// </summary>
            Date = 2,

            /// <summary>
            /// Roll files based on both the size and date of the file
            /// </summary>
            Composite = 3
        }

        protected enum RollPoint
        {
            /// <summary>
            /// Roll the log not based on the date
            /// </summary>
            InvalidRollPoint = -1,

            /// <summary>
            /// Roll the log for each minute
            /// </summary>
            TopOfMinute = 0,

            /// <summary>
            /// Roll the log for each hour
            /// </summary>
            TopOfHour = 1,

            /// <summary>
            /// Roll the log twice a day (midday and midnight)
            /// </summary>
            HalfDay = 2,

            /// <summary>
            /// Roll the log each day (midnight)
            /// </summary>
            TopOfDay = 3,

            /// <summary>
            /// Roll the log each week
            /// </summary>
            TopOfWeek = 4,

            /// <summary>
            /// Roll the log each month
            /// </summary>
            TopOfMonth = 5
        }

        public RollingFileAppender()
        {
        }

        ~RollingFileAppender()
        {
            if (m_mutexForRolling != null)
            {
                m_mutexForRolling.Dispose();
                m_mutexForRolling = null;
            }
        }

        #region Public Instance Properties

        public IDateTime DateTimeStrategy
        {
            get { return m_dateTime; }
            set { m_dateTime = value; }
        }

        public string DatePattern
        {
            get { return m_datePattern; }
            set { m_datePattern = value; }
        }

        public int MaxSizeRollBackups
        {
            get { return m_maxSizeRollBackups; }
            set { m_maxSizeRollBackups = value; }
        }

        public long MaxFileSize
        {
            get { return m_maxFileSize; }
            set { m_maxFileSize = value; }
        }

        public string MaximumFileSize
        {
            get { return m_maxFileSize.ToString(NumberFormatInfo.InvariantInfo); }
            set { m_maxFileSize = OptionConverter.ToFileSize(value, m_maxFileSize + 1); }
        }

        public int CountDirection
        {
            get { return m_countDirection; }
            set { m_countDirection = value; }
        }

        public RollingMode RollingStyle
        {
            get { return m_rollingStyle; }
            set
            {
                m_rollingStyle = value;
                switch (m_rollingStyle)
                {
                    case RollingMode.Once:
                        m_rollDate = false;
                        m_rollSize = false;

                        this.AppendToFile = false;
                        break;

                    case RollingMode.Size:
                        m_rollDate = false;
                        m_rollSize = true;
                        break;

                    case RollingMode.Date:
                        m_rollDate = true;
                        m_rollSize = false;
                        break;

                    case RollingMode.Composite:
                        m_rollDate = true;
                        m_rollSize = true;
                        break;
                }
            }
        }

        public bool PreserveLogFileNameExtension
        {
            get { return m_preserveLogFileNameExtension; }
            set { m_preserveLogFileNameExtension = value; }
        }

        public bool StaticLogFileName
        {
            get { return m_staticLogFileName; }
            set { m_staticLogFileName = value; }
        }

        #endregion

        #region Override implementation of FileAppender 

        override protected void SetQWForFiles(TextWriter writer)
        {
            QuietWriter = new CountingQuietTextWriter(writer, ErrorHandler);
        }

        override protected void Append(LoggingEvent loggingEvent)
        {
            AdjustFileBeforeAppend();
            base.Append(loggingEvent);
        }

        override protected void Append(LoggingEvent[] loggingEvents)
        {
            AdjustFileBeforeAppend();
            base.Append(loggingEvents);
        }

        virtual protected void AdjustFileBeforeAppend()
        {
            // reuse the file appenders locking model to lock the rolling
            try
            {
                // if rolling should be locked, acquire the lock
                if (m_mutexForRolling != null)
                {
                    m_mutexForRolling.WaitOne();
                }
                if (m_rollDate)
                {
                    DateTime n = m_dateTime.Now;
                    if (n >= m_nextCheck)
                    {
                        m_now = n;
                        m_nextCheck = NextCheckDate(m_now, m_rollPoint);

                        RollOverTime(true);
                    }
                }

                if (m_rollSize)
                {
                    if ((File != null) && ((CountingQuietTextWriter)QuietWriter).Count >= m_maxFileSize)
                    {
                        RollOverSize();
                    }
                }
            }
            finally
            {
                // if rolling should be locked, release the lock
                if (m_mutexForRolling != null)
                {
                    m_mutexForRolling.ReleaseMutex();
                }
            }
        }

        override protected void OpenFile(string fileName, bool append)
        {
            lock (this)
            {
                fileName = GetNextOutputFileName(fileName);

                // Calculate the current size of the file
                long currentCount = 0;
                if (append)
                {
                    using (SecurityContext.Impersonate(this))
                    {
                        if (System.IO.File.Exists(fileName))
                        {
                            currentCount = (new FileInfo(fileName)).Length;
                        }
                    }
                }
                else
                {
                    if (LogLog.IsErrorEnabled)
                    {
                        // Internal check that the file is not being overwritten
                        // If not Appending to an existing file we should have rolled the file out of the
                        // way. Therefore we should not be over-writing an existing file.
                        // The only exception is if we are not allowed to roll the existing file away.
                        if (m_maxSizeRollBackups != 0 && FileExists(fileName))
                        {
                            LogLog.Error(declaringType, "RollingFileAppender: INTERNAL ERROR. Append is False but OutputFile [" + fileName + "] already exists.");
                        }
                    }
                }

                if (!m_staticLogFileName)
                {
                    m_scheduledFilename = fileName;
                }

                // Open the file (call the base class to do it)
                base.OpenFile(fileName, append);

                // Set the file size onto the counting writer
                ((CountingQuietTextWriter)QuietWriter).Count = currentCount;
            }
        }

        protected string GetNextOutputFileName(string fileName)
        {
            if (!m_staticLogFileName)
            {
                fileName = fileName.Trim();

                if (m_rollDate)
                {
                    fileName = CombinePath(fileName, m_now.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo));
                }

                if (m_countDirection >= 0)
                {
                    fileName = CombinePath(fileName, "." + m_curSizeRollBackups);
                }
            }

            return fileName;
        }

        #endregion

        #region Initialize Options

        private void DetermineCurSizeRollBackups()
        {
            m_curSizeRollBackups = 0;

            string fullPath = null;
            string fileName = null;

            using (SecurityContext.Impersonate(this))
            {
                fullPath = System.IO.Path.GetFullPath(m_baseFileName);
                fileName = System.IO.Path.GetFileName(fullPath);
            }

            ArrayList arrayFiles = GetExistingFiles(fullPath);
            InitializeRollBackups(fileName, arrayFiles);

            LogLog.Debug(declaringType, "curSizeRollBackups starts at [" + m_curSizeRollBackups + "]");
        }

        private string GetWildcardPatternForFile(string baseFileName)
        {
            if (m_preserveLogFileNameExtension)
            {
                return Path.GetFileNameWithoutExtension(baseFileName) + "*" + Path.GetExtension(baseFileName);
            }
            else
            {
                return baseFileName + '*';
            }
        }

        private ArrayList GetExistingFiles(string baseFilePath)
        {
            ArrayList alFiles = new ArrayList();

            string directory = null;

            using (SecurityContext.Impersonate(this))
            {
                string fullPath = Path.GetFullPath(baseFilePath);

                directory = Path.GetDirectoryName(fullPath);
                if (Directory.Exists(directory))
                {
                    string baseFileName = Path.GetFileName(fullPath);

                    string[] files = Directory.GetFiles(directory, GetWildcardPatternForFile(baseFileName));

                    if (files != null)
                    {
                        for (int i = 0; i < files.Length; i++)
                        {
                            string curFileName = Path.GetFileName(files[i]);
                            if (curFileName.StartsWith(Path.GetFileNameWithoutExtension(baseFileName)))
                            {
                                alFiles.Add(curFileName);
                            }
                        }
                    }
                }
            }
            LogLog.Debug(declaringType, "Searched for existing files in [" + directory + "]");
            return alFiles;
        }

        private void RollOverIfDateBoundaryCrossing()
        {
            if (m_staticLogFileName && m_rollDate)
            {
                if (FileExists(m_baseFileName))
                {
                    DateTime last;
                    using (SecurityContext.Impersonate(this))
                    {
                        if (DateTimeStrategy is UniversalDateTime)
                        {
                            last = System.IO.File.GetLastWriteTimeUtc(m_baseFileName);
                        }
                        else
                        {
                            last = System.IO.File.GetLastWriteTime(m_baseFileName);
                        }
                    }
                    LogLog.Debug(declaringType, "[" + last.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo) + "] vs. [" + m_now.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo) + "]");

                    if (!(last.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo).Equals(m_now.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo))))
                    {
                        m_scheduledFilename = CombinePath(m_baseFileName, last.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo));
                        LogLog.Debug(declaringType, "Initial roll over to [" + m_scheduledFilename + "]");
                        RollOverTime(false);
                        LogLog.Debug(declaringType, "curSizeRollBackups after rollOver at [" + m_curSizeRollBackups + "]");
                    }
                }
            }
        }

        protected void ExistingInit()
        {
            DetermineCurSizeRollBackups();
            RollOverIfDateBoundaryCrossing();

            // If file exists and we are not appending then roll it out of the way
            if (AppendToFile == false)
            {
                bool fileExists = false;
                string fileName = GetNextOutputFileName(m_baseFileName);

                using (SecurityContext.Impersonate(this))
                {
                    fileExists = System.IO.File.Exists(fileName);
                }

                if (fileExists)
                {
                    if (m_maxSizeRollBackups == 0)
                    {
                        LogLog.Debug(declaringType, "Output file [" + fileName + "] already exists. MaxSizeRollBackups is 0; cannot roll. Overwriting existing file.");
                    }
                    else
                    {
                        LogLog.Debug(declaringType, "Output file [" + fileName + "] already exists. Not appending to file. Rolling existing file out of the way.");

                        RollOverRenameFiles(fileName);
                    }
                }
            }
        }

        private void InitializeFromOneFile(string baseFile, string curFileName)
        {
            if (curFileName.StartsWith(Path.GetFileNameWithoutExtension(baseFile)) == false)
            {
                // This is not a log file, so ignore
                return;
            }
            if (curFileName.Equals(baseFile))
            {
                // Base log file is not an incremented logfile (.1 or .2, etc)
                return;
            }

            /*
			if (m_staticLogFileName) 
			{
				int endLength = curFileName.Length - index;
				if (baseFile.Length + endLength != curFileName.Length) 
				{
					// file is probably scheduledFilename + .x so I don't care
					return;
				}
			}
            */

            // Only look for files in the current roll point
            if (m_rollDate && !m_staticLogFileName)
            {
                string date = m_dateTime.Now.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                string prefix = m_preserveLogFileNameExtension ? Path.GetFileNameWithoutExtension(baseFile) + date : baseFile + date;
                string suffix = m_preserveLogFileNameExtension ? Path.GetExtension(baseFile) : "";
                if (!curFileName.StartsWith(prefix) || !curFileName.EndsWith(suffix))
                {
                    LogLog.Debug(declaringType, "Ignoring file [" + curFileName + "] because it is from a different date period");
                    return;
                }
            }

            try
            {
                // Bump the counter up to the highest count seen so far
                int backup = GetBackUpIndex(curFileName);

                // caution: we might get a false positive when certain
                // date patterns such as yyyyMMdd are used...those are
                // valid number but aren't the kind of back up index
                // we're looking for
                if (backup > m_curSizeRollBackups)
                {
                    if (0 == m_maxSizeRollBackups)
                    {
                        // Stay at zero when zero backups are desired
                    }
                    else if (-1 == m_maxSizeRollBackups)
                    {
                        // Infinite backups, so go as high as the highest value
                        m_curSizeRollBackups = backup;
                    }
                    else
                    {
                        // Backups limited to a finite number
                        if (m_countDirection >= 0)
                        {
                            // Go with the highest file when counting up
                            m_curSizeRollBackups = backup;
                        }
                        else
                        {
                            // Clip to the limit when counting down
                            if (backup <= m_maxSizeRollBackups)
                            {
                                m_curSizeRollBackups = backup;
                            }
                        }
                    }
                    LogLog.Debug(declaringType, "File name [" + curFileName + "] moves current count to [" + m_curSizeRollBackups + "]");
                }
            }
            catch (FormatException)
            {
                //this happens when file.log -> file.log.yyyy-MM-dd which is normal
                //when staticLogFileName == false
                LogLog.Debug(declaringType, "Encountered a backup file not ending in .x [" + curFileName + "]");
            }
        }

        private int GetBackUpIndex(string curFileName)
        {
            int backUpIndex = -1;
            string fileName = curFileName;

            if (m_preserveLogFileNameExtension)
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }

            int index = fileName.LastIndexOf(".");
            if (index > 0)
            {
                // if the "yyyy-MM-dd" component of file.log.yyyy-MM-dd is passed to TryParse
                // it will gracefully fail and return backUpIndex will be 0
                SystemInfo.TryParse(fileName.Substring(index + 1), out backUpIndex);
            }

            return backUpIndex;
        }

        private void InitializeRollBackups(string baseFile, ArrayList arrayFiles)
        {
            if (null != arrayFiles)
            {
                string baseFileLower = baseFile.ToLower(System.Globalization.CultureInfo.InvariantCulture);

                foreach (string curFileName in arrayFiles)
                {
                    InitializeFromOneFile(baseFileLower, curFileName.ToLower(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
        }

        private RollPoint ComputeCheckPeriod(string datePattern)
        {
            // s_date1970 is 1970-01-01 00:00:00 this is UniversalSortableDateTimePattern 
            // (based on ISO 8601) using universal time. This date is used for reference
            // purposes to calculate the resolution of the date pattern.

            // Get string representation of base line date
            string r0 = s_date1970.ToString(datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo);

            // Check each type of rolling mode starting with the smallest increment.
            for (int i = (int)RollPoint.TopOfMinute; i <= (int)RollPoint.TopOfMonth; i++)
            {
                // Get string representation of next pattern
                string r1 = NextCheckDate(s_date1970, (RollPoint)i).ToString(datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo);

                LogLog.Debug(declaringType, "Type = [" + i + "], r0 = [" + r0 + "], r1 = [" + r1 + "]");

                // Check if the string representations are different
                if (r0 != null && r1 != null && !r0.Equals(r1))
                {
                    // Found highest precision roll point
                    return (RollPoint)i;
                }
            }

            return RollPoint.InvalidRollPoint; // Deliberately head for trouble...
        }

        #endregion

        #region Override implementation of IOptionHandler 

        override public void ActivateOptions()
        {
            if (m_dateTime == null)
            {
                m_dateTime = new LocalDateTime();
            }

            if (m_rollDate && m_datePattern != null)
            {
                m_now = m_dateTime.Now;
                m_rollPoint = ComputeCheckPeriod(m_datePattern);

                if (m_rollPoint == RollPoint.InvalidRollPoint)
                {
                    throw new ArgumentException("Invalid RollPoint, unable to parse [" + m_datePattern + "]");
                }

                // next line added as this removes the name check in rollOver
                m_nextCheck = NextCheckDate(m_now, m_rollPoint);
            }
            else
            {
                if (m_rollDate)
                {
                    ErrorHandler.Error("Either DatePattern or rollingStyle options are not set for [" + Name + "].");
                }
            }

            if (SecurityContext == null)
            {
                SecurityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
            }

            using (SecurityContext.Impersonate(this))
            {
                // Must convert the FileAppender's m_filePath to an absolute path before we
                // call ExistingInit(). This will be done by the base.ActivateOptions() but
                // we need to duplicate that functionality here first.
                base.File = ConvertToFullPath(base.File.Trim());

                // Store fully qualified base file name
                m_baseFileName = base.File;
            }

            // initialize the mutex that is used to lock rolling
            m_mutexForRolling = new Mutex(false, m_baseFileName.Replace("\\", "_").Replace(":", "_").Replace("/", "_"));

            if (m_rollDate && File != null && m_scheduledFilename == null)
            {
                m_scheduledFilename = CombinePath(File, m_now.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo));
            }

            ExistingInit();

            base.ActivateOptions();
        }

        #endregion

        #region Roll File

        private string CombinePath(string path1, string path2)
        {
            string extension = Path.GetExtension(path1);
            if (m_preserveLogFileNameExtension && extension.Length > 0)
            {
                return Path.Combine(Path.GetDirectoryName(path1), Path.GetFileNameWithoutExtension(path1) + path2 + extension);
            }
            else
            {
                return path1 + path2;
            }
        }

        protected void RollOverTime(bool fileIsOpen)
        {
            if (m_staticLogFileName)
            {
                // Compute filename, but only if datePattern is specified
                if (m_datePattern == null)
                {
                    ErrorHandler.Error("Missing DatePattern option in rollOver().");
                    return;
                }

                //is the new file name equivalent to the 'current' one
                //something has gone wrong if we hit this -- we should only
                //roll over if the new file will be different from the old
                string dateFormat = m_now.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo);
                if (m_scheduledFilename.Equals(CombinePath(File, dateFormat)))
                {
                    ErrorHandler.Error("Compare " + m_scheduledFilename + " : " + CombinePath(File, dateFormat));
                    return;
                }

                if (fileIsOpen)
                {
                    // close current file, and rename it to datedFilename
                    this.CloseFile();
                }

                //we may have to roll over a large number of backups here
                for (int i = 1; i <= m_curSizeRollBackups; i++)
                {
                    string from = CombinePath(File, "." + i);
                    string to = CombinePath(m_scheduledFilename, "." + i);
                    RollFile(from, to);
                }

                RollFile(File, m_scheduledFilename);
            }

            //We've cleared out the old date and are ready for the new
            m_curSizeRollBackups = 0;

            //new scheduled name
            m_scheduledFilename = CombinePath(File, m_now.ToString(m_datePattern, System.Globalization.DateTimeFormatInfo.InvariantInfo));

            if (fileIsOpen)
            {
                // This will also close the file. This is OK since multiple close operations are safe.
                SafeOpenFile(m_baseFileName, false);
            }
        }

        protected void RollFile(string fromFile, string toFile)
        {
            if (FileExists(fromFile))
            {
                // Delete the toFile if it exists
                DeleteFile(toFile);

                // We may not have permission to move the file, or the file may be locked
                try
                {
                    LogLog.Debug(declaringType, "Moving [" + fromFile + "] -> [" + toFile + "]");
                    using (SecurityContext.Impersonate(this))
                    {
                        System.IO.File.Move(fromFile, toFile);
                    }
                }
                catch (Exception moveEx)
                {
                    ErrorHandler.Error("Exception while rolling file [" + fromFile + "] -> [" + toFile + "]", moveEx, ErrorCode.GenericFailure);
                }
            }
            else
            {
                LogLog.Warn(declaringType, "Cannot RollFile [" + fromFile + "] -> [" + toFile + "]. Source does not exist");
            }
        }

        protected bool FileExists(string path)
        {
            using (SecurityContext.Impersonate(this))
            {
                return System.IO.File.Exists(path);
            }
        }

        protected void DeleteFile(string fileName)
        {
            if (FileExists(fileName))
            {
                // We may not have permission to delete the file, or the file may be locked

                string fileToDelete = fileName;

                // Try to move the file to temp name.
                // If the file is locked we may still be able to move it
                string tempFileName = fileName + "." + Environment.TickCount + ".DeletePending";
                try
                {
                    using (SecurityContext.Impersonate(this))
                    {
                        System.IO.File.Move(fileName, tempFileName);
                    }
                    fileToDelete = tempFileName;
                }
                catch (Exception moveEx)
                {
                    LogLog.Debug(declaringType, "Exception while moving file to be deleted [" + fileName + "] -> [" + tempFileName + "]", moveEx);
                }

                // Try to delete the file (either the original or the moved file)
                try
                {
                    using (SecurityContext.Impersonate(this))
                    {
                        System.IO.File.Delete(fileToDelete);
                    }
                    LogLog.Debug(declaringType, "Deleted file [" + fileName + "]");
                }
                catch (Exception deleteEx)
                {
                    if (fileToDelete == fileName)
                    {
                        // Unable to move or delete the file
                        ErrorHandler.Error("Exception while deleting file [" + fileToDelete + "]", deleteEx, ErrorCode.GenericFailure);
                    }
                    else
                    {
                        // Moved the file, but the delete failed. File is probably locked.
                        // The file should automatically be deleted when the lock is released.
                        LogLog.Debug(declaringType, "Exception while deleting temp file [" + fileToDelete + "]", deleteEx);
                    }
                }
            }
        }

        protected void RollOverSize()
        {
            this.CloseFile(); // keep windows happy.

            LogLog.Debug(declaringType, "rolling over count [" + ((CountingQuietTextWriter)QuietWriter).Count + "]");
            LogLog.Debug(declaringType, "maxSizeRollBackups [" + m_maxSizeRollBackups + "]");
            LogLog.Debug(declaringType, "curSizeRollBackups [" + m_curSizeRollBackups + "]");
            LogLog.Debug(declaringType, "countDirection [" + m_countDirection + "]");

            RollOverRenameFiles(File);

            if (!m_staticLogFileName && m_countDirection >= 0)
            {
                m_curSizeRollBackups++;
            }

            // This will also close the file. This is OK since multiple close operations are safe.
            SafeOpenFile(m_baseFileName, false);
        }

        protected void RollOverRenameFiles(string baseFileName)
        {
            // If maxBackups <= 0, then there is no file renaming to be done.
            if (m_maxSizeRollBackups != 0)
            {
                if (m_countDirection < 0)
                {
                    // Delete the oldest file, to keep Windows happy.
                    if (m_curSizeRollBackups == m_maxSizeRollBackups)
                    {
                        DeleteFile(CombinePath(baseFileName, "." + m_maxSizeRollBackups));
                        m_curSizeRollBackups--;
                    }

                    // Map {(maxBackupIndex - 1), ..., 2, 1} to {maxBackupIndex, ..., 3, 2}
                    for (int i = m_curSizeRollBackups; i >= 1; i--)
                    {
                        RollFile((CombinePath(baseFileName, "." + i)), (CombinePath(baseFileName, "." + (i + 1))));
                    }

                    m_curSizeRollBackups++;

                    // Rename fileName to fileName.1
                    RollFile(baseFileName, CombinePath(baseFileName, ".1"));
                }
                else
                {
                    //countDirection >= 0
                    if (m_curSizeRollBackups >= m_maxSizeRollBackups && m_maxSizeRollBackups > 0)
                    {
                        //delete the first and keep counting up.
                        int oldestFileIndex = m_curSizeRollBackups - m_maxSizeRollBackups;

                        // If static then there is 1 file without a number, therefore 1 less archive
                        if (m_staticLogFileName)
                        {
                            oldestFileIndex++;
                        }

                        // If using a static log file then the base for the numbered sequence is the baseFileName passed in
                        // If not using a static log file then the baseFileName will already have a numbered postfix which
                        // we must remove, however it may have a date postfix which we must keep!
                        string archiveFileBaseName = baseFileName;
                        if (!m_staticLogFileName)
                        {
                            int lastDotIndex = archiveFileBaseName.LastIndexOf(".");
                            if (lastDotIndex >= 0)
                            {
                                archiveFileBaseName = archiveFileBaseName.Substring(0, lastDotIndex);
                            }
                        }

                        // Delete the archive file
                        DeleteFile(CombinePath(archiveFileBaseName, "." + oldestFileIndex));
                    }

                    if (m_staticLogFileName)
                    {
                        m_curSizeRollBackups++;
                        RollFile(baseFileName, CombinePath(baseFileName, "." + m_curSizeRollBackups));
                    }
                }
            }
        }

        #endregion

        #region NextCheckDate

        protected DateTime NextCheckDate(DateTime currentDateTime, RollPoint rollPoint)
        {
            // Local variable to work on (this does not look very efficient)
            DateTime current = currentDateTime;

            // Do slightly different things depending on what the type of roll point we want.
            switch (rollPoint)
            {
                case RollPoint.TopOfMinute:
                    current = current.AddMilliseconds(-current.Millisecond);
                    current = current.AddSeconds(-current.Second);
                    current = current.AddMinutes(1);
                    break;

                case RollPoint.TopOfHour:
                    current = current.AddMilliseconds(-current.Millisecond);
                    current = current.AddSeconds(-current.Second);
                    current = current.AddMinutes(-current.Minute);
                    current = current.AddHours(1);
                    break;

                case RollPoint.HalfDay:
                    current = current.AddMilliseconds(-current.Millisecond);
                    current = current.AddSeconds(-current.Second);
                    current = current.AddMinutes(-current.Minute);

                    if (current.Hour < 12)
                    {
                        current = current.AddHours(12 - current.Hour);
                    }
                    else
                    {
                        current = current.AddHours(-current.Hour);
                        current = current.AddDays(1);
                    }
                    break;

                case RollPoint.TopOfDay:
                    current = current.AddMilliseconds(-current.Millisecond);
                    current = current.AddSeconds(-current.Second);
                    current = current.AddMinutes(-current.Minute);
                    current = current.AddHours(-current.Hour);
                    current = current.AddDays(1);
                    break;

                case RollPoint.TopOfWeek:
                    current = current.AddMilliseconds(-current.Millisecond);
                    current = current.AddSeconds(-current.Second);
                    current = current.AddMinutes(-current.Minute);
                    current = current.AddHours(-current.Hour);
                    current = current.AddDays(7 - (int)current.DayOfWeek);
                    break;

                case RollPoint.TopOfMonth:
                    current = current.AddMilliseconds(-current.Millisecond);
                    current = current.AddSeconds(-current.Second);
                    current = current.AddMinutes(-current.Minute);
                    current = current.AddHours(-current.Hour);
                    current = current.AddDays(1 - current.Day); /* first day of month is 1 not 0 */
                    current = current.AddMonths(1);
                    break;
            }
            return current;
        }

        #endregion

        private IDateTime m_dateTime = null;
        private string m_datePattern = ".yyyy-MM-dd";
        private string m_scheduledFilename = null;
        private DateTime m_nextCheck = DateTime.MaxValue;
        private DateTime m_now;
        private RollPoint m_rollPoint;
        private long m_maxFileSize = 10 * 1024 * 1024;
        private int m_maxSizeRollBackups = 0;
        private int m_curSizeRollBackups = 0;
        private int m_countDirection = -1;
        private RollingMode m_rollingStyle = RollingMode.Composite;
        private bool m_rollDate = true;
        private bool m_rollSize = true;
        private bool m_staticLogFileName = true;
        private bool m_preserveLogFileNameExtension = false;
        private string m_baseFileName;
        private Mutex m_mutexForRolling;

        private static readonly DateTime s_date1970 = new DateTime(1970, 1, 1);
        private readonly static Type declaringType = typeof(RollingFileAppender);

        #region DateTime

        public interface IDateTime
        {
            DateTime Now { get; }
        }

        private class LocalDateTime : IDateTime
        {
            public DateTime Now
            {
                get { return DateTime.Now; }
            }
        }

        private class UniversalDateTime : IDateTime
        {
            public DateTime Now
            {
                get { return DateTime.UtcNow; }
            }
        }

        #endregion
    }
}
