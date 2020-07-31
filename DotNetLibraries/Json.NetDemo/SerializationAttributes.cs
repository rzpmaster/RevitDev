using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Json.NetDemo
{
    // Serialization Attributes Example

    [JsonObject(MemberSerialization.OptIn)]
    class Person
    {
        // "John Smith"
        [JsonProperty]
        public string Name { get; set; }

        // "2000-12-15T22:11:03"
        [JsonProperty]
        public DateTime BirthDate { get; set; }

        // new Date(976918263055)
        [JsonProperty]
        public DateTime LastModified { get; set; }

        // not serialized because mode is opt-in
        public string Department { get; set; }
    }

    // JsonConverterAttribute Property Example

    public enum UserStatus
    {
        NotConfirmed,
        Active,
        Deleted
    }

    public class User
    {
        public string UserName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UserStatus Status { get; set; }
    }

    // JsonExtensionDataAttribute Property Example

    public class DirectoryAccount
    {
        // normal deserialization
        public string DisplayName { get; set; }

        // these properties are set in OnDeserialized
        public string UserName { get; set; }
        public string Domain { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // 没有匹配上的数据放在这里
            // SAMAccountName is not deserialized to any property
            // and so it is added to the extension data dictionary
            string samAccountName = (string)_additionalData["SAMAccountName"];

            Domain = samAccountName.Split('\\')[0];
            UserName = samAccountName.Split('\\')[1];
        }

        public DirectoryAccount()
        {
            _additionalData = new Dictionary<string, JToken>();
        }
    }

    // JsonConstructorAttribute 

    public class User2
    {
        public string UserName { get; private set; }
        public bool Enabled { get; private set; }

        public User2()
        {
        }

        [JsonConstructor]
        private User2(string userName, bool enabled)
        {
            UserName = userName;
            Enabled = enabled;
        }
    }
}
