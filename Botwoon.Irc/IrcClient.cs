using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Botwoon.Irc
{
    public delegate void CommandReceived(string command);

    public delegate void CommandSent(string command);

    public delegate void TopicSet(string fromChannel, string topic);

    public delegate void TopicChanged(string sender, string fromChannel, string topic);

    public delegate void TopicOwner(string fromChannel, string user, string topicDate);

    public delegate void NamesList(string userNames);

    public delegate void ServerMessage(string serverMessage);

    public delegate void Join(string fromChannel, string user);

    public delegate void Part(string fromChannel, string user);

    public delegate void Mode(string fromChannel, string user, string userMode);

    public delegate void NickChange(string oldNick, string newNick);

    public delegate void Kick(string fromChannel, string kickingUser, string kickedUser, string kickMessage);

    public delegate void Quit(string user, string quitMessage);

    public delegate void Disconnected();

    public class IrcClient
    {
        public event CommandReceived CommandReceived;
        public event CommandSent CommandSent;
        public event TopicSet TopicSet;
        public event TopicOwner TopicOwner;
        public event NamesList NamesList;
        public event ServerMessage ServerMessage;
        public event Join Join;
        public event Part Part;
        public event Mode Mode;
        public event NickChange NickChange;
        public event Kick Kick;
        public event Quit Quit;
        public event Disconnected Disconnected;
        public event TopicChanged TopicChanged;

        public string Server { get; set; }
        public int Port { get; set; }
        public string Nick { get; set; }
        public string NickPass { get; set; }
        public string User { get; set; }
        public string RealName { get; set; }
        public string Channel { get; set; }
        public bool IsInvisble { get; set; }
        public TcpClient Connection { get; set; }
        public NetworkStream Stream { get; set; }
        public StreamWriter Writer { get; set; }
        public StreamReader Reader { get; set; }
        public bool Connected { get; set; }
        public List<string> ChannelsIn { get; set; }
        public Dictionary<string, List<string>> ChannelUsers { get; set; }

        private Thread listenThread;
        private bool disconnecting;

        public IrcClient(string nick, string nickPass, string channel, string realName)
        {
            Nick = nick;
            NickPass = nickPass;
            User = nick;
            RealName = realName;
            Channel = channel;
            IsInvisble = false;
            Connected = false;
            disconnecting = false;
            ChannelsIn = new List<string>();
            ChannelUsers = new Dictionary<string, List<string>>();
        }

        public void Connect(string ircServer, int ircPort)
        {
            Server = ircServer;
            Port = ircPort;

            // Connect with the IRC server.
            Connection = new TcpClient(Server, Port);
            Connection.ReceiveTimeout = 120000;
            Stream = Connection.GetStream();
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream);

            // Authenticate our user
            string isInvisible = IsInvisble ? "8" : "0";
            SendCommand(String.Format("PASS {0}", NickPass));
            SetNick(Nick);
            SendCommand(String.Format("USER {0} {1} * :{2}", User, isInvisible, RealName));
            SendCommand("CAP REQ :twitch.tv/membership");

            Connected = true;

            // Listen for commands
            listenThread = new Thread(Listen);
            listenThread.Start();
        }

        public void JoinChannel(string channel)
        {
            var channels = channel.Split(',');

            foreach (var chan in channels)
            {
                SendCommand(String.Format("JOIN {0}", chan));
            }
        }

        public void LeaveChannel(string channel)
        {
            var channels = channel.Split(',');

            foreach (var chan in channels)
            {
                if (ChannelsIn.Contains(chan))
                    ChannelsIn.Remove(chan);

                SendCommand(String.Format("PART {0}", chan));
            }
        }

        public void GetChannelUsers(string channel)
        {
            if (!ChannelUsers.ContainsKey(channel))
            {
                ChannelUsers.Add(channel, new List<string>());
            }
            else
            {
                ChannelUsers[channel].Clear();
            }

            SendCommand(string.Format("NAMES {0}", channel));
        }

        private void SetNick(string nick)
        {
            SendCommand(string.Format("NICK {0}", nick));
        }

        private void SetPass(string nickPass)
        {
            SendCommand(string.Format("PRIVMSG NICKSERV IDENTIFY {0}", nickPass));
        }

        public void GhostNick(string nick, string nickPass)
        {
            SetNick(nick + DateTime.Now.Ticks);
            SendCommand(string.Format("PRIVMSG NICKSERV GHOST {0} {1}", nick, nickPass));
            SetNick(nick);
            SetNick(nickPass);
        }

        public void Disconnect()
        {
            disconnecting = true;
            Reader.Close();
        }

        public void SendNotice(string user, string message)
        {
            SendCommand(string.Format("NOTICE {0} :{1}", user, message));
        }

        public void SendMessage(string destination, string message)
        {
            SendCommand(string.Format("PRIVMSG {0} :{1}", destination, message));
        }

        public void SendCommand(string ircCommand)
        {
            if (Writer == null)
                Writer = new StreamWriter(Stream);

            Writer.WriteLine(ircCommand);
            Writer.Flush();
            if (CommandSent != null)
                CommandSent(ircCommand);
        }

        private void Listen()
        {
            while (!disconnecting)
            {
                string ircCommand;
                try
                {
                    while (!disconnecting && (ircCommand = Reader.ReadLine()) != null)
                    {
                        if (CommandReceived != null)
                        {
                            CommandReceived(ircCommand);
                        }

                        string[] commandParts = ircCommand.Split(' ');
                        if (commandParts[0].Substring(0, 1) == ":")
                        {
                            commandParts[0] = commandParts[0].Remove(0, 1);
                        }

                        if ((!commandParts[0].Contains("!")) && (commandParts[0].EndsWith(Server.Substring(Server.IndexOf('.'))) || commandParts[0].EndsWith("testserver.local")))
                        {
                            // Server message
                            switch (commandParts[1])
                            {
                                case "001":
                                    RegisterUser();
                                    break;
                                case "332":
                                    IrcTopic(commandParts);
                                    break;
                                case "333":
                                    IrcTopicOwner(commandParts);
                                    break;
                                case "353":
                                    IrcNamesList(commandParts);
                                    break;
                                case "366": /*this.IrcEndNamesList(commandParts);*/
                                    break;
                                case "372": /*this.IrcMOTD(commandParts);*/
                                    break;
                                case "376": /*this.IrcEndMOTD(commandParts);*/
                                    break;
                                default:
                                    IrcServerMessage(commandParts);
                                    break;
                            }
                        }
                        else if (commandParts[0] == "PING")
                        {
                            // Server PING, send PONG back
                            IrcPing(commandParts);
                        }
                        else
                        {
                            // Normal message
                            string commandAction = commandParts[1];
                            switch (commandAction)
                            {
                                case "JOIN":
                                    IrcJoin(commandParts);
                                    break;
                                case "PART":
                                    IrcPart(commandParts);
                                    break;
                                case "MODE":
                                    IrcMode(commandParts);
                                    break;
                                case "NICK":
                                    IrcNickChange(commandParts);
                                    break;
                                case "KICK":
                                    IrcKick(commandParts);
                                    break;
                                case "QUIT":
                                    IrcQuit(commandParts);
                                    break;
                                case "TOPIC":
                                    IrcTopicChanged(commandParts);
                                    break;
                            }
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    //This means we got disconnected, carry on.
                }
                catch (IOException)
                {
                    //Do nothing, this is probably because the stream closed.
                }
                catch (ArgumentOutOfRangeException)
                {
                    //Do nothing, this is probably because a staff member joined.
                }
                catch (ArgumentException)
                {
                    //Do nothing, this is probably safe to ignore.
                }
                catch (Exception ex)
                {
                    LogError(ex);

                    throw;
                }

                disconnecting = true;
                Writer.Close();
                Reader.Close();
                Connection.Close();
                ChannelsIn.Clear();

                if (Disconnected != null)
                {
                    Disconnected();
                }

                Thread.Sleep(100);
            }

            Writer = null;
            Connected = false;
            disconnecting = false;
        }

        private void IrcTopicChanged(string[] ircCommand)
        {
            string ircChannel = ircCommand[2];
            string ircUser = ircCommand[0].Split('!')[0];

            if (!ChannelsIn.Contains(ircChannel))
                ChannelsIn.Add(ircChannel);

            string ircTopic = "";
            for (int intI = 4; intI < ircCommand.Length; intI++)
            {
                ircTopic += ircCommand[intI] + " ";
            }
            if (TopicChanged != null)
            {
                try
                {
                    TopicChanged(ircUser, ircChannel, ircTopic.Remove(0, 1).Trim());
                }
                catch (ArgumentException)
                {
                    //TODO: figure out why this error happens.
                }
            }
        }

        public static void LogError(Exception ex)
        {
            using (var writer = new StreamWriter("ircClientErrorLog.txt", true))
            {
                writer.WriteLine(string.Format("[{0}]\r\n{1}\r\n\r\n", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                                               ex));
            }
        }

        private void RegisterUser()
        {
            SetPass(NickPass);
            JoinChannel(Channel);
        }

        private void IrcTopic(string[] ircCommand)
        {
            string ircChannel = ircCommand[3];

            if (!ChannelsIn.Contains(ircChannel))
                ChannelsIn.Add(ircChannel);

            string ircTopic = "";
            for (int intI = 4; intI < ircCommand.Length; intI++)
            {
                ircTopic += ircCommand[intI] + " ";
            }
            if (TopicSet != null)
            {
                TopicSet(ircChannel, ircTopic.Remove(0, 1).Trim());
            }
        }

        private void IrcTopicOwner(string[] ircCommand)
        {
            string ircChannel = ircCommand[3];
            string ircUser = ircCommand[4].Split('!')[0];
            string topicDate = ircCommand[5];
            if (TopicOwner != null)
            {
                TopicOwner(ircChannel, ircUser, topicDate);
            }
        }

        private void IrcNamesList(string[] ircCommand)
        {
            string userNames = "";
            for (int intI = 5; intI < ircCommand.Length; intI++)
            {
                userNames += ircCommand[intI] + " ";
            }
            if (NamesList != null)
            {
                NamesList(userNames.Remove(0, 1).Trim());
            }
        }

        private void IrcServerMessage(string[] ircCommand)
        {
            string serverMessage = "";
            for (int intI = 1; intI < ircCommand.Length; intI++)
            {
                serverMessage += ircCommand[intI] + " ";
            }
            if (ServerMessage != null)
            {
                ServerMessage(serverMessage.Trim());
            }
        }

        private void IrcPing(string[] ircCommand)
        {
            string pingHash = "";

            for (int intI = 1; intI < ircCommand.Length; intI++)
            {
                pingHash += ircCommand[intI] + " ";
            }
            SendCommand("PONG " + pingHash);
        }

        private void IrcJoin(string[] ircCommand)
        {
            string ircChannel = ircCommand[2];
            string ircUser = ircCommand[0].Split('!')[0];
            if (Join != null)
            {
                Join(ircChannel.Remove(0, 1), ircUser);
            }
        }

        private void IrcPart(string[] ircCommand)
        {
            string ircChannel = ircCommand[2];
            string ircUser = ircCommand[0].Split('!')[0];

            if (ircUser == Nick && ChannelsIn.Contains(ircChannel))
                ChannelsIn.Remove(ircChannel);

            if (Part != null)
            {
                Part(ircChannel, ircUser);
            }
        }

        private void IrcMode(string[] ircCommand)
        {
            string ircChannel = ircCommand[2];
            string ircUser = ircCommand[0].Split('!')[0];
            string userMode = "";
            for (int intI = 3; intI < ircCommand.Length; intI++)
            {
                userMode += ircCommand[intI] + " ";
            }
            if (userMode.Substring(0, 1) == ":")
            {
                userMode = userMode.Remove(0, 1);
            }
            if (Mode != null)
            {
                Mode(ircChannel, ircUser, userMode.Trim());
            }
        }

        private void IrcNickChange(string[] ircCommand)
        {
            string userOldNick = ircCommand[0].Split('!')[0];
            string userNewNick = ircCommand[2].Remove(0, 1);
            if (NickChange != null)
            {
                NickChange(userOldNick, userNewNick);
            }
        }

        private void IrcKick(string[] ircCommand)
        {
            string userKicker = ircCommand[0].Split('!')[0];
            string userKicked = ircCommand[3];
            string ircChannel = ircCommand[2];
            string kickMessage = "";

            if (userKicked == Nick && ChannelsIn.Contains(ircChannel))
                ChannelsIn.Remove(ircChannel);

            for (int intI = 4; intI < ircCommand.Length; intI++)
            {
                kickMessage += ircCommand[intI] + " ";
            }
            if (Kick != null)
            {
                Kick(ircChannel, userKicker, userKicked, kickMessage.Remove(0, 1).Trim());
            }
        }

        private void IrcQuit(string[] ircCommand)
        {
            string userQuit = ircCommand[0].Split('!')[0];
            string quitMessage = "";
            for (int intI = 2; intI < ircCommand.Length; intI++)
            {
                quitMessage += ircCommand[intI] + " ";
            }
            if (Quit != null)
            {
                Quit(userQuit, quitMessage.Remove(0, 1).Trim());
            }
        }
    }
}
