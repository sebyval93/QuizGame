namespace AdminTool
{
    partial class AdminTool
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
            this.BtnUserAdmin = new System.Windows.Forms.Button();
            this.BtnQuestionsAdmin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnUserAdmin
            // 
            this.BtnUserAdmin.Location = new System.Drawing.Point(12, 12);
            this.BtnUserAdmin.Name = "BtnUserAdmin";
            this.BtnUserAdmin.Size = new System.Drawing.Size(153, 23);
            this.BtnUserAdmin.TabIndex = 0;
            this.BtnUserAdmin.Text = "User Administration";
            this.BtnUserAdmin.UseVisualStyleBackColor = true;
            this.BtnUserAdmin.Click += new System.EventHandler(this.button1_Click);
            // 
            // BtnQuestionsAdmin
            // 
            this.BtnQuestionsAdmin.Location = new System.Drawing.Point(171, 12);
            this.BtnQuestionsAdmin.Name = "BtnQuestionsAdmin";
            this.BtnQuestionsAdmin.Size = new System.Drawing.Size(153, 23);
            this.BtnQuestionsAdmin.TabIndex = 1;
            this.BtnQuestionsAdmin.Text = "Questions Administration";
            this.BtnQuestionsAdmin.UseVisualStyleBackColor = true;
            this.BtnQuestionsAdmin.Click += new System.EventHandler(this.BtnQuestionsAdmin_Click);
            // 
            // AdminTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 46);
            this.Controls.Add(this.BtnQuestionsAdmin);
            this.Controls.Add(this.BtnUserAdmin);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "AdminTool";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AdminTool";
            this.Load += new System.EventHandler(this.AdminTool_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnUserAdmin;
        private System.Windows.Forms.Button BtnQuestionsAdmin;
    }
}