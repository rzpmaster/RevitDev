using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json.NetDemo
{
    class PreservingObjectReferences
    {
        List<Person> people;
        public PreservingObjectReferences()
        {
            Person p = new Person
            {
                BirthDate = new DateTime(1980, 12, 23, 0, 0, 0, DateTimeKind.Utc),
                LastModified = new DateTime(2009, 2, 20, 12, 59, 21, DateTimeKind.Utc),
                Name = "James"
            };

            this.people = new List<Person>();
            people.Add(p);
            people.Add(p);
        }

        // Preserve Object References Off

        public void Test()
        {
            string json = JsonConvert.SerializeObject(people, Formatting.Indented);
            //[
            //  {
            //    "Name": "James",
            //    "BirthDate": "1980-12-23T00:00:00Z",
            //    "LastModified": "2009-02-20T12:59:21Z"
            //  },
            //  {
            //    "Name": "James",
            //    "BirthDate": "1980-12-23T00:00:00Z",
            //    "LastModified": "2009-02-20T12:59:21Z"
            //  }
            //]
        }

        // Preserve Object References On

        public void Test2()
        {
            string json = JsonConvert.SerializeObject(people, Formatting.Indented,
    new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

            //[
            //  {
            //    "$id": "1",
            //    "Name": "James",
            //    "BirthDate": "1983-03-08T00:00Z",
            //    "LastModified": "2012-03-21T05:40Z"
            //  },
            //  {
            //    "$ref": "1"
            //  }
            //]

            List<Person> deserializedPeople = JsonConvert.DeserializeObject<List<Person>>(json,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });

            Console.WriteLine(deserializedPeople.Count);
            // 2

            Person p1 = deserializedPeople[0];
            Person p2 = deserializedPeople[1];

            Console.WriteLine(p1.Name);
            // James
            Console.WriteLine(p2.Name);
            // James

            bool equal = Object.ReferenceEquals(p1, p2);
            // true

            /// 保留对象引用只能用于启用默认无参的构造函数的对象
        }
    }
}
