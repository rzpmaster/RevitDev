using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json.NetDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            #region Serializing Collections
            //SerializingCollectionsDemo demo = new SerializingCollectionsDemo();
            ////demo.JsonConvertDemo();
            ////demo.JsonSerializerDemo();
            //demo.SerializingCollections();
            //demo.DeserializingCollections();
            //demo.DeserializingDictionaries();
            #endregion


            #region SerializationAttributes
            //string json = @"{
            //  'DisplayName': 'John Smith',
            //  'SAMAccountName': 'contoso\\johns'
            //}";

            //DirectoryAccount account = JsonConvert.DeserializeObject<DirectoryAccount>(json);

            //Console.WriteLine(account.DisplayName);
            //// John Smith

            //Console.WriteLine(account.Domain);
            //// contoso

            //Console.WriteLine(account.UserName);
            //// johns

            //string json = @"{
            //  ""UserName"": ""domain\\username"",
            //  ""Enabled"": true
            //}";

            //// 指定了反序列化时使用的构造函数,哪怕他是私有的,也可以使用构造器初始化对象
            //User2 user = JsonConvert.DeserializeObject<User2>(json);

            //Console.WriteLine(user.UserName);
            //// domain\username 

            #endregion


            #region SerializationCallbacks
            //SerializationEventTestObject obj = new SerializationEventTestObject();

            //Console.WriteLine(obj.Member1);
            //// 11
            //Console.WriteLine(obj.Member2);
            //// Hello World!
            //Console.WriteLine(obj.Member3);
            //// This is a nonserialized value
            //Console.WriteLine(obj.Member4);
            //// null

            //string json = JsonConvert.SerializeObject(obj, Formatting.Indented);    // Formatting.Indented 缩进 
            //// {
            ////   "Member1": 11,
            ////   "Member2": "This value went into the data file during serialization.",
            ////   "Member4": null
            //// }

            //Console.WriteLine(obj.Member1);
            //// 11
            //Console.WriteLine(obj.Member2);
            //// This value was reset after serialization.
            //Console.WriteLine(obj.Member3);
            //// This is a nonserialized value
            //Console.WriteLine(obj.Member4);
            //// null

            //obj = JsonConvert.DeserializeObject<SerializationEventTestObject>(json);

            //Console.WriteLine(obj.Member1);
            //// 11
            //Console.WriteLine(obj.Member2);
            //// This value went into the data file during serialization.
            //Console.WriteLine(obj.Member3);
            //// This value was set during deserialization
            //Console.WriteLine(obj.Member4);
            //// This value was set after deserialization.
            #endregion


            #region SerializationErrorHandling
            //SerializationErrorHandling serializationErrorHandling = new SerializationErrorHandling();

            //serializationErrorHandling.Test();
            //serializationErrorHandling.Test2();

            //PersonError person = new PersonError
            //{
            //    Name = "George Michael Bluth",
            //    Age = 16,
            //    Roles = null,
            //    Title = "Mister Manager"
            //};

            //string json = JsonConvert.SerializeObject(person, Formatting.Indented);

            //Console.WriteLine(json);
            ////{
            ////  "Name": "George Michael Bluth",
            ////  "Age": 16,
            ////  "Title": "Mister Manager"
            ////}
            #endregion


            #region PreservingObjectReferences
            //PreservingObjectReferences preservingObjectReferences = new PreservingObjectReferences();

            //preservingObjectReferences.Test();
            //preservingObjectReferences.Test2();

            #endregion


            #region CustomCreationConverter
            //CustomCreationConverterDemo customCreationConverterDemo = new CustomCreationConverterDemo();

            //customCreationConverterDemo.Test();
            #endregion


            #region IContractResolverDemo
            //IContractResolverDemo contractResolverDemo = new IContractResolverDemo();

            //contractResolverDemo.Test();
            #endregion


            #region Linq2JsonDemo
            //Linq2JsonDemo linq2JsonDemo = new Linq2JsonDemo();

            //linq2JsonDemo.Test();
            #endregion


            #region DebuggingWithSerializationTracing
            DebuggingWithSerializationTracing debuggingWithSerializationTracing = new DebuggingWithSerializationTracing();
            debuggingWithSerializationTracing.Test();
            #endregion


            Console.ReadKey();
        }
    }
}
