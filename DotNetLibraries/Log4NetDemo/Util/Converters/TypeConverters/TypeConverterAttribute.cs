using System;

namespace Log4NetDemo.Util.Converters.TypeConverters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum)]
    public sealed class TypeConverterAttribute : Attribute
    {
        private string m_typeName = null;

        public TypeConverterAttribute()
        {
        }

        public TypeConverterAttribute(string typeName)
        {
            m_typeName = typeName;
        }

        public TypeConverterAttribute(Type converterType)
        {
            m_typeName = SystemInfo.AssemblyQualifiedName(converterType);
        }

        public string ConverterTypeName
        {
            get { return m_typeName; }
            set { m_typeName = value; }
        }
    }
}
