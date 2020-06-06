namespace PowerCircuit
{
    partial class EditCircuitForm
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
            this.components = new System.ComponentModel.Container();
            this.buttonSelectPanel = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // buttonSelectPanel
            // 
            this.buttonSelectPanel.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSelectPanel.Location = new System.Drawing.Point(138, 12);
            this.buttonSelectPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonSelectPanel.Name = "buttonSelectPanel";
            this.buttonSelectPanel.Size = new System.Drawing.Size(106, 32);
            this.buttonSelectPanel.TabIndex = 4;
            this.buttonSelectPanel.Text = "SelectPanel";
            this.buttonSelectPanel.UseVisualStyleBackColor = true;
            this.buttonSelectPanel.Click += new System.EventHandler(this.buttonSelectPanel_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonRemove.Location = new System.Drawing.Point(66, 12);
            this.buttonRemove.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(64, 32);
            this.buttonRemove.TabIndex = 5;
            this.buttonRemove.Text = "Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonAdd.Location = new System.Drawing.Point(13, 12);
            this.buttonAdd.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(45, 32);
            this.buttonAdd.TabIndex = 3;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // EditCircuitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(263, 59);
            this.Controls.Add(this.buttonSelectPanel);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonAdd);
            this.Name = "EditCircuitForm";
            this.Text = "EditCircuitForm";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonSelectPanel;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ToolTip toolTip;
    }
}