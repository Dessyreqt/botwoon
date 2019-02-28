using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Botwoon.Data;
using Botwoon.Irc;
// ReSharper disable All

namespace BotwoonIrcBot
{
    public delegate void CommandHandled(string user, string command);
    public delegate void OutputSent(string channel, string output);
    public delegate void CommandReceived(string command);
    public delegate void CommandSent(string command);
    public delegate void TopicSet(string fromChannel, string topic);
    public delegate void TopicOwner(string fromChannel, string user, string topicDate);
    public delegate void NamesList(string userNames);
    public delegate void ServerMessage(string serverMessage);
    public delegate void Join(string fromChannel, string user);
    public delegate void JoinedChannel(string channel);
    public delegate void Part(string fromChannel, string user);
    public delegate void Mode(string fromChannel, string user, string userMode);
    public delegate void NickChange(string oldNick, string newNick);
    public delegate void Kick(string fromChannel, string kickingUser, string kickedUser, string kickMessage);
    public delegate void Quit(string user, string quitMessage);
    public delegate void ChannelsInChanged();
    public delegate void Disconnected();

    public class Controller
    {
        private IrcClient ircClient;
        private DateTime startTime;
        private Thread messageQueue;
        private List<string> messageList;
        private Dictionary<string, string> lastLine;
        private CommandHandler handler;
        private Dictionary<string, List<string>> opsList;
        private readonly List<Trigger> triggers = new List<Trigger>
                                            {
                                                new Trigger { MatchPattern = "rekt", IgnorePattern = "[a-z]{2,6}rekt", IgnoreSenderPattern="nutbotty", Response = "rеqt*", Frequency = 50 },
                                                new Trigger { MatchPattern = "lttp", Response = ":Oooo*" },
                                                new Trigger { MatchPattern = "^(nice|good|great|solid|amazing|awesome|fine|best).+botwoon$", Response = "THANKS {SENDER}" },
                                                new Trigger { MatchPattern = "^(lol|lmfao|rofl|lmao).+botwoon$", Response = "I KNOW RIGHT, {SENDER}?" },
                                                new Trigger { MatchPattern = "rekt", SenderPattern = "nutbotty", Response = "What well thought-out and totally appropriate response to an otherwise innocent joke. I'm so glad there's a bot like you around to enforce the correct incorrect spellings of words. Thanks!" },
                                                new Trigger { MatchPattern = "You have 50 seconds to place your guess.", SenderPattern = "ceresbot", ChannelPattern = "#oatsngoats", Response = "!guess {oatsavgceres}", CustomResponse = ((fromChannel, sender, text) =>
                                                {
                                                    OatsCeres.GetOatsCeres().ActiveCeres = true;

                                                    return false;
                                                })},
                                                new Trigger { MatchPattern = "^!endceres", TrustedSenderOnly = true, Response = "!", CustomResponse = ((fromChannel, sender, text) => 
                                                {
                                                    if (OatsCeres.GetOatsCeres().ActiveCeres)
                                                    {
                                                        var ceresTime = Regex.Replace(text.Split(' ')[1], "[^0-9]", "");

                                                        if (!string.IsNullOrWhiteSpace(ceresTime))
                                                        {
                                                            var ceresTimeInt = int.Parse(ceresTime);
                                                            if (4500 < ceresTimeInt && ceresTimeInt < 4800)
                                                            {
                                                                OatsCeres.AddTime(ceresTime);
                                                            }

                                                            OatsCeres.GetOatsCeres().ActiveCeres = false;
                                                        }
                                                    }

                                                    return true;
                                                })},
                                            };

        public string Nick { get { return ircClient != null ? ircClient.Nick : null; } }
        public List<string> ChannelsIn { get { return ircClient != null ? ircClient.ChannelsIn : null; } }
        public bool Connected { get { return ircClient.Connected; } }
        public bool EndProcessing { get; set; }

        public event CommandHandled CommandHandled;
        public event OutputSent OutputSent;
        public event CommandReceived CommandReceived;
        public event CommandSent CommandSent;
        public event TopicSet TopicSet;
        public event TopicOwner TopicOwner;
        public event NamesList NamesList;
        public event ServerMessage ServerMessage;
        public event Join Join;
        public event JoinedChannel JoinedChannel;
        public event Part Part;
        public event Mode Mode;
        public event NickChange NickChange;
        public event Kick Kick;
        public event Quit Quit;
        public event ChannelsInChanged ChannelsInChanged;
        public event Disconnected Disconnected;

