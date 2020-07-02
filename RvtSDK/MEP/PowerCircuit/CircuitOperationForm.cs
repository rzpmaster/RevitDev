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
    public partial class CircuitOperationForm : Form
    {
        private CircuitOperationData m_optionData;

        private CircuitOperationForm()
        {
            InitializeComponent();
        }

        public CircuitOperationForm(CircuitOperationData optionData) : this()
        {
            this.m_optionData = optionData;

            InitializeButtons();
            AddToolTips();
        }

        private void InitializeButtons()
        {
            buttonCreate.Enabled = m_optionData.CanCreateCircuit;
            buttonEdit.Enabled = m_optionData.HasCircuit;
            buttonSelectPanel.Enabled = m_optionData.HasCircuit;
            buttonDisconnectPanel.Enabled = m_optionData.HasPanel;
        }

        private void AddToolTips()
        {
            toolTip.SetToolTip(buttonCreate, "tipCreateCircuit");
            toolTip.SetToolTip(buttonEdit, "tipEditCircuit");
            toolTip.SetToolTip(buttonSelectPanel, "tipSelectPanel");
            toolTip.SetToolTip(buttonDisconnectPanel, "tipDisconnectPanel");
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            m_optionData.Operation = Operation.CreateCircuit;
            this.Close();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            m_optionData.Operation = Operation.EditCircuit;
            this.Close();
        }

        private void buttonSelectPanel_Click(object sender, EventArgs e)
        {
            m_optionData.Operation = Operation.SelectPanel;
            this.Close();
        }

        private void buttonDisconnectPanel_Click(object sender, EventArgs e)
        {
            m_optionData.Operation = Operation.DisconnectPanel;
            this.Close();
        }
    }
}
