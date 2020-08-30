using Log4NetDemo.Core.Data;
using Log4NetDemo.Layout.Data;
using Log4NetDemo.Layout.PatternConverters;
using System;
using System.Collections;
using System.IO;

namespace Log4NetDemo.Layout
{
    /// <summary>
    /// 
    /// </summary>
    public class PatternLayout : LayoutSkeleton
    {
        public const string DetailConversionPattern = "%timestamp [%thread] %level %logger %ndc - %message%newline";
        public const string DefaultConversionPattern = "%message%newline";

        public PatternLayout() : this(DefaultConversionPattern)
        {
        }

        public PatternLayout(string pattern)
        {
            // By default we do not process the exception
            IgnoresException = true;

            m_pattern = pattern;
            if (m_pattern == null)
            {
                m_pattern = DefaultConversionPattern;
            }

            ActivateOptions();
        }

        #region Static Constructor

        private static Hashtable s_globalRulesRegistry;

        static PatternLayout()
        {
            s_globalRulesRegistry = new Hashtable(45);

            s_globalRulesRegistry.Add("literal", typeof(PatternStringConverters.LiteralPatternConverter));
            s_globalRulesRegistry.Add("newline", typeof(PatternStringConverters.NewLinePatternConverter));
            s_globalRulesRegistry.Add("n", typeof(PatternStringConverters.NewLinePatternConverter));

            s_globalRulesRegistry.Add("c", typeof(LoggerPatternConverter));
            s_globalRulesRegistry.Add("logger", typeof(LoggerPatternConverter));

            s_globalRulesRegistry.Add("C", typeof(TypeNamePatternConverter));
            s_globalRulesRegistry.Add("class", typeof(TypeNamePatternConverter));
            s_globalRulesRegistry.Add("type", typeof(TypeNamePatternConverter));

            s_globalRulesRegistry.Add("d", typeof(DatePatternConverter));
            s_globalRulesRegistry.Add("date", typeof(DatePatternConverter));

            s_globalRulesRegistry.Add("exception", typeof(ExceptionPatternConverter));

            s_globalRulesRegistry.Add("F", typeof(FileLocationPatternConverter));
            s_globalRulesRegistry.Add("file", typeof(FileLocationPatternConverter));

            s_globalRulesRegistry.Add("l", typeof(FullLocationPatternConverter));
            s_globalRulesRegistry.Add("location", typeof(FullLocationPatternConverter));

            s_globalRulesRegistry.Add("L", typeof(LineLocationPatternConverter));
            s_globalRulesRegistry.Add("line", typeof(LineLocationPatternConverter));

            s_globalRulesRegistry.Add("m", typeof(MessagePatternConverter));
            s_globalRulesRegistry.Add("message", typeof(MessagePatternConverter));

            s_globalRulesRegistry.Add("M", typeof(MethodLocationPatternConverter));
            s_globalRulesRegistry.Add("method", typeof(MethodLocationPatternConverter));

            s_globalRulesRegistry.Add("p", typeof(LevelPatternConverter));
            s_globalRulesRegistry.Add("level", typeof(LevelPatternConverter));

            s_globalRulesRegistry.Add("P", typeof(PropertyPatternConverter));
            s_globalRulesRegistry.Add("property", typeof(PropertyPatternConverter));
            s_globalRulesRegistry.Add("properties", typeof(PropertyPatternConverter));

            s_globalRulesRegistry.Add("r", typeof(RelativeTimePatternConverter));
            s_globalRulesRegistry.Add("timestamp", typeof(RelativeTimePatternConverter));

            s_globalRulesRegistry.Add("stacktrace", typeof(StackTracePatternConverter));
            s_globalRulesRegistry.Add("stacktracedetail", typeof(StackTraceDetailPatternConverter));

            s_globalRulesRegistry.Add("t", typeof(ThreadPatternConverter));
            s_globalRulesRegistry.Add("thread", typeof(ThreadPatternConverter));

            //// For backwards compatibility the NDC patterns
            //s_globalRulesRegistry.Add("x", typeof(NdcPatternConverter));
            //s_globalRulesRegistry.Add("ndc", typeof(NdcPatternConverter));

            //// For backwards compatibility the MDC patterns just do a property lookup
            //s_globalRulesRegistry.Add("X", typeof(PropertyPatternConverter));
            //s_globalRulesRegistry.Add("mdc", typeof(PropertyPatternConverter));

            s_globalRulesRegistry.Add("a", typeof(AppDomainPatternConverter));
            s_globalRulesRegistry.Add("appdomain", typeof(AppDomainPatternConverter));

            s_globalRulesRegistry.Add("u", typeof(IdentityPatternConverter));
            s_globalRulesRegistry.Add("identity", typeof(IdentityPatternConverter));

            s_globalRulesRegistry.Add("utcdate", typeof(UtcDatePatternConverter));
            s_globalRulesRegistry.Add("utcDate", typeof(UtcDatePatternConverter));
            s_globalRulesRegistry.Add("UtcDate", typeof(UtcDatePatternConverter));

            s_globalRulesRegistry.Add("w", typeof(UserNamePatternConverter));
            s_globalRulesRegistry.Add("username", typeof(UserNamePatternConverter));
        }

