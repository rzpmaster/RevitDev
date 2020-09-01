namespace GridCreation.CreateForms
{
    partial class CreateWithSelectedCurvesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelBubbleLocation = new System.Windows.Forms.Label();
            this.textBoxFirstLabel = new System.Windows.Forms.TextBox();
            this.labelFirstLabel = new System.Windows.Forms.Label();
            this.comboBoxBubbleLocation = new System.Windows.Forms.ComboBox();
            this.groupBoxGridSettings = new System.Windows.Forms.GroupBox();
            this.checkBoxDeleteElements = new System.Windows.Forms.CheckBox();
            this.groupBoxGridSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(306, 164);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(120, 27);
            this.buttonCancel.TabIndex = 20;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(165, 164);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(120, 27);
            this.buttonOK.TabIndex = 19;
            this.buttonOK.Text = "Create &Grids";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelBubbleLocation
            // 
            this.labelBubbleLocation.Location = new System.Drawing.Point(8, 24);
            this.labelBubbleLocation.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBubbleLocation.Name = "labelBubbleLocation";
            this.labelBubbleLocation.Size = new System.Drawing.Size(149, 22);
            this.labelBubbleLocation.TabIndex = 16;
            this.labelBubbleLocation.Text = "Bubble location:";
            // 
            // textBoxFirstLabel
            // 
            this.textBoxFirstLabel.Location = new System.Drawing.Point(165, 61);
            this.textBoxFirstLabel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxFirstLabel.Name = "textBoxFirstLabel";
            this.textBoxFirstLabel.Size = new System.Drawing.Size(227, 25);
            this.textBoxFirstLabel.TabIndex = 1;
            this.textBoxFirstLabel.Tag = "";
            this.textBoxFirstLabel.Text = "1";
            // 
            // labelFirstLabel
            // 
            this.labelFirstLabel.Location = new System.Drawing.Point(8, 65);
            this.labelFirstLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelFirstLabel.Name = "labelFirstLabel";
            this.labelFirstLabel.Size = new System.Drawing.Size(149, 22);
            this.labelFirstLabel.TabIndex = 15;
            this.labelFirstLabel.Text = "Label of first grid:";
            // 
            // comboBoxBubbleLocation
            // 
            this.comboBoxBubbleLocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxBubbleLocation.FormattingEnabled = true;
            this.comboBoxBubbleLocation.Items.AddRange(new object[] {
            "At start point of lines/arcs",
            "At end point of  lines/arcs"});
            this.comboBoxBubbleLocation.Location = new System.Drawing.Point(165, 22);
            this.comboBoxBubbleLocation.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBoxBubbleLocation.Name = "comboBoxBubbleLocation";
            this.comboBoxBubbleLocation.Size = new System.Drawing.Size(227, 23);
            this.comboBoxBubbleLocation.TabIndex = 0;
            // 
            // groupBoxGridSettings
            // 
            this.groupBoxGridSettings.Controls.Add(this.checkBoxDeleteElements);
            this.groupBoxGridSettings.Controls.Add(this.labelBubbleLocation);
            this.groupBoxGridSettings.Controls.Add(this.textBoxFirstLabel);
            this.groupBoxGridSettings.Controls.Add(this.labelFirstLabel);
            this.groupBoxGridSettings.Controls.Add(this.comboBoxBubbleLocation);
            this.groupBoxGridSettings.Location = new System.Drawing.Point(13, 12);
            this.groupBoxGridSettings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxGridSettings.Name = "groupBoxGridSettings";
            this.groupBoxGridSettings.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxGridSettings.Size = new System.Drawing.Size(413, 130);
            this.groupBoxGridSettings.TabIndex = 21;
            this.groupBoxGridSettings.TabStop = false;
            this.groupBoxGridSettings.Text = "Settings";
            // 
            // checkBoxDeleteElements
            // 
            this.checkBoxDeleteElements.AutoSize = true;
            this.checkBoxDeleteElements.Checked = true;
            this.checkBoxDeleteElements.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDeleteElements.Location = new System.Drawing.Point(12, 100);
            this.checkBoxDeleteElements.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxDeleteElements.Name = "checkBoxDeleteElements";
            this.checkBoxDeleteElements.Size = new System.Drawing.Size(389, 19);
            this.checkBoxDeleteElements.TabIndex = 2;
            this.checkBoxDeleteElements.Text = "Delete the selected lines/arcs after creation";
            this.checkBoxDeleteElements.UseVisualStyleBackColor = true;
            // 
            // CreateWithSelectedCurvesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 210);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxGridSettings);
            this.Name = "CreateWithSelectedCurvesForm";
            this.Text = "CreateWithSelectedCurvesForm";
            this.groupBoxGridSettings.ResumeLayout(false);
            this.groupBoxGridSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelBubbleLocation;
        private System.Windows.Forms.TextBox textBoxFirstLabel;
        private System.Windows.Forms.Label labelFirstLabel;
        private System.Windows.Forms.ComboBox comboBoxBubbleLocation;
        private System.Windows.Forms.GroupBox groupBoxGridSettings;
        private System.Windows.Forms.CheckBox checkBoxDeleteElements;
    }
}