        public void StartController(string server, int port, string channel)
        {
            startTime = DateTime.UtcNow;
            ircClient = new IrcClient(ConfigurationManager.AppSettings["nick"], ConfigurationManager.AppSettings["nickPass"], channel, "Botwoon");
            opsList = new Dictionary<string, List<string>>();

            ircClient.CommandReceived += ircClient_CommandReceived;
            ircClient.CommandSent += ircClient_CommandSent;
            ircClient.TopicChanged += ircClient_TopicChanged;
            ircClient.TopicSet += ircClient_TopicSet;
            ircClient.TopicOwner += ircClient_TopicOwner;
            ircClient.NamesList += ircClient_NamesList;
            ircClient.ServerMessage += ircClient_ServerMessage;
            ircClient.Join += ircClient_Join;
            ircClient.Part += ircClient_Part;
            ircClient.Mode += ircClient_Mode;
            ircClient.NickChange += ircClient_NickChange;
            ircClient.Kick += ircClient_Kick;
            ircClient.Quit += ircClient_Quit;
            ircClient.Disconnected += ircClient_Disconnected;

            ircClient.Connect(server, port);

            EndProcessing = false;
            messageList = new List<string>();
            lastLine = new Dictionary<string, string>();

            if (messageQueue == null)
            {
                messageQueue = new Thread(OutputMessages);
                messageQueue.Start();
            }
        }

        private void OutputMessages()
        {
            while (!EndProcessing)
            {
                Thread.Sleep(2000);
                if (ircClient.Connected)
                {
                    lock (messageList)
                    {
                        if (messageList.Count > 0)
                        {
                            var nextItem = messageList[0];
                            var fromChannel = nextItem.Substring(0, nextItem.IndexOf(":"));
                            var line = nextItem.Substring(nextItem.IndexOf(":") + 1);

                            if (lastLine.ContainsKey(fromChannel) && lastLine[fromChannel] == line)
                            {
                                line += " ";
                            }

                            ircClient.SendMessage(fromChannel, line);

                            lastLine[fromChannel] = line;
                            messageList.RemoveAt(0);
                        }
                    }
                }
            }
        }

        private void ircClient_TopicChanged(string sender, string fromChannel, string topic)
        {
        }

        public void StopController()
        {
            if (ircClient != null)
            {
                ircClient.Disconnect();
                EndProcessing = true;
                messageQueue = null;
            }
        }

        private void ircClient_CommandReceived(string command)
        {
            if (CommandReceived != null)
                CommandReceived(command);

            if (command.StartsWith(string.Format(":{0}", ircClient.Server)))
            {
                if (command.Contains(string.Format("353 {0}", ircClient.User)))
                {
                    AddChannelUsers(command);
                }

                return;
            }

            int msgIndex = command.IndexOf("NOTICE");
            if (msgIndex > -1 && command.IndexOf('!') > -1)
            {
                var sender = command.Substring(1, command.IndexOf('!') - 1);
                var receivedMessage = command.Substring(msgIndex);
                var text = receivedMessage.Substring(receivedMessage.IndexOf(':') + 1);

                if (text.Contains("#srl-") && sender == "RaceBot")
                    JoinRaceChannel(text);
            }

            msgIndex = command.IndexOf("PRIVMSG");
            if (msgIndex > -1 && command.IndexOf('!') > -1)
            {
                var sender = command.Substring(1, command.IndexOf('!') - 1);
                var receivedMessage = command.Substring(msgIndex);
                var fromChannel = receivedMessage.Substring(8, receivedMessage.IndexOf(':') - 9);
                var text = receivedMessage.Substring(receivedMessage.IndexOf(':') + 1);

                if (text.StartsWith("\u0001") && text.EndsWith("\u0001"))
                    HandleCtcpRequest(sender, text);

                // every once in a while, just echo something
                if (fromChannel.StartsWith("#") && Randomizer.GetRandomizer().GetRandomNumber(2300) == 23)
                {
                    var output = OutputEcho(text);
                    messageList.Add(string.Format("{0}:{1}", fromChannel, output));
                    if (OutputSent != null)
                        OutputSent(fromChannel, output);
                }

                if (ConfigurationManager.AppSettings["handleCommands"] != "false")
                {
                    foreach (var trigger in triggers)
                    {
                        var response = trigger.GetResponse(fromChannel, sender, text, IsTrustedUser(fromChannel, sender));

                        if (response != null)
                        {
                            var responseHandled = trigger.CustomResponse != null &&
                                                  trigger.CustomResponse(fromChannel, sender, text);

                            if (!responseHandled)
                            {
                                messageList.Add(string.Format("{0}:{1}", fromChannel, response));
                                if (OutputSent != null)
                                    OutputSent(fromChannel, response);
                            }
                        }
                    }
                }

                if (!text.StartsWith("!"))
                {
                    return;
                }

                if (!fromChannel.StartsWith("#"))
                    fromChannel = sender;

                var handlerThread = new Thread(() => HandleCommand(text, sender, fromChannel));
                handlerThread.Start();
            }
        }

