using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BotwoonIrcBot
{
    public partial class IrcPanel : UserControl
    {
        private Chat chatBox;
        private Controller controller;
        private bool autoStart;
        private bool autoReconnect;
        
        public IrcPanel()
        {
            InitializeComponent();
        }

        public void connect_Click(object sender, EventArgs e)
        {
            try
            {
                if (connect.Text == "Connect")
                {
                    controller = new Controller();

                    controller.CommandHandled += controller_CommandHandled;
                    controller.OutputSent += controller_OutputSent;
                    controller.CommandReceived += controller_CommandReceived;
                    controller.CommandSent += controller_CommandSent;
                    controller.TopicSet += controller_TopicSet;
                    controller.TopicOwner += controller_TopicOwner;
                    controller.NamesList += controller_NamesList;
                    controller.ServerMessage += controller_ServerMessage;
                    controller.Join += controller_Join;
                    controller.JoinedChannel += controller_JoinedChannel;
                    controller.Part += controller_Part;
                    controller.Mode += controller_Mode;
                    controller.NickChange += controller_NickChange;
                    controller.Kick += controller_Kick;
                    controller.Quit += controller_Quit;
                    controller.ChannelsInChanged += controller_ChannelsInChanged;
                    controller.Disconnected += controller_Disconnected;

                    autoReconnect = true;
                    controller.StartController(server.Text, Convert.ToInt32(port.Text), channel.Text);
                    destination.Items.AddRange(channel.Text.Split(','));
                    connect.Text = "Disconnect";
                }
                else
                {
                    autoReconnect = false;
                    controller.StopController();
                    destination.Items.Clear();
                    connect.Text = "Connect";
                }
            }
            catch (SocketException)
            {
                MessageBox.Show(this,
                                "Could not find the host specified. Please check your connection and the host address and try again.",
                                "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void controller_ChannelsInChanged()
        {
            var destinationList = new List<string>();

            for (int i = 0; i < destination.Items.Count; i++)
                destinationList.Add(destination.Items[i].ToString());

            foreach (var channel in destinationList)
            {
                if (!controller.ChannelsIn.Contains(channel))
                    RemoveChannel(channel);
            }

            foreach (var channel in controller.ChannelsIn)
            {
                if (!destination.Items.Contains(channel))
                    AddChannel(channel);
            }
        }

        private void controller_OutputSent(string sentChannel, string output)
        {
            if (!output.StartsWith("["))
            {
                WriteLine(string.Format("Output Sent ({0}): {1}", sentChannel, output));
            }
        }

        private void controller_JoinedChannel(string channel)
        {
            AddChannel(channel);
        }

        private void controller_CommandHandled(string user, string command)
        {
            WriteLine(string.Format("Command received from {0}: {1}", user, command));
        }


        private void controller_CommandReceived(string command)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowCommands"]))
                WriteLine(command);

            var msgIndex = command.IndexOf("KICK");
            if (msgIndex > -1 && command.IndexOf(controller.Nick) > -1)
            {
                var receivedMessage = command.Substring(msgIndex);
                var tokens = receivedMessage.Split(' ');
                var fromChannel = tokens[1];
                RemoveChannel(fromChannel);
            }
            
            msgIndex = command.IndexOf("PRIVMSG");
            if (msgIndex > -1 && chatBox != null)
            {
                var receivedMessage = command.Substring(msgIndex);
                var tokens = receivedMessage.Split(' ');
                var fromChannel = tokens[1];
                var sender = command.Substring(1, command.IndexOf("!") - 1);
                var message = receivedMessage.Substring(receivedMessage.IndexOf(":") + 1);
                if (fromChannel == "#dessyreqt")
                    AddChat(sender, message);
            }
        }

        private delegate void AddChatCallback(string sender, string message);

        private void AddChat(string sender, string message)
        {
            if (destination.InvokeRequired)
            {
                try
                {
                    var d = new AddChatCallback(AddChat);
                    Invoke(d, new[] { sender, message });
                }
                catch (InvalidOperationException)
                {
                    //This is probably safely ignored.
                }
            }
            else
            {
                chatBox.AddChat(sender, message);
            }
        }


        private void controller_CommandSent(string command)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowCommands"]))
                WriteLine(command);
        }

        private void controller_TopicSet(string fromChannel, string topic)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowTopicSet"]))
                WriteLine(String.Format("Topic of {0} is: {1}", fromChannel, topic));
        }

        private void controller_TopicOwner(string fromChannel, string user, string topicDate)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowTopicOwner"]))
                WriteLine(String.Format("Topic of {0} set by {1} on {2} (unixtime)", fromChannel, user, topicDate));
        }

        private void controller_NamesList(string userNames)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowNamesList"]))
                WriteLine(String.Format("Names List: {0}", userNames));
        }

        private void controller_ServerMessage(string serverMessage)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowServerMessage"]))
                WriteLine(String.Format("Server Message: {0}", serverMessage));
        }

        private void controller_Join(string fromChannel, string user)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowJoin"]))
                WriteLine(String.Format("{0} joins {1}", user, fromChannel));
        }

        private void controller_Part(string fromChannel, string user)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowPart"]))
                WriteLine(String.Format("{0} parts {1}", user, fromChannel));
        }

        private void controller_Mode(string fromChannel, string user, string userMode)
        {
            if (user != fromChannel)
            {
                if (bool.Parse(ConfigurationManager.AppSettings["ShowMode"]))
                    WriteLine(String.Format("{0} sets {1} in {2}", user, userMode, fromChannel));
            }
        }

        private void controller_NickChange(string oldNick, string newNick)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowNickChange"]))
                WriteLine(String.Format("{0} changes nick to {1}", oldNick, newNick));
        }

        private void controller_Kick(string fromChannel, string kickingUser, string kickedUser, string kickMessage)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowKick"]))
                WriteLine(String.Format("{0} kicks {1} out {2} ({3})", kickingUser, kickedUser, fromChannel, kickMessage));
        }

        private void controller_Quit(string user, string quitMessage)
        {
            if (bool.Parse(ConfigurationManager.AppSettings["ShowQuit"]))
                WriteLine(String.Format("{0} has quit IRC ({1})", user, quitMessage));
        }

        private void controller_Disconnected()
        {
            SetDisconnected();

            if (autoReconnect)
            {
                var autoReconnectThread = new Thread(AutoReconnect);
                autoReconnectThread.Start();
            }
        }

        private void AutoReconnect()
        {
            do
            {
                try
                {
                    controller.StartController(server.Text, Convert.ToInt32(port.Text), channel.Text);
                    foreach (var ch in channel.Text.Split(','))
                    {
                        AddChannel(ch);
                    }
                    Thread.Sleep(5000);
                }
                catch (SocketException)
                {
                    //still can't connect, let's keep going until we do.
                    Thread.Sleep(5000);
                }
            } while (!controller.Connected);

            SetConnected();
        }

        public void Close()
        {
            if (controller != null)
                controller.StopController();
            autoReconnect = false;
        }

        private delegate void RemoveChannelCallback(string channel);

        private void RemoveChannel(string removedChannel)
        {
            if (destination.InvokeRequired)
            {
                try
                {
                    var d = new RemoveChannelCallback(RemoveChannel);
                    Invoke(d, new[] { removedChannel });
                }
                catch (InvalidOperationException)
                {
                    //This is probably safely ignored.
                }
            }
            else
            {
                destination.Items.Remove(removedChannel);
            }
        }

        private delegate void AddChannelCallback(string channel);

        private void AddChannel(string addedChannel)
        {
            if (destination.InvokeRequired)
            {
                try
                {
                    var d = new AddChannelCallback(AddChannel);
                    Invoke(d, new[] { addedChannel });
                }
                catch (InvalidOperationException)
                {
                    //This is probably safely ignored.
                }
            }
            else
            {
                destination.Items.Add(addedChannel);
            }
        }

        private delegate void SetDisconnectedCallback();

        private void SetDisconnected()
        {
            if (eventLog.InvokeRequired)
            {
                try
                {
                    var d = new SetDisconnectedCallback(SetDisconnected);
                    Invoke(d, new object[0]);
                }
                catch (InvalidOperationException)
                {
                    //This is probably safely ignored.
                }
            }
            else
            {
                destination.Items.Clear();
                connect.Text = "Connect";
            }
        }

        private delegate void SetConnectedCallback();

        private void SetConnected()
        {
            if (eventLog.InvokeRequired)
            {
                try
                {
                    var d = new SetConnectedCallback(SetConnected);
                    Invoke(d, new object[0]);
                }
                catch (InvalidOperationException)
                {
                    //This is probably safely ignored.
                }
            }
            else
            {
                connect.Text = "Disconnect";
            }
        }

        private delegate void WriteLineCallback(string line);

        private void WriteLine(string line)
        {
            var sb = new StringBuilder();

            sb.Append("[");
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
            sb.Append("] ");
            sb.AppendLine(line);

            if (eventLog.InvokeRequired)
            {
                var d = new WriteLineCallback(WriteLine);
                Invoke(d, new[] { line });
            }
            else
            {
                eventLog.Text += sb.ToString();
                eventLog.SelectionStart = eventLog.Text.Length;
                eventLog.ScrollToCaret();
            }
        }

        private void send_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(message.Text) && !string.IsNullOrWhiteSpace(destination.Text))
            {
                controller.SendMessage(destination.Text, message.Text);
                WriteLine(string.Format("Sent message to {0}: {1}", destination.Text, message.Text));
            }
            message.Text = "";
        }

        private void message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                send_Click(sender, e);
            }
        }

        private void IrcPanel_Load(object sender, EventArgs e)
        {
            server.Text = ConfigurationManager.AppSettings["server"];
            port.Text = ConfigurationManager.AppSettings["port"];
            channel.Text = ConfigurationManager.AppSettings["channels"];
            
            if (autoStart)
                connect_Click(this, new EventArgs());
        }

        private void chat_Click(object sender, EventArgs e)
        {
            chatBox = new Chat();
            chatBox.Show();
        }
    }
}
