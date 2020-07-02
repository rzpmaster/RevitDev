using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerCircuit
{
    public partial class EditCircuitForm : Form
    {
        private CircuitOperationData optionData;

        public EditCircuitForm()
        {
            InitializeComponent();
        }

        public EditCircuitForm(CircuitOperationData optionData):this()
        {
            this.optionData = optionData;

            AddToolTips();
        }

        private void AddToolTips()
        {
            toolTip.SetToolTip(buttonAdd, "tipAddToCircuit");
            toolTip.SetToolTip(buttonRemove, "tipRemoveFromCircuit");
            toolTip.SetToolTip(buttonSelectPanel, "tipSelectPanel");
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            optionData.EditOption = EditOption.Add;
            this.Close();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            optionData.EditOption = EditOption.Remove;
            this.Close();
        }

        private void buttonSelectPanel_Click(object sender, EventArgs e)
        {
            optionData.EditOption = EditOption.SelectPanel;
            this.Close();
        }
    }
}