        private void ircClient_CommandSent(string command)
        {
            if (CommandSent != null)
                CommandSent(command);
        }

        private void HandleCommand(string text, string sender, string fromChannel)
        {
            if (ConfigurationManager.AppSettings["handleCommands"] == "false")
                return;
            
            if (handler ==  null)
            {
                handler = new CommandHandler();
            }

            var output = handler.ParseCommand(sender, fromChannel, text, IsTrustedUser(fromChannel, sender));

            if (output.MatchedCommand && CommandHandled != null)
            {
                CommandHandled(sender, text);
            }

            switch (output.Type)
            {
                default:
                    foreach (var line in output.Text)
                    {
                        switch (output.Type)
                        {
                            case CommandType.SilentOutput:
                                ircClient.SendNotice(sender, line);
                                break;
                            case CommandType.NormalOutput:
                                messageList.Add(string.Format("{0}:{1}", fromChannel, line));
                                break;
                            case CommandType.JoinChannel:
                                ircClient.JoinChannel(output.Text[0]);
                                if (JoinedChannel != null)
                                    JoinedChannel(output.Text[0]);
                                break;
                        }

                        if (OutputSent != null)
                        {
                            OutputSent(fromChannel, line);
                        }
                    }
                    break;
            }
        }

        private string OutputEcho(string text)
        {
            return Regex.Replace(string.Format("YEAH {0}", text.ToUpper()), "[,:;.?!]", "");
        }


