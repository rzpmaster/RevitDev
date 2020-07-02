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
    public partial class SelectCircuitForm : Form
    {
        private CircuitOperationData optionData;

        public SelectCircuitForm()
        {
            InitializeComponent();
        }

        public SelectCircuitForm(CircuitOperationData optionData):this()
        {
            this.optionData = optionData;

            InitializeElectricalSystems();
        }

        private void InitializeElectricalSystems()
        {
            listBoxElectricalSystem.DataSource = optionData.ElectricalSystemItems;
            listBoxElectricalSystem.DisplayMember = "Name";
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            int index = listBoxElectricalSystem.SelectedIndex;
            optionData.SelectCircuit(index);
        }

        private void listBoxElectricalSystem_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBoxElectricalSystem.SelectedIndex;
            optionData.ShowCircuit(index);
        }
    }
}
