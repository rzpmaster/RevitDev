using System.Collections;
using System.IO;

namespace Log4NetDemo.Layout.PatternStringConverters
{
    internal sealed class AppSettingPatternConverter : PatternConverter
    {
        private static Hashtable _appSettingsHashTable;
        private static IDictionary AppSettingsDictionary
        {
            get
            {
                if (_appSettingsHashTable == null)
                {
                    Hashtable h = new Hashtable();
                    foreach (string key in System.Configuration.ConfigurationManager.AppSettings)
                    {
                        h.Add(key, System.Configuration.ConfigurationManager.AppSettings[key]);
                    }
                    _appSettingsHashTable = h;
                }
                return _appSettingsHashTable;
            }

        }

        protected override void Convert(TextWriter writer, object state)
        {
            if (Option != null)
            {
                // Write the value for the specified key
                WriteObject(writer, null, System.Configuration.ConfigurationManager.AppSettings[Option]);
            }
            else
            {
                // Write all the key value pairs
                WriteDictionary(writer, null, AppSettingsDictionary);
            }
        }
    }
}
