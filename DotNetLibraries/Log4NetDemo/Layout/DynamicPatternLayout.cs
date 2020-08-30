namespace Log4NetDemo.Layout
{
    public class DynamicPatternLayout : PatternLayout
    {
        public DynamicPatternLayout()
            : base()
        {
        }

        public DynamicPatternLayout(string pattern)
            : base(pattern)
        {
        }

        #region Override implementation of LayoutSkeleton

        public override string Header
        {
            get
            {
                return m_headerPatternString.Format();
            }
            set
            {
                base.Header = value;
                m_headerPatternString = new PatternString(value);
            }
        }

        public override string Footer
        {
            get
            {
                return m_footerPatternString.Format();
            }
            set
            {
                base.Footer = value;
                m_footerPatternString = new PatternString(value);
            }
        }

        #endregion

        private PatternString m_headerPatternString = new PatternString("");
        private PatternString m_footerPatternString = new PatternString("");
    }
}
