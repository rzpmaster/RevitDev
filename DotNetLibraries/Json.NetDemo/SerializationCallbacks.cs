using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Json.NetDemo
{
    // Serialization Callback Attributes

    public class SerializationEventTestObject
    {
        // 2222
        // This member is serialized and deserialized with no change.
        public int Member1 { get; set; }

        // The value of this field is set and reset during and 
        // after serialization.
        public string Member2 { get; set; }

        // This field is not serialized. The OnDeserializedAttribute 
        // is used to set the member value after serialization.
        [JsonIgnore]
        public string Member3 { get; set; }

        // This field is set to null, but populated after deserialization.
        public string Member4 { get; set; }

        public SerializationEventTestObject()
        {
            Member1 = 11;
            Member2 = "Hello World!";
            Member3 = "This is a nonserialized value";
            Member4 = null;
        }


        /// 猜一下,
        /// 序列化的结果为:
        /// M1 = 11 
        /// M2 = "This value went into the data file during serialization."
        /// M4 = NULL
        /// 
        /// 序列化后原对象为
        /// M1 = 11 
        /// M2 = "This value was reset after serialization."
        /// Member3 = "This is a nonserialized value";
        /// M4 = NULL
        /// 
        /// 反序列化后
        /// M1 = 11
        /// M2 = "This value went into the data file during serialization."
        /// M4 = "This value was set after deserialization."
        /// 

        /// 反序列化猜错了
        /// 正确结果是 M3 也会被在反序列化之前设置值
        /// M1 = 11
        /// M2 = "This value went into the data file during serialization."
        /// Member3 = "This value was set during deserialization";
        /// M4 = "This value was set after deserialization."



        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            // 序列化之前改掉 M2 的值
            Member2 = "This value went into the data file during serialization.";
        }

        [OnSerialized]
        internal void OnSerializedMethod(StreamingContext context)
        {
            // 序列化之后 M2 的值 又被设置了一次(序列化的结果是上一个值)
            Member2 = "This value was reset after serialization.";
        }

        [OnDeserializing]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            // 这里解释了为啥 M3 会被赋值,就是因为在反序列化之前,就调用默认的无参构造函数,反序列化的过程其实就是读取json字符串赋值的过程

            Console.WriteLine(this.Member1);
            Console.WriteLine(this.Member2);
            Console.WriteLine(this.Member3);
            Console.WriteLine(this.Member4);


            // 反序列化前 改掉 M3
            Member3 = "This value was set during deserialization";
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // 反序列化后 改掉 M4
            Member4 = "This value was set after deserialization.";
        }
    }


}
