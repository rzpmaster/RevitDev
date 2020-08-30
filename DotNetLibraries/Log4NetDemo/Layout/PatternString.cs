using Log4NetDemo.Core.Interface;
using Log4NetDemo.Layout.Data;
using Log4NetDemo.Layout.PatternStringConverters;
using System;
using System.Collections;
using System.IO;

namespace Log4NetDemo.Layout
{
    public class PatternString : IOptionHandler
    {
        public PatternString()
        {
        }

        public PatternString(string pattern)
        {
            m_pattern = pattern;
            ActivateOptions();
        }

        #region Static Constructor

        private static Hashtable s_globalRulesRegistry;

        static PatternString()
        {
            s_globalRulesRegistry = new Hashtable(18);

            s_globalRulesRegistry.Add("appdomain", typeof(AppDomainPatternConverter));
            s_globalRulesRegistry.Add("date", typeof(DatePatternConverter));

            s_globalRulesRegistry.Add("env", typeof(EnvironmentPatternConverter));
            s_globalRulesRegistry.Add("envFolderPath", typeof(EnvironmentFolderPathPatternConverter));

            s_globalRulesRegistry.Add("identity", typeof(IdentityPatternConverter));
            s_globalRulesRegistry.Add("literal", typeof(LiteralPatternConverter));
            s_globalRulesRegistry.Add("newline", typeof(NewLinePatternConverter));
            s_globalRulesRegistry.Add("processid", typeof(ProcessIdPatternConverter));
            s_globalRulesRegistry.Add("property", typeof(PropertyPatternConverter));
            s_globalRulesRegistry.Add("random", typeof(RandomStringPatternConverter));
            s_globalRulesRegistry.Add("username", typeof(UserNamePatternConverter));

            s_globalRulesRegistry.Add("utcdate", typeof(UtcDatePatternConverter));
            s_globalRulesRegistry.Add("utcDate", typeof(UtcDatePatternConverter));
            s_globalRulesRegistry.Add("UtcDate", typeof(UtcDatePatternConverter));

            s_globalRulesRegistry.Add("appsetting", typeof(AppSettingPatternConverter));
            s_globalRulesRegistry.Add("appSetting", typeof(AppSettingPatternConverter));
            s_globalRulesRegistry.Add("AppSetting", typeof(AppSettingPatternConverter));
        }

        #endregion

        public string ConversionPattern
        {
            get { return m_pattern; }
            set { m_pattern = value; }
        }

        #region Implementation of IOptionHandler

        virtual public void ActivateOptions()
        {
            m_head = CreatePatternParser(m_pattern).Parse();
        }

        private PatternParser CreatePatternParser(string pattern)
        {
            PatternParser patternParser = new PatternParser(pattern);

            // Add all the builtin patterns
            foreach (DictionaryEntry entry in s_globalRulesRegistry)
            {
                ConverterInfo converterInfo = new ConverterInfo();
                converterInfo.Name = (string)entry.Key;
                converterInfo.Type = (Type)entry.Value;
                patternParser.PatternConverters.Add(entry.Key, converterInfo);
            }
            // Add the instance patterns
            foreach (DictionaryEntry entry in m_instanceRulesRegistry)
            {
                patternParser.PatternConverters[entry.Key] = entry.Value;
            }

            return patternParser;
        }

        #endregion

        #region Format

        public void Format(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            PatternConverter c = m_head;

            // loop through the chain of pattern converters
            while (c != null)
            {
                c.Format(writer, null);
                c = c.Next;
            }
        }

        public string Format()
        {
            StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            Format(writer);
            return writer.ToString();
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
