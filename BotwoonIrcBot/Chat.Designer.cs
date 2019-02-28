namespace BotwoonIrcBot
{
    partial class Chat
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Chat));
            this.fadeTimer = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.customLabel1 = new BotwoonIrcBot.CustomLabel();
            this.SuspendLayout();
            // 
            // fadeTimer
            // 
            this.fadeTimer.Enabled = true;
            this.fadeTimer.Tick += new System.EventHandler(this.fadeTimer_Tick);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Eras Bold ITC", 12F);
            this.label1.Location = new System.Drawing.Point(194, 131);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(237, 58);
            this.label1.TabIndex = 2;
            this.label1.Text = "Test label says \"Do not show\"";
            this.label1.Visible = false;
            // 
            // customLabel1
            // 
            this.customLabel1.AutoEllipsis = true;
            this.customLabel1.Countdown = 0;
            this.customLabel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.customLabel1.Font = new System.Drawing.Font("Eras Bold ITC", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.customLabel1.Location = new System.Drawing.Point(0, 385);
            this.customLabel1.Name = "customLabel1";
            this.customLabel1.OutlineForeColor = System.Drawing.Color.White;
            this.customLabel1.OutlineWidth = 2F;
            this.customLabel1.Size = new System.Drawing.Size(838, 57);
            this.customLabel1.TabIndex = 1;
            this.customLabel1.Text = resources.GetString("customLabel1.Text");
            this.customLabel1.Visible = false;
            // 
            // Chat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Green;
            this.ClientSize = new System.Drawing.Size(838, 442);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.customLabel1);
            this.Name = "Chat";
            this.Text = "chat";
            this.ResumeLayout(false);

        }

        #endregion

        private CustomLabel customLabel1;
        private System.Windows.Forms.Timer fadeTimer;
        private System.Windows.Forms.Label label1;
    }
}