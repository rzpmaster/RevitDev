using Log4NetDemo.Core.Interface;

namespace Log4NetDemo.Core.Data
{
    public abstract class LevelMappingEntry : IOptionHandler
    {
        private Level m_level;

        protected LevelMappingEntry()
        {
        }

        public Level Level
        {
            get { return m_level; }
            set { m_level = value; }
        }

        #region IOptionHandler Members

        virtual public void ActivateOptions()
        {
            // default implementation is to do nothing
        }

        #endregion
    }
}
