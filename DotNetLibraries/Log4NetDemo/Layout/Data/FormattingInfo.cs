namespace Log4NetDemo.Layout.Data
{
    public class FormattingInfo
    {
        public FormattingInfo()
        {
        }

        public FormattingInfo(int min, int max, bool leftAlign)
        {
            m_min = min;
            m_max = max;
            m_leftAlign = leftAlign;
        }

        public int Min
        {
            get { return m_min; }
            set { m_min = value; }
        }

        public int Max
        {
            get { return m_max; }
            set { m_max = value; }
        }

        public bool LeftAlign
        {
            get { return m_leftAlign; }
            set { m_leftAlign = value; }
        }

        private int m_min = -1;
        private int m_max = int.MaxValue;
        private bool m_leftAlign = false;
    }
}