        private void AddChannelUsers(string command)
        {
            var userlist = command.Substring(command.IndexOf('#'));
            var inChannel = userlist.Substring(0, userlist.IndexOf(':') - 1);
            var users = userlist.Substring(userlist.IndexOf(':') + 1);

            if (!ircClient.ChannelUsers.ContainsKey(inChannel))
            {
                ircClient.ChannelUsers.Add(inChannel, new List<string>());
            }

            ircClient.ChannelUsers[inChannel].AddRange(users.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void HandleCtcpRequest(string sender, string text)
        {
            if (text.ToLower().StartsWith("\u0001ping"))
            {
                ircClient.SendNotice(sender, text);
                return;
            }

            if (text.ToLower().StartsWith("\u0001action"))
            {
                return;
            }

            switch (text.ToLower().Substring(1, text.Length - 2).Trim())
            {
                case "clientinfo":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO CLIENTINFO FINGER HOST OS SOURCE TIME USERINFO VERSION\u0001");
                    return;
                case "clientinfo clientinfo":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO CLIENTINFO gives information on available CTCP commands\u0001");
                    return;
                case "clientinfo finger":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO FINGER returns the user's full name and uptime\u0001");
                    return;
                case "clientinfo host":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO HOST returns the client's host application name and version\u0001");
                    return;
                case "clientinfo os":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO PING echos the parameter passed to the client\u0001");
                    return;
                case "clientinfo source":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO SOURCE returns an address where you can obtain the client\u0001");
                    return;
                case "clientinfo time":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO TIME gives the local date and time for the client\u0001");
                    return;
                case "clientinfo userinfo":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO USERINFO returns the information set by the user\u0001");
                    return;
                case "clientinfo version":
                    ircClient.SendNotice(sender, "\u0001CLIENTINFO VERSION returns the client's version\u0001");
                    return;
                case "finger":
                    ircClient.SendNotice(sender, string.Format("\u0001FINGER :I'm Mnemosyne, an IRC bot. I've been connected for {0}\u0001", DateTime.Now - startTime));
                    return;
                case "host":
                    ircClient.SendNotice(sender, string.Format("\u0001{0}\u0001", Environment.Version));
                    return;
                case "os":
                    ircClient.SendNotice(sender, string.Format("\u0001{0}\u0001", Environment.OSVersion.VersionString));
                    return;
                case "source":
                    ircClient.SendNotice(sender, "\u0001SOURCE The client for Mnemosyne is custom and isn't available right now. Sorry.\u0001");
                    return;
                case "time":
                    ircClient.SendNotice(sender, string.Format("\u0001TIME {0}\u0001", DateTime.Now));
                    return;
                case "userinfo":
                    ircClient.SendNotice(sender, "\u0001USERINFO :Botwoon is a bot written by Dessyreqt in C#, using SQL Server CE as a backend for storage.\u0001");
                    return;
                case "version":
                    ircClient.SendNotice(sender, "\u0001VERSION Botwoon IRC bot. Custom interface written by Dessyreqt.\u0001");
                    return;
            }

            if (text.ToLower().StartsWith("\u0001clientinfo "))
            {
                ircClient.SendNotice(sender, string.Format("\u0001CLIENTINFO {0} is not a valid CTCP function for this client\u0001", text.Substring(1, text.Length - 2).Substring(11)));
                return;
            }

            ircClient.SendNotice(sender, string.Format("\u0001ERRMSG {0} :Unknown query.\u0001", text.Substring(1, text.Length - 2)));
        }

        private void JoinRaceChannel(string text)
        {
            var joinedChannel = text.Substring(text.IndexOf("#srl-"), 10);

            if (!ircClient.ChannelsIn.Contains(joinedChannel))
            {
                ircClient.JoinChannel(joinedChannel);
            }

            if (ChannelsInChanged != null)
                ChannelsInChanged();
        }

        private void ircClient_TopicSet(string fromChannel, string topic)
        {
            if (TopicSet != null)
                TopicSet(fromChannel, topic);
            //notifier.Notify(text, sender, fromChannel);
        }

        private void ircClient_TopicOwner(string fromChannel, string user, string topicDate)
        {
            if (TopicOwner != null)
                TopicOwner(fromChannel, user, topicDate);
        }

        private void ircClient_NamesList(string userNames)
        {
            if (NamesList != null)
                NamesList(userNames);
        }

        private void ircClient_ServerMessage(string serverMessage)
        {
            if (ServerMessage != null)
                ServerMessage(serverMessage);

            if (serverMessage.EndsWith("Nickname is already in use."))
            {
                ircClient.GhostNick(ircClient.Nick, ircClient.NickPass);
            }
        }

        private void ircClient_Join(string fromChannel, string user)
        {
            if (Join != null)
                Join(fromChannel, user);
        }

        private void ircClient_Part(string fromChannel, string user)
        {
            if (Part != null)
                Part(fromChannel, user);

            if (user == "RaceBot" && fromChannel.StartsWith("#srl-"))
                LeaveRaceChannel(fromChannel);
        }

        private void ircClient_Mode(string fromChannel, string user, string userMode)
        {
            if (Mode != null)
                Mode(fromChannel, user, userMode);

            if (!opsList.ContainsKey(fromChannel))
            {
                opsList[fromChannel] = new List<string>();
            }

            var userModes = userMode.Split(' ');
            var change = userModes[0];
            var userName = userModes[1];

            if (change == "-o")
            {
                opsList[fromChannel].Remove(userName.ToLower());
            }
            if (change == "+o")
            {
                opsList[fromChannel].Add(userName.ToLower());
            }
        }

        private bool IsTrustedUser(string fromChannel, string userName)
        {
            if (userName.ToLower() == "dessyreqt")
            {
                return true;
            }

            if (!opsList.ContainsKey(fromChannel))
            {
                return false;
            }

            return opsList[fromChannel].Contains(userName.ToLower());
        }

        private void ircClient_NickChange(string oldNick, string newNick)
        {
            if (NickChange != null)
                NickChange(oldNick, newNick);
        }

        private void ircClient_Kick(string fromChannel, string kickingUser, string kickedUser, string kickMessage)
        {
            if (Kick != null)
                Kick(fromChannel, kickingUser, kickedUser, kickMessage);
        }

        private void ircClient_Quit(string user, string quitMessage)
        {
            if (Quit != null)
                Quit(user, quitMessage);
        }

        private void ircClient_Disconnected()
        {
            if (Disconnected != null)
                Disconnected();
        }

        private void LeaveRaceChannel(string leftChannel)
        {
            if (ChannelsInChanged != null)
                ChannelsInChanged();

            if (ircClient.ChannelsIn.Contains(leftChannel))
            {
                ircClient.LeaveChannel(leftChannel);
            }
        }

        public void SendMessage(string destination, string message)
        {
            ircClient.SendMessage(destination, message);
        }

        public static void LogError(Exception ex, string text)
        {
            try
            {
                using (var writer = new StreamWriter("errorLog.txt", true))
                {
                    writer.WriteLine("[{0}]\r\nCommand: {2}\r\n{1}\r\n\r\n", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), ex, text);
                }
            }
            catch (IOException)
            {
                //if we can't log the error, we shouldn't explode over it.
            }
        }
    }
}
