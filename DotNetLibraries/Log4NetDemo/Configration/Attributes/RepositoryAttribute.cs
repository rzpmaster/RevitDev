using System;

namespace Log4NetDemo.Configration.Attributes
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Assembly)]
    public class RepositoryAttribute : Attribute
    {
        public RepositoryAttribute()
        {
        }

        public RepositoryAttribute(string name)
        {
            m_name = name;
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public Type RepositoryType
        {
            get { return m_repositoryType; }
            set { m_repositoryType = value; }
        }

        private string m_name = null;
        private Type m_repositoryType = null;
    }
}
