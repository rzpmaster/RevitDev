using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GridCreation.CreateForms
{
    public partial class CreateWithSelectedCurvesForm : Form
    {
        // data class object
        private CreateWithSelectedCurvesData m_data;

        public CreateWithSelectedCurvesForm(CreateWithSelectedCurvesData data)
        {
            m_data = data;

            InitializeComponent();
            // Set state of controls
            InitializeControls();
        }

        private void InitializeControls()
        {
            comboBoxBubbleLocation.SelectedIndex = 1;
        }


        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Check if input are validated
            if (ValidateValues())
            {
                // Transfer data back into data class
                SetData();
            }
            else
            {
                this.DialogResult = DialogResult.None;
            }
        }

        private bool ValidateValues()
        {
            return Validation.ValidateLabel(textBoxFirstLabel, m_data.LabelsList);
        }

        private void SetData()
        {
            m_data.BubbleLocation = (BubbleLocation)comboBoxBubbleLocation.SelectedIndex;
            m_data.FirstLabel = textBoxFirstLabel.Text;
            m_data.DeleteSelectedElements = checkBoxDeleteElements.Checked;
        }
    }
}
