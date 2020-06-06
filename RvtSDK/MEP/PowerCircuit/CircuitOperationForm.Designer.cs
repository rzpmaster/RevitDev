namespace PowerCircuit
{
    partial class CircuitOperationForm
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
            this.buttonDisconnectPanel = new System.Windows.Forms.Button();
            this.buttonSelectPanel = new System.Windows.Forms.Button();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonCreate = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // buttonDisconnectPanel
            // 
            this.buttonDisconnectPanel.BackColor = System.Drawing.Color.Transparent;
            this.buttonDisconnectPanel.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonDisconnectPanel.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.buttonDisconnectPanel.FlatAppearance.BorderSize = 0;
            this.buttonDisconnectPanel.Location = new System.Drawing.Point(370, 12);
            this.buttonDisconnectPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonDisconnectPanel.Name = "buttonDisconnectPanel";
            this.buttonDisconnectPanel.Size = new System.Drawing.Size(137, 32);
            this.buttonDisconnectPanel.TabIndex = 8;
            this.buttonDisconnectPanel.Text = "DisconnectPanel";
            this.buttonDisconnectPanel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonDisconnectPanel.UseVisualStyleBackColor = false;
            this.buttonDisconnectPanel.Click += new System.EventHandler(this.buttonDisconnectPanel_Click);
            // 
            // buttonSelectPanel
            // 
            this.buttonSelectPanel.BackColor = System.Drawing.Color.Transparent;
            this.buttonSelectPanel.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSelectPanel.FlatAppearance.BorderSize = 0;
            this.buttonSelectPanel.Location = new System.Drawing.Point(257, 12);
            this.buttonSelectPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonSelectPanel.Name = "buttonSelectPanel";
            this.buttonSelectPanel.Size = new System.Drawing.Size(105, 32);
            this.buttonSelectPanel.TabIndex = 7;
            this.buttonSelectPanel.Text = "SelectPanel";
            this.buttonSelectPanel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonSelectPanel.UseVisualStyleBackColor = false;
            this.buttonSelectPanel.Click += new System.EventHandler(this.buttonSelectPanel_Click);
            // 
            // buttonEdit
            // 
            this.buttonEdit.BackColor = System.Drawing.Color.Transparent;
            this.buttonEdit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonEdit.FlatAppearance.BorderSize = 0;
            this.buttonEdit.Location = new System.Drawing.Point(145, 12);
            this.buttonEdit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(104, 32);
            this.buttonEdit.TabIndex = 6;
            this.buttonEdit.Text = "EditCircuit";
            this.buttonEdit.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonEdit.UseVisualStyleBackColor = false;
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            // 
            // buttonCreate
            // 
            this.buttonCreate.BackColor = System.Drawing.Color.Transparent;
            this.buttonCreate.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonCreate.FlatAppearance.BorderSize = 0;
            this.buttonCreate.Location = new System.Drawing.Point(13, 12);
            this.buttonCreate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCreate.Name = "buttonCreate";
            this.buttonCreate.Size = new System.Drawing.Size(124, 32);
            this.buttonCreate.TabIndex = 5;
            this.buttonCreate.Text = "CreateCircuit";
            this.buttonCreate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.buttonCreate.UseVisualStyleBackColor = false;
            this.buttonCreate.Click += new System.EventHandler(this.buttonCreate_Click);
            // 
            // CircuitOperationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 60);
            this.Controls.Add(this.buttonDisconnectPanel);
            this.Controls.Add(this.buttonSelectPanel);
            this.Controls.Add(this.buttonEdit);
            this.Controls.Add(this.buttonCreate);
            this.Name = "CircuitOperationForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonDisconnectPanel;
        private System.Windows.Forms.Button buttonSelectPanel;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.Button buttonCreate;
        private System.Windows.Forms.ToolTip toolTip;
    }
}