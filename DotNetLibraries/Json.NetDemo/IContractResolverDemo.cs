using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Json.NetDemo
{
    class IContractResolverDemo
    {
        public void Test()
        {
            Book book = new Book
            {
                BookName = "The Gathering Storm",
                BookPrice = 16.19m,
                AuthorName = "Brandon Sanderson",
                AuthorAge = 34,
                AuthorCountry = "United States of America"
            };

            string startingWithA = JsonConvert.SerializeObject(book, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('A') });

            // {
            //   "AuthorName": "Brandon Sanderson",
            //   "AuthorAge": 34,
            //   "AuthorCountry": "United States of America"
            // }

            string startingWithB = JsonConvert.SerializeObject(book, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('B') });

            // {
            //   "BookName": "The Gathering Storm",
            //   "BookPrice": 16.19
            // }
        }

        public void Test2()
        {
            Product product = new Product
            {
                ExpiryDate = new DateTime(2010, 12, 20, 18, 1, 0, DateTimeKind.Utc),
                Name = "Widget",
                Price = 9.99m,
                //Sizes = new[] { "Small", "Medium", "Large" }
            };

            string json =
                JsonConvert.SerializeObject(
                    product,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                    );

            //{
            //  "name": "Widget",
            //  "expiryDate": "2010-12-20T18:01:00Z",
            //  "price": 9.99,
            //  "sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}
        }
    }

    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly char _startingWithChar;

        public DynamicContractResolver(char startingWithChar)
        {
            _startingWithChar = startingWithChar;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // only serializer properties that start with the specified character
            properties =
                properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

            return properties;
        }
    }

    public class Book
    {
        public string BookName { get; set; }
        public decimal BookPrice { get; set; }
        public string AuthorName { get; set; }
        public int AuthorAge { get; set; }
        public string AuthorCountry { get; set; }
    }

    // ContractResolver 比较费时间，缓存起来只需要调用一次

    public class ConverterContractResolver : DefaultContractResolver
    {
        public new static readonly ConverterContractResolver Instance = new ConverterContractResolver();

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset))
            {
                contract.Converter = new JavaScriptDateTimeConverter();
            }

            return contract;
        }
    }

    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        public new static readonly ShouldSerializeContractResolver Instance = new ShouldSerializeContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(Employee2) && property.PropertyName == "Manager")
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        Employee2 e = (Employee2)instance;
                        return e.Manager != e;
                    };
            }

            return property;
        }
    }

    public class Employee2
    {
        public string Name { get; set; }
        public Employee2 Manager { get; set; }

        public bool ShouldSerializeManager()
        {
            // don't serialize the Manager property if an employee is their own manager
            return (Manager != this);
        }
    }
}
