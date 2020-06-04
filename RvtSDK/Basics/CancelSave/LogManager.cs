using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CancelSave
{
    class LogManager
    {
        private static TraceListener TxtListener;
        private static string AssemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static bool RegressionTestNow
        {
            get
            {
                string expectedLogFile = Path.Combine(AssemblyLocation, "ExpectedOutPut.log");
                if (File.Exists(expectedLogFile))
                {
                    return true;
                }

                return false;
            }
        }

        static LogManager()
        {
            // Create CancelSave.log .
            string actullyLogFile = Path.Combine(AssemblyLocation, "CancelSave.log");

            // if already existed, delete it.
            if (File.Exists(actullyLogFile))
            {
                File.Delete(actullyLogFile);
            }

            TxtListener = new TextWriterTraceListener(actullyLogFile);
            Trace.Listeners.Add(TxtListener);
            Trace.AutoFlush = true;
        }

        public static void LogFinalize()
        {
            Trace.Flush();
            TxtListener.Close();
            Trace.Close();
            Trace.Listeners.Remove(TxtListener);
        }

        public static void WriteLog(string message)
        {
            Trace.WriteLine(message);
        }

        public static void WriteLog(EventArgs args, Document doc)
        {
            Trace.WriteLine("");
            Trace.WriteLine("[Event] " + GetEventName(args.GetType()) + ": " + TitleNoExt(doc.Title));
        }

        //
        // private Methods
        //

        private static string GetEventName(Type type)
        {
            String argName = type.ToString();
            String tail = "EventArgs";
            String head = "Autodesk.Revit.DB.Events.";
            int firstIndex = head.Length;
            int length = argName.Length - head.Length - tail.Length;
            String eventName = argName.Substring(firstIndex, length);
            return eventName;
        }

        private static string TitleNoExt(String orgTitle)
        {
            // return null directly if it's null
            if (String.IsNullOrEmpty(orgTitle))
            {
                return "";
            }

            // Remove the extension 
            int pos = orgTitle.LastIndexOf('.');
            if (-1 != pos)
            {
                return orgTitle.Remove(pos);
            }
            else
            {
                return orgTitle;
            }
        }
    }
}
