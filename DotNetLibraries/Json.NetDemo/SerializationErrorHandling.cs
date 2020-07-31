using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Json.NetDemo
{
    class SerializationErrorHandling
    {
        string jsonStr = @"[
                  '2009-09-09T00:00:00Z',
                  'I am not a date and will error!',
                  [
                    1
                  ],
                  '1977-02-20T00:00:00Z',
                  null,
                  '2000-12-01T00:00:00Z'
                ]";

        // Serialization Error Handling

        public void Test()
        {
            List<string> errors = new List<string>();

            List<DateTime> c = JsonConvert.DeserializeObject<List<DateTime>>(jsonStr,
                new JsonSerializerSettings
                {
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;   // 标记错误已经被处理
                    },
                    Converters = { new IsoDateTimeConverter() }
                });

            // 2009-09-09T00:00:00Z
            // 1977-02-20T00:00:00Z
            // 2000-12-01T00:00:00Z

            // The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
            // Unexpected token parsing date. Expected String, got StartArray.
            // Cannot convert null value to System.DateTime.
        }

        // Parent Error Handling

        public void Test2()
        {
            List<string> errors = new List<string>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Error += delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
                // 只能被捕获一次~~
                // only log an error once

                // OriginalObject是引发错误的对象，而CurrentObject是引发事件的对象。
                // 它们仅等于第一次针对OriginalObject引发事件
                if (args.CurrentObject == args.ErrorContext.OriginalObject)
                {
                    errors.Add(args.ErrorContext.Error.Message);
                }
            };

            List<DateTime> c = null;
            using (StringReader sr = new StringReader(jsonStr))
            using (JsonReader jr = new JsonTextReader(sr))
            {
                c = serializer.Deserialize<List<DateTime>>(jr);
            }
        }
    }

    // Serialization Error Handling Attribut

    public class PersonError
    {
        private List<string> _roles;

        public string Name { get; set; }
        public int Age { get; set; }

        public List<string> Roles
        {
            get
            {
                if (_roles == null)
                {
                    throw new Exception("Roles not loaded!");
                }

                return _roles;
            }
            set { _roles = value; }
        }

        public string Title { get; set; }

        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }
    }
}
