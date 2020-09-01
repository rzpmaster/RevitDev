using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GridCreation
{
    public partial class GridCreationOptionForm : Form
    {
        private GridCreationOptionData m_gridCreationOption;

        public GridCreationOptionForm(GridCreationOptionData opt)
        {
            m_gridCreationOption = opt;

            InitializeComponent();

            // Set state of controls
            InitializeControls();
        }

        /// <summary>
        /// Set state of controls
        /// </summary>
        private void InitializeControls()
        {
            if (!m_gridCreationOption.HasSelectedLinesOrArcs)
            {
                radioButtonSelect.Enabled = false;
                radioButtonOrthogonalGrids.Checked = true;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Transfer data back into data class
            SetData();
        }

        /// <summary>
        /// Transfer data back into data class
        /// </summary>
        private void SetData()
        {
            m_gridCreationOption.CreateGridsMode = radioButtonSelect.Checked ? CreateMode.Select :
                (radioButtonOrthogonalGrids.Checked ? CreateMode.Orthogonal : CreateMode.RadialAndArc);
        }
    }
}
