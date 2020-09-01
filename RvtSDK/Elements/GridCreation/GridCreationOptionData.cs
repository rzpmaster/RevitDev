namespace GridCreation
{
    public class GridCreationOptionData
    {
        // The way to create grids
        private CreateMode m_createGridsMode;
        // If lines/arcs have been selected
        private bool m_hasSelectedLinesOrArcs;

        public GridCreationOptionData(bool hasSelectedLinesOrArcs)
        {
            m_hasSelectedLinesOrArcs = hasSelectedLinesOrArcs;
        }

        public CreateMode CreateGridsMode
        {
            get
            {
                return m_createGridsMode;
            }
            set
            {
                m_createGridsMode = value;
            }
        }

        public bool HasSelectedLinesOrArcs
        {
            get
            {
                return m_hasSelectedLinesOrArcs;
            }
        }
    }
}
