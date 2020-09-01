namespace GridCreation
{
    partial class GridCreationOptionForm
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
            this.radioButtonSelect = new System.Windows.Forms.RadioButton();
            this.radioButtonRadialAndCircularGrids = new System.Windows.Forms.RadioButton();
            this.groupBoxCreateOptions = new System.Windows.Forms.GroupBox();
            this.radioButtonOrthogonalGrids = new System.Windows.Forms.RadioButton();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxCreateOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButtonSelect
            // 
            this.radioButtonSelect.AutoSize = true;
            this.radioButtonSelect.Checked = true;
            this.radioButtonSelect.Location = new System.Drawing.Point(8, 22);
            this.radioButtonSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButtonSelect.Name = "radioButtonSelect";
            this.radioButtonSelect.Size = new System.Drawing.Size(348, 19);
            this.radioButtonSelect.TabIndex = 0;
            this.radioButtonSelect.TabStop = true;
            this.radioButtonSelect.Text = "Create grids with selected lines or arcs";
            this.radioButtonSelect.UseVisualStyleBackColor = true;
            // 
            // radioButtonRadialAndCircularGrids
            // 
            this.radioButtonRadialAndCircularGrids.AutoSize = true;
            this.radioButtonRadialAndCircularGrids.Location = new System.Drawing.Point(8, 80);
            this.radioButtonRadialAndCircularGrids.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButtonRadialAndCircularGrids.Name = "radioButtonRadialAndCircularGrids";
            this.radioButtonRadialAndCircularGrids.Size = new System.Drawing.Size(332, 19);
            this.radioButtonRadialAndCircularGrids.TabIndex = 2;
            this.radioButtonRadialAndCircularGrids.Text = "Create a batch of radial and arc grids";
            this.radioButtonRadialAndCircularGrids.UseVisualStyleBackColor = true;
            // 
            // groupBoxCreateOptions
            // 
            this.groupBoxCreateOptions.Controls.Add(this.radioButtonSelect);
            this.groupBoxCreateOptions.Controls.Add(this.radioButtonOrthogonalGrids);
            this.groupBoxCreateOptions.Controls.Add(this.radioButtonRadialAndCircularGrids);
            this.groupBoxCreateOptions.Location = new System.Drawing.Point(13, 12);
            this.groupBoxCreateOptions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxCreateOptions.Name = "groupBoxCreateOptions";
            this.groupBoxCreateOptions.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxCreateOptions.Size = new System.Drawing.Size(374, 111);
            this.groupBoxCreateOptions.TabIndex = 16;
            this.groupBoxCreateOptions.TabStop = false;
            this.groupBoxCreateOptions.Text = "Choose the way to create grids";
            // 
            // radioButtonOrthogonalGrids
            // 
            this.radioButtonOrthogonalGrids.AutoSize = true;
            this.radioButtonOrthogonalGrids.Location = new System.Drawing.Point(8, 51);
            this.radioButtonOrthogonalGrids.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.radioButtonOrthogonalGrids.Name = "radioButtonOrthogonalGrids";
            this.radioButtonOrthogonalGrids.Size = new System.Drawing.Size(300, 19);
            this.radioButtonOrthogonalGrids.TabIndex = 1;
            this.radioButtonOrthogonalGrids.Text = "Create a batch of orthogonal grids";
            this.radioButtonOrthogonalGrids.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(209, 147);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(120, 27);
            this.buttonCancel.TabIndex = 15;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(74, 147);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(120, 27);
            this.buttonOK.TabIndex = 14;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // GridCreationOptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 188);
            this.Controls.Add(this.groupBoxCreateOptions);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Name = "GridCreationOptionForm";
            this.Text = "GridCreationOptionForm";
            this.groupBoxCreateOptions.ResumeLayout(false);
            this.groupBoxCreateOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtonSelect;
        private System.Windows.Forms.RadioButton radioButtonRadialAndCircularGrids;
        private System.Windows.Forms.GroupBox groupBoxCreateOptions;
        private System.Windows.Forms.RadioButton radioButtonOrthogonalGrids;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}