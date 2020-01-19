namespace Socks5Local
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.IP_ADDR = new System.Windows.Forms.TextBox();
            this.PORT = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Local_PORT = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Pass = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(347, 66);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(163, 36);
            this.button1.TabIndex = 0;
            this.button1.Text = "启动";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // IP_ADDR
            // 
            this.IP_ADDR.Location = new System.Drawing.Point(108, 12);
            this.IP_ADDR.MaxLength = 72;
            this.IP_ADDR.Name = "IP_ADDR";
            this.IP_ADDR.Size = new System.Drawing.Size(178, 21);
            this.IP_ADDR.TabIndex = 1;
            this.IP_ADDR.Text = "127.0.0.1";
            // 
            // PORT
            // 
            this.PORT.Location = new System.Drawing.Point(108, 50);
            this.PORT.MaxLength = 5;
            this.PORT.Name = "PORT";
            this.PORT.Size = new System.Drawing.Size(178, 21);
            this.PORT.TabIndex = 2;
            this.PORT.Text = "1080";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(52, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "端口";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(334, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "代理端口";
            // 
            // Local_PORT
            // 
            this.Local_PORT.Location = new System.Drawing.Point(411, 15);
            this.Local_PORT.MaxLength = 5;
            this.Local_PORT.Name = "Local_PORT";
            this.Local_PORT.Size = new System.Drawing.Size(99, 21);
            this.Local_PORT.TabIndex = 6;
            this.Local_PORT.Text = "3080";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(52, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "密码";
            // 
            // Pass
            // 
            this.Pass.Location = new System.Drawing.Point(108, 87);
            this.Pass.Name = "Pass";
            this.Pass.Size = new System.Drawing.Size(179, 21);
            this.Pass.TabIndex = 8;
            this.Pass.Text = "123456";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 119);
            this.Controls.Add(this.Pass);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Local_PORT);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PORT);
            this.Controls.Add(this.IP_ADDR);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Socks5Local";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox IP_ADDR;
        private System.Windows.Forms.TextBox PORT;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox Local_PORT;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Pass;
        private System.Windows.Forms.Button button1;
    }
}