        #endregion

        public string ConversionPattern
        {
            get { return m_pattern; }
            set { m_pattern = value; }
        }

        #region Implementation of IOptionHandler

        override public void ActivateOptions()
        {
            m_head = CreatePatternParser(m_pattern).Parse();

            PatternConverter curConverter = m_head;
            while (curConverter != null)
            {
                PatternLayoutConverter layoutConverter = curConverter as PatternLayoutConverter;
                if (layoutConverter != null)
                {
                    if (!layoutConverter.IgnoresException)
                    {
                        // Found converter that handles the exception
                        this.IgnoresException = false;

                        break;
                    }
                }
                curConverter = curConverter.Next;
            }
        }

        virtual protected PatternParser CreatePatternParser(string pattern)
        {
            PatternParser patternParser = new PatternParser(pattern);

            // Add all the builtin patterns
            foreach (DictionaryEntry entry in s_globalRulesRegistry)
            {
                ConverterInfo converterInfo = new ConverterInfo();
                converterInfo.Name = (string)entry.Key;
                converterInfo.Type = (Type)entry.Value;
                patternParser.PatternConverters[entry.Key] = converterInfo;
            }
            // Add the instance patterns
            foreach (DictionaryEntry entry in m_instanceRulesRegistry)
            {
                patternParser.PatternConverters[entry.Key] = entry.Value;
            }

            return patternParser;
        }

        #endregion

        #region Override implementation of LayoutSkeleton

        override public void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            PatternConverter c = m_head;

            // loop through the chain of pattern converters
            while (c != null)
            {
                c.Format(writer, loggingEvent);
                c = c.Next;
            }
        }

        #endregion

        public void AddConverter(ConverterInfo converterInfo)
        {
            if (converterInfo == null) throw new ArgumentNullException("converterInfo");

            if (!typeof(PatternConverter).IsAssignableFrom(converterInfo.Type))
            {
                throw new ArgumentException("The converter type specified [" + converterInfo.Type + "] must be a subclass of log4net.Util.PatternConverter", "converterInfo");
            }
            m_instanceRulesRegistry[converterInfo.Name] = converterInfo;
        }

        public void AddConverter(string name, Type type)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");

            ConverterInfo converterInfo = new ConverterInfo();
            converterInfo.Name = name;
            converterInfo.Type = type;

            AddConverter(converterInfo);
        }


        /// <summary>
		/// the pattern
		/// </summary>
		private string m_pattern;

        /// <summary>
        /// the head of the pattern converter chain
        /// </summary>
        private PatternConverter m_head;

        /// <summary>
        /// patterns defined on this PatternLayout only
        /// </summary>
        private Hashtable m_instanceRulesRegistry = new Hashtable();
    }
}
