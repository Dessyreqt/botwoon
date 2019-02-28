namespace BotwoonIrcBot
{
    partial class IrcPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.send = new System.Windows.Forms.Button();
            this.eventLog = new System.Windows.Forms.TextBox();
            this.message = new System.Windows.Forms.TextBox();
            this.server = new System.Windows.Forms.TextBox();
            this.destination = new System.Windows.Forms.ComboBox();
            this.port = new System.Windows.Forms.TextBox();
            this.connect = new System.Windows.Forms.Button();
            this.channel = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chat = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // send
            // 
            this.send.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.send.Location = new System.Drawing.Point(609, 357);
            this.send.Name = "send";
            this.send.Size = new System.Drawing.Size(56, 20);
            this.send.TabIndex = 34;
            this.send.Text = "Send";
            this.send.UseVisualStyleBackColor = true;
            this.send.Click += new System.EventHandler(this.send_Click);
            // 
            // eventLog
            // 
            this.eventLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eventLog.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eventLog.Location = new System.Drawing.Point(5, 45);
            this.eventLog.Multiline = true;
            this.eventLog.Name = "eventLog";
            this.eventLog.ReadOnly = true;
            this.eventLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.eventLog.Size = new System.Drawing.Size(660, 306);
            this.eventLog.TabIndex = 30;
            // 
            // message
            // 
            this.message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.message.Location = new System.Drawing.Point(132, 357);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(471, 20);
            this.message.TabIndex = 33;
            this.message.KeyDown += new System.Windows.Forms.KeyEventHandler(this.message_KeyDown);
            // 
            // server
            // 
            this.server.Location = new System.Drawing.Point(8, 19);
            this.server.Name = "server";
            this.server.Size = new System.Drawing.Size(178, 20);
            this.server.TabIndex = 24;
            // 
            // destination
            // 
            this.destination.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.destination.FormattingEnabled = true;
            this.destination.Location = new System.Drawing.Point(5, 357);
            this.destination.Name = "destination";
            this.destination.Size = new System.Drawing.Size(121, 21);
            this.destination.TabIndex = 32;
            // 
            // port
            // 
            this.port.Location = new System.Drawing.Point(192, 19);
            this.port.Name = "port";
            this.port.Size = new System.Drawing.Size(46, 20);
            this.port.TabIndex = 25;
            // 
            // connect
            // 
            this.connect.Location = new System.Drawing.Point(425, 19);
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(100, 20);
            this.connect.TabIndex = 31;
            this.connect.Text = "Connect";
            this.connect.UseVisualStyleBackColor = true;
            this.connect.Click += new System.EventHandler(this.connect_Click);
            // 
            // channel
            // 
            this.channel.Location = new System.Drawing.Point(244, 19);
            this.channel.Name = "channel";
            this.channel.Size = new System.Drawing.Size(175, 20);
            this.channel.TabIndex = 26;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 27;
            this.label1.Text = "Server";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(241, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "Channel";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(187, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 28;
            this.label2.Text = "Port";
            // 
            // chat
            // 
            this.chat.Location = new System.Drawing.Point(531, 19);
            this.chat.Name = "chat";
            this.chat.Size = new System.Drawing.Size(75, 20);
            this.chat.TabIndex = 35;
            this.chat.Text = "Chat Box";
            this.chat.UseVisualStyleBackColor = true;
            this.chat.Click += new System.EventHandler(this.chat_Click);
            // 
            // IrcPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chat);
            this.Controls.Add(this.send);
            this.Controls.Add(this.eventLog);
            this.Controls.Add(this.message);
            this.Controls.Add(this.server);
            this.Controls.Add(this.destination);
            this.Controls.Add(this.port);
            this.Controls.Add(this.connect);
            this.Controls.Add(this.channel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.MinimumSize = new System.Drawing.Size(670, 380);
            this.Name = "IrcPanel";
            this.Size = new System.Drawing.Size(670, 384);
            this.Load += new System.EventHandler(this.IrcPanel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button send;
        private System.Windows.Forms.TextBox eventLog;
        private System.Windows.Forms.TextBox message;
        private System.Windows.Forms.TextBox server;
        private System.Windows.Forms.ComboBox destination;
        private System.Windows.Forms.TextBox port;
        private System.Windows.Forms.Button connect;
        private System.Windows.Forms.TextBox channel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button chat;
    }
}
