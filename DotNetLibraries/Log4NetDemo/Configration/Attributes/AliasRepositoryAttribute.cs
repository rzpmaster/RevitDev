using System;

namespace Log4NetDemo.Configration.Attributes
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class AliasRepositoryAttribute : Attribute
    {
        public AliasRepositoryAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Repository 的别名
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private string m_name = null;
    }
}
