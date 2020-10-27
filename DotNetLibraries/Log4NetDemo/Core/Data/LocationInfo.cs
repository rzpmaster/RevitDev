using Log4NetDemo.Util;
using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace Log4NetDemo.Core.Data
{
    /// <summary>
    /// 表示调用者的堆栈信息的类
    /// </summary>
    /// <remarks>
    /// <para>release版本,可能会不准确</para>
    /// </remarks>
    [Serializable]
    public class LocationInfo
    {
        public LocationInfo(Type callerStackBoundaryDeclaringType)
        {
            // Initialize all fields
            m_className = NA;
            m_fileName = NA;
            m_lineNumber = NA;
            m_methodName = NA;
            m_fullInfo = NA;

            if (callerStackBoundaryDeclaringType != null)
            {
                try
                {
                    StackTrace st = new StackTrace(true);
                    int frameIndex = 0;

                    // skip frames not from fqnOfCallingClass
                    while (frameIndex < st.FrameCount)
                    {
                        StackFrame frame = st.GetFrame(frameIndex);
                        if (frame != null && frame.GetMethod().DeclaringType == callerStackBoundaryDeclaringType)
                        {
                            break;
                        }
                        frameIndex++;
                    }

                    // skip frames from fqnOfCallingClass
                    while (frameIndex < st.FrameCount)
                    {
                        StackFrame frame = st.GetFrame(frameIndex);
                        if (frame != null && frame.GetMethod().DeclaringType != callerStackBoundaryDeclaringType)
                        {
                            break;
                        }
                        frameIndex++;
                    }

                    if (frameIndex < st.FrameCount)
                    {
                        // take into account the frames we skip above
                        int adjustedFrameCount = st.FrameCount - frameIndex;
                        ArrayList stackFramesList = new ArrayList(adjustedFrameCount);
                        m_stackFrames = new StackFrameItem[adjustedFrameCount];
                        for (int i = frameIndex; i < st.FrameCount; i++)
                        {
                            stackFramesList.Add(new StackFrameItem(st.GetFrame(i)));
                        }

                        stackFramesList.CopyTo(m_stackFrames, 0);

                        // now frameIndex is the first 'user' caller frame
                        StackFrame locationFrame = st.GetFrame(frameIndex);

                        if (locationFrame != null)
                        {
                            System.Reflection.MethodBase method = locationFrame.GetMethod();

                            if (method != null)
                            {
                                m_methodName = method.Name;
                                if (method.DeclaringType != null)
                                {
                                    m_className = method.DeclaringType.FullName;
                                }
                            }
                            m_fileName = locationFrame.GetFileName();
                            m_lineNumber = locationFrame.GetFileLineNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);

                            // Combine all location info
                            m_fullInfo = m_className + '.' + m_methodName + '(' + m_fileName + ':' + m_lineNumber + ')';
                        }
                    }
                }
                catch (System.Security.SecurityException)
                {
                    // This security exception will occur if the caller does not have 
                    // some undefined set of SecurityPermission flags.
                    LogLog.Debug(declaringType, "Security exception while trying to get caller stack frame. Error Ignored. Location Information Not Available.");
                }
            }
        }

        public LocationInfo(string className, string methodName, string fileName, string lineNumber)
        {
            m_className = className;
            m_fileName = fileName;
            m_lineNumber = lineNumber;
            m_methodName = methodName;
            m_fullInfo = m_className + '.' + m_methodName + '(' + m_fileName +
                ':' + m_lineNumber + ')';
        }

        public string ClassName
        {
            get { return m_className; }
        }

        public string FileName
        {
            get { return m_fileName; }
        }

        public string LineNumber
        {
            get { return m_lineNumber; }
        }

        public string MethodName
        {
            get { return m_methodName; }
        }

        public string FullInfo
        {
            get { return m_fullInfo; }
        }

        /// <summary>
        /// 表示在调用类型中的所有堆栈信息数组
        /// </summary>
        public StackFrameItem[] StackFrames
        {
            get { return m_stackFrames; }
        }

        private readonly string m_className;
        private readonly string m_fileName;
        private readonly string m_lineNumber;
        private readonly string m_methodName;
        private readonly string m_fullInfo;
        private readonly StackFrameItem[] m_stackFrames;

        private readonly static Type declaringType = typeof(LocationInfo);
        private const string NA = "?";
    }

    /// <summary>
    /// 提供 System.Diagnostics.StackFrame 中的信息,而不依赖,因为前者需要加载程序集
    /// </summary>
    [Serializable]
    public class StackFrameItem
    {
        public StackFrameItem(StackFrame frame)
        {
            // set default values
            m_lineNumber = NA;
            m_fileName = NA;
            m_method = new MethodItem();
            m_className = NA;

            try
            {
                // get frame values
                m_lineNumber = frame.GetFileLineNumber().ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                m_fileName = frame.GetFileName();
                // get method values
                MethodBase method = frame.GetMethod();
                if (method != null)
                {
                    if (method.DeclaringType != null)
                        m_className = method.DeclaringType.FullName;
                    m_method = new MethodItem(method);
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "An exception ocurred while retreiving stack frame information.", ex);
            }

            // set full info
            m_fullInfo = m_className + '.' + m_method.Name + '(' + m_fileName + ':' + m_lineNumber + ')';
        }

        public string ClassName
        {
            get { return m_className; }
        }

        public string FileName
        {
            get { return m_fileName; }
        }

        public string LineNumber
        {
            get { return m_lineNumber; }
        }

        public MethodItem Method
        {
            get { return m_method; }
        }

        public string FullInfo
        {
            get { return m_fullInfo; }
        }

        private readonly string m_lineNumber;
        private readonly string m_fileName;
        private readonly string m_className;
        private readonly string m_fullInfo;
        private readonly MethodItem m_method;

        private readonly static Type declaringType = typeof(StackFrameItem);
        private const string NA = "?";
    }

    [Serializable]
    public class MethodItem
    {
        public MethodItem()
        {
            m_name = NA;
            m_parameters = new string[0];
        }

        public MethodItem(string name)
            : this()
        {
            m_name = name;
        }

        public MethodItem(string name, string[] parameters)
            : this(name)
        {
            m_parameters = parameters;
        }

        public MethodItem(System.Reflection.MethodBase methodBase)
            : this(methodBase.Name, GetMethodParameterNames(methodBase))
        {
        }

        private static string[] GetMethodParameterNames(System.Reflection.MethodBase methodBase)
        {
            ArrayList methodParameterNames = new ArrayList();
            try
            {
                System.Reflection.ParameterInfo[] methodBaseGetParameters = methodBase.GetParameters();

                int methodBaseGetParametersCount = methodBaseGetParameters.GetUpperBound(0);

                for (int i = 0; i <= methodBaseGetParametersCount; i++)
                {
                    methodParameterNames.Add(methodBaseGetParameters[i].ParameterType + " " + methodBaseGetParameters[i].Name);
                }
            }
            catch (Exception ex)
            {
                LogLog.Error(declaringType, "An exception ocurred while retreiving method parameters.", ex);
            }

            return (string[])methodParameterNames.ToArray(typeof(string));
        }

        public string Name
        {
            get { return m_name; }
        }

        public string[] Parameters
        {
            get { return m_parameters; }
        }

        private readonly string m_name;
        private readonly string[] m_parameters;

        private readonly static Type declaringType = typeof(StackFrameItem);
        private const string NA = "?";
    }

}
