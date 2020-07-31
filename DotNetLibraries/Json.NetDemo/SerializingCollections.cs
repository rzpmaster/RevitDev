using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json.NetDemo
{
    class SerializingCollectionsDemo
    {
        private static string deskpath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        //Serializing and Deserializing JSON with JsonConvert

        public void JsonConvertDemo()
        {
            Product product = new Product();

            product.Name="Apple";
            product.ExpiryDate = new DateTime(2008, 12, 28);
            product.Price = 3.99M;
            product.SetSizes(new string[] { "Small", "Medium", "Large" });

            string output = JsonConvert.SerializeObject(product);
            //{
            //  "Name": "Apple",
            //  "ExpiryDate": "2008-12-28T00:00:00",
            //  "Price": 3.99,
            //  "Sizes": [
            //    "Small",
            //    "Medium",
            //    "Large"
            //  ]
            //}

            // 反序列化时默认调用无参的构造函数,如果属性的setter是public的,反序列化时回顺便把值写进去,否则显示默认值
            Product deserializedProduct = JsonConvert.DeserializeObject<Product>(output);
        }

        //Serializing JSON to a Stream with JsonSerializer

        public void JsonSerializerDemo()
        {
            Product product = new Product();
            product.ExpiryDate = new DateTime(2008, 12, 28);

            JsonSerializer serializer = new JsonSerializer();

            // 格式转化
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            // 忽略空值
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(Path.Combine(deskpath, "json.txt")))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, product);
                // {"ExpiryDate":new Date(1230375600000),"Price":0}
            }
        }

        //Serializing Collections

        public void SerializingCollections()
        {
            Product p1 = new Product
            {
                Name = "Product 1",
                Price = 99.95m,
                ExpiryDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
            };
            Product p2 = new Product
            {
                Name = "Product 2",
                Price = 12.50m,
                ExpiryDate = new DateTime(2009, 7, 31, 0, 0, 0, DateTimeKind.Utc),
            };

            List<Product> products = new List<Product>();
            products.Add(p1);
            products.Add(p2);

            string json = JsonConvert.SerializeObject(products, Formatting.Indented);
            //[
            //  {
            //    "Name": "Product 1",
            //    "ExpiryDate": "2000-12-29T00:00:00Z",
            //    "Price": 99.95,
            //    "Sizes": null
            //  },
            //  {
            //    "Name": "Product 2",
            //    "ExpiryDate": "2009-07-31T00:00:00Z",
            //    "Price": 12.50,
            //    "Sizes": null
            //  }
            //]
        }

        //Deserializing Collections

        public void DeserializingCollections()
        {
            string json = @"[
               {
                 'Name': 'Product 1',
                 'ExpiryDate': '2000-12-29T00:00Z',
                 'Price': 99.95,
                 'Sizes': null
               },
               {
                 'Name': 'Product 2',
                 'ExpiryDate': '2009-07-31T00:00Z',
                 'Price': 12.50,
                 'Sizes': null
               }
            ]";

            List<Product> products = JsonConvert.DeserializeObject<List<Product>>(json);

            Console.WriteLine(products.Count);
            // 2

            Product p1 = products[0];

            Console.WriteLine(p1.Name);
            // Product 1
        }

        //Deserializing Dictionaries

        public void DeserializingDictionaries()
        {
            string json = @"{""key1"":""value1"",""key2"":""value2""}";

            Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            Console.WriteLine(values.Count);
            // 2

            Console.WriteLine(values["key1"]);
            // value1
        }
    }

    class Product
    {
        public string Name { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Price { get; set; }
        public string[] Sizes { get; private set; }

        public void SetSizes(string[] sizes)
        {
            Sizes = new string[sizes.Length];
            for (int i = 0; i < sizes.Length; i++)
                Sizes[i] = sizes[i];
        }
    }
}
