using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BotwoonIrcBot
{
    public partial class MainForm : Form
    {
        private bool autoStart;

        public MainForm(bool autoStart = false)
        {
            InitializeComponent();
            this.autoStart = autoStart;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //MessageBox.Show("Please don't.");
            //e.Cancel = true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            chatDisplay.Text = Settings.Default.chatDisplay.ToString();

            if (autoStart)
            {
                ircPanel1.connect_Click(this, new EventArgs());
            }
        }

        private void chatDisplay_TextChanged(object sender, EventArgs e)
        {
            var newChatDisplay = 0;

            if (int.TryParse(chatDisplay.Text, out newChatDisplay))
            {
                Settings.Default.chatDisplay = newChatDisplay;
                Settings.Default.Save();

                var newDisplayTime = newChatDisplay / 10f;
                displayTime.Text = string.Format("{0:0.0} seconds", newDisplayTime);
            }
        }
    }
}
