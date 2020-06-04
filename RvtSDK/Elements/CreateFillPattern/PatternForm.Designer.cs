namespace CreateFillPattern
{
    partial class PatternForm
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
            this.buttonApplyToSurface = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.treeViewFillPattern = new System.Windows.Forms.TreeView();
            this.buttonApplyToCutSurface = new System.Windows.Forms.Button();
            this.tabControlFillPattern = new System.Windows.Forms.TabControl();
            this.tabPageFillPattern = new System.Windows.Forms.TabPage();
            this.tabPageLinePattern = new System.Windows.Forms.TabPage();
            this.buttonApplyToGrids = new System.Windows.Forms.Button();
            this.treeViewLinePattern = new System.Windows.Forms.TreeView();
            this.buttonCreateLinePattern = new System.Windows.Forms.Button();
            this.buttonCreateComplexFillPattern = new System.Windows.Forms.Button();
            this.buttonCreateFillPattern = new System.Windows.Forms.Button();
            this.tabControlFillPattern.SuspendLayout();
            this.tabPageFillPattern.SuspendLayout();
            this.tabPageLinePattern.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonApplyToSurface
            // 
            this.buttonApplyToSurface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApplyToSurface.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonApplyToSurface.Location = new System.Drawing.Point(8, 378);
            this.buttonApplyToSurface.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonApplyToSurface.Name = "buttonApplyToSurface";
            this.buttonApplyToSurface.Size = new System.Drawing.Size(193, 27);
            this.buttonApplyToSurface.TabIndex = 0;
            this.buttonApplyToSurface.Text = "Apply To Surface";
            this.buttonApplyToSurface.UseVisualStyleBackColor = true;
            this.buttonApplyToSurface.Click += new System.EventHandler(this.buttonApplyToSurface_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(260, 495);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(157, 27);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // treeViewFillPattern
            // 
            this.treeViewFillPattern.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewFillPattern.Location = new System.Drawing.Point(-5, 0);
            this.treeViewFillPattern.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.treeViewFillPattern.Name = "treeViewFillPattern";
            this.treeViewFillPattern.Size = new System.Drawing.Size(501, 372);
            this.treeViewFillPattern.TabIndex = 0;
            // 
            // buttonApplyToCutSurface
            // 
            this.buttonApplyToCutSurface.Location = new System.Drawing.Point(241, 378);
            this.buttonApplyToCutSurface.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonApplyToCutSurface.Name = "buttonApplyToCutSurface";
            this.buttonApplyToCutSurface.Size = new System.Drawing.Size(157, 27);
            this.buttonApplyToCutSurface.TabIndex = 3;
            this.buttonApplyToCutSurface.Text = "Apply To CutSurface";
            this.buttonApplyToCutSurface.UseVisualStyleBackColor = true;
            this.buttonApplyToCutSurface.Click += new System.EventHandler(this.buttonApplyToCutSurface_Click);
            // 
            // tabControlFillPattern
            // 
            this.tabControlFillPattern.Controls.Add(this.tabPageFillPattern);
            this.tabControlFillPattern.Controls.Add(this.tabPageLinePattern);
            this.tabControlFillPattern.Location = new System.Drawing.Point(13, 12);
            this.tabControlFillPattern.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControlFillPattern.Name = "tabControlFillPattern";
            this.tabControlFillPattern.SelectedIndex = 0;
            this.tabControlFillPattern.Size = new System.Drawing.Size(512, 442);
            this.tabControlFillPattern.TabIndex = 8;
            // 
            // tabPageFillPattern
            // 
            this.tabPageFillPattern.Controls.Add(this.treeViewFillPattern);
            this.tabPageFillPattern.Controls.Add(this.buttonApplyToSurface);
            this.tabPageFillPattern.Controls.Add(this.buttonApplyToCutSurface);
            this.tabPageFillPattern.Location = new System.Drawing.Point(4, 25);
            this.tabPageFillPattern.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPageFillPattern.Name = "tabPageFillPattern";
            this.tabPageFillPattern.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPageFillPattern.Size = new System.Drawing.Size(504, 413);
            this.tabPageFillPattern.TabIndex = 0;
            this.tabPageFillPattern.Text = "FillPatterns";
            this.tabPageFillPattern.UseVisualStyleBackColor = true;
            // 
            // tabPageLinePattern
            // 
            this.tabPageLinePattern.Controls.Add(this.buttonApplyToGrids);
            this.tabPageLinePattern.Controls.Add(this.treeViewLinePattern);
            this.tabPageLinePattern.Location = new System.Drawing.Point(4, 25);
            this.tabPageLinePattern.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPageLinePattern.Name = "tabPageLinePattern";
            this.tabPageLinePattern.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPageLinePattern.Size = new System.Drawing.Size(504, 413);
            this.tabPageLinePattern.TabIndex = 1;
            this.tabPageLinePattern.Text = "LinePatterns";
            this.tabPageLinePattern.UseVisualStyleBackColor = true;
            // 
            // buttonApplyToGrids
            // 
            this.buttonApplyToGrids.Location = new System.Drawing.Point(8, 378);
            this.buttonApplyToGrids.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonApplyToGrids.Name = "buttonApplyToGrids";
            this.buttonApplyToGrids.Size = new System.Drawing.Size(157, 27);
            this.buttonApplyToGrids.TabIndex = 2;
            this.buttonApplyToGrids.Text = "Apply To Grids";
            this.buttonApplyToGrids.UseVisualStyleBackColor = true;
            this.buttonApplyToGrids.Click += new System.EventHandler(this.buttonApplyToGrids_Click);
            // 
            // treeViewLinePattern
            // 
            this.treeViewLinePattern.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewLinePattern.Location = new System.Drawing.Point(-5, 0);
            this.treeViewLinePattern.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.treeViewLinePattern.Name = "treeViewLinePattern";
            this.treeViewLinePattern.Size = new System.Drawing.Size(501, 372);
            this.treeViewLinePattern.TabIndex = 0;
            // 
            // buttonCreateLinePattern
            // 
            this.buttonCreateLinePattern.Location = new System.Drawing.Point(260, 461);
            this.buttonCreateLinePattern.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCreateLinePattern.Name = "buttonCreateLinePattern";
            this.buttonCreateLinePattern.Size = new System.Drawing.Size(157, 27);
            this.buttonCreateLinePattern.TabIndex = 11;
            this.buttonCreateLinePattern.Text = "Create Line Pattern";
            this.buttonCreateLinePattern.UseVisualStyleBackColor = true;
            this.buttonCreateLinePattern.Click += new System.EventHandler(this.buttonCreateLinePattern_Click);
            // 
            // buttonCreateComplexFillPattern
            // 
            this.buttonCreateComplexFillPattern.Location = new System.Drawing.Point(26, 495);
            this.buttonCreateComplexFillPattern.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCreateComplexFillPattern.Name = "buttonCreateComplexFillPattern";
            this.buttonCreateComplexFillPattern.Size = new System.Drawing.Size(193, 27);
            this.buttonCreateComplexFillPattern.TabIndex = 9;
            this.buttonCreateComplexFillPattern.Text = "Create Complex Fill Pattern";
            this.buttonCreateComplexFillPattern.UseVisualStyleBackColor = true;
            this.buttonCreateComplexFillPattern.Click += new System.EventHandler(this.buttonCreateComplexFillPattern_Click);
            // 
            // buttonCreateFillPattern
            // 
            this.buttonCreateFillPattern.Location = new System.Drawing.Point(26, 461);
            this.buttonCreateFillPattern.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonCreateFillPattern.Name = "buttonCreateFillPattern";
            this.buttonCreateFillPattern.Size = new System.Drawing.Size(193, 27);
            this.buttonCreateFillPattern.TabIndex = 10;
            this.buttonCreateFillPattern.Text = "Create Fill Pattern";
            this.buttonCreateFillPattern.UseVisualStyleBackColor = true;
            this.buttonCreateFillPattern.Click += new System.EventHandler(this.buttonCreateFillPattern_Click);
            // 
            // PatternForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 537);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.tabControlFillPattern);
            this.Controls.Add(this.buttonCreateLinePattern);
            this.Controls.Add(this.buttonCreateComplexFillPattern);
            this.Controls.Add(this.buttonCreateFillPattern);
            this.Name = "PatternForm";
            this.Text = "PatternForm";
            this.tabControlFillPattern.ResumeLayout(false);
            this.tabPageFillPattern.ResumeLayout(false);
            this.tabPageLinePattern.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonApplyToSurface;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TreeView treeViewFillPattern;
        private System.Windows.Forms.Button buttonApplyToCutSurface;
        private System.Windows.Forms.TabControl tabControlFillPattern;
        private System.Windows.Forms.TabPage tabPageFillPattern;
        private System.Windows.Forms.TabPage tabPageLinePattern;
        private System.Windows.Forms.Button buttonApplyToGrids;
        private System.Windows.Forms.TreeView treeViewLinePattern;
        private System.Windows.Forms.Button buttonCreateLinePattern;
        private System.Windows.Forms.Button buttonCreateComplexFillPattern;
        private System.Windows.Forms.Button buttonCreateFillPattern;
    }
}