namespace MailBlastApp
{
    partial class Form1
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
            this.SendMails = new System.Windows.Forms.Button();
            this.PassCode = new System.Windows.Forms.TextBox();
            this.PassCode1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SendMails
            // 
            this.SendMails.Location = new System.Drawing.Point(85, 104);
            this.SendMails.Name = "SendMails";
            this.SendMails.Size = new System.Drawing.Size(75, 23);
            this.SendMails.TabIndex = 0;
            this.SendMails.Text = "Send Mails";
            this.SendMails.UseVisualStyleBackColor = true;
            this.SendMails.Click += new System.EventHandler(this.SendMails_Click);
            // 
            // PassCode
            // 
            this.PassCode.Location = new System.Drawing.Point(76, 56);
            this.PassCode.Name = "PassCode";
            this.PassCode.Size = new System.Drawing.Size(146, 20);
            this.PassCode.TabIndex = 1;
            // 
            // PassCode1
            // 
            this.PassCode1.AutoSize = true;
            this.PassCode1.Location = new System.Drawing.Point(13, 59);
            this.PassCode1.Name = "PassCode1";
            this.PassCode1.Size = new System.Drawing.Size(58, 13);
            this.PassCode1.TabIndex = 2;
            this.PassCode1.Text = "Pass Code";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.PassCode1);
            this.Controls.Add(this.PassCode);
            this.Controls.Add(this.SendMails);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button SendMails;
        private System.Windows.Forms.TextBox PassCode;
        private System.Windows.Forms.Label PassCode1;
    }
}

