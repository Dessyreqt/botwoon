using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Botwoon.Data;
using Botwoon.Irc;

namespace BotwoonIrcBot
{
    public enum CommandType
    {
        None,
        NormalOutput,
        SilentOutput,
        JoinChannel,
        Queue,
        ClearQueue
    }

    public class CommandOutput
    {
        public List<string> Text { get; set; }
        public CommandType Type { get; set; }
        public bool MatchedCommand { get; set; }

        public CommandOutput()
        {
            Text = new List<string>();
        }
    }

    public class CommandHandler
    {
        private Dictionary<string, int> lastQuote;

        public CommandHandler()
        {
            lastQuote = new Dictionary<string, int>();
        }

        public CommandOutput ParseCommand(string sender, string fromChannel, string text, bool isTrustedUser)
        {
            var output = new CommandOutput { Type = CommandType.NormalOutput };

            try
            {
                HandleCommand(text, sender, fromChannel, output, isTrustedUser);
            }
            catch (Exception ex)
            {
                Controller.LogError(ex, text);

                output.Text.Add("I have encountered an error... please check the last command. This error has been logged.");
            }

            return output;
        }

        private void HandleCommand(string text, string sender, string fromChannel, CommandOutput output, bool isTrustedUser)
        {
            string command = text.Split(' ')[0].Substring(1);
            string parameters = "";
            if (text.IndexOf(' ') > 0)
                parameters = text.Substring(text.IndexOf(' ')).Trim();

            var matchedCommand = MatchCommand(command);

            // I want to keep "!add" as a command.
            if (matchedCommand.Count > 1 && matchedCommand.Contains("add"))
            {
                matchedCommand.Clear();
                matchedCommand.Add("add");
            }
                            
            if (matchedCommand.Count == 1)
            {
                output.MatchedCommand = true;
                switch (matchedCommand[0])
                {
                    case "add":
                        if (sender == "dessyreqt")
                        {
                            var newCommand = parameters.Substring(0, parameters.IndexOf(' ')).Trim();
                            var newText = parameters.Substring(parameters.IndexOf(' ')).Trim();

                            CustomCommands.Add(newCommand, newText);
                        }
                        break;
                    case "delete":
                        if (sender == "dessyreqt")
                        {
                            CustomCommands.Delete(parameters);
                        }
                        break;
                    case "drops":
                        if (parameters.Length > 2)
                            output.Text.AddRange(OutputDrops(parameters));
                        break;
                    case "summon":
                        var channel = SummonToChannel(parameters);

                        if (channel != "")
                        {
                            output.Text.Add(SummonToChannel(parameters));
                            output.Type = CommandType.JoinChannel;
                        }
                        break;
                    case "randnum":
                    case "randomnumber":
                        output.Text.Add(GetRandomNumber(parameters).ToString());
                        break;
                    case "randlist":
                    case "randomlist":
                    case "randomizelist":
                        output.Text.Add(GetRandomList(parameters));
                        break;
                    case "fight":
                        output.Text.AddRange(GetFightInfo(parameters));
                        break;
                    case "addquote":
                        if (isTrustedUser && Quotes.Validate(parameters))
                        {
                            Quotes.Add(parameters);
                            output.Text.Add(string.Format("Quote added! {0} quotes in the database.", Quotes.GetCount()));
                        }
                        break;
                    case "quote":
                        var quote = Quotes.GetRandom();
                        int quoteId;

                        if (parameters.Length > 0 && int.TryParse(parameters, out quoteId))
                        {
                            var tempQuote = new Quote {QuoteId = quoteId, QuoteText = Quotes.Get(quoteId)};
                            if (tempQuote.QuoteText != null)
                                quote = tempQuote;
                        }

                        lastQuote[fromChannel] = quote.QuoteId;
                        output.Text.Add(FormatQuote(quote));
                        break;
                    case "delquote":
                        if (isTrustedUser)
                        {
                            if (parameters.Length > 0 && int.TryParse(parameters, out quoteId))
                            {
                                Quotes.Delete(quoteId);
                                output.Text.Add("That terrible quote has been deleted. Good riddance!");
                            }
                            else if (lastQuote.ContainsKey(fromChannel))
                            {
                                Quotes.Delete(lastQuote[fromChannel]);
                                output.Text.Add("That terrible quote has been deleted. Good riddance!");
                            }
                        }
                        break;
                   case "insult":
                        var insultTo = GetInsultTarget(sender, fromChannel, parameters);
                        var insult = Insults.Get();
                        output.Text.Add(string.Format("{0}, you're nothing but {1}", insultTo, insult));
                        break;
                   case "compliment":
                        var complimentTo = GetInsultTarget(sender, fromChannel, parameters);
                        var compliment = Insults.Get();
                        output.Text.Add(string.Format("{0}, you're decent for {1}", complimentTo, compliment));
                        break;
                    case "listquotes":
                        if (isTrustedUser)
                        {
                            output.Text.Add(ListQuotes());
                        }
                        break;
                    case "addc":
                        if (isTrustedUser)
                        {
                            var newCommand = string.Format("{0}+{1}", parameters.Substring(0, parameters.IndexOf(' ')).Trim().Replace("+", ""), fromChannel);
                            var newText = parameters.Substring(parameters.IndexOf(' ')).Trim();

                            CustomCommands.Add(newCommand, newText);
                        }
                        break;
                    case "delc":
                        if (isTrustedUser)
                        {
                            var commandName = string.Format("{0}+{1}", parameters, fromChannel);
                            CustomCommands.Delete(commandName);
                        }
                        break;
                }
            }
            else if (matchedCommand.Count == 0)
            {
                var commandOutput = CustomCommands.Get(string.Format("{0}+{1}", command, fromChannel));

                if (commandOutput != null)
                {
                    commandOutput = commandOutput.Replace("{sender}", sender);
                    commandOutput = commandOutput.Replace("{streamer}", fromChannel.Remove(0, 1));
                    commandOutput = commandOutput.Replace("{oatsavgceres}", OatsCeres.GetAvgTime());

                    output.Text.Add(commandOutput);
                }
                else if ((commandOutput = CustomCommands.Get(command)) != null)
                {
                    commandOutput = commandOutput.Replace("{sender}", sender);
                    commandOutput = commandOutput.Replace("{streamer}", fromChannel.Remove(0, 1));

                    output.Text.Add(commandOutput);
                }
            }
        }

        private static string FormatQuote(Quote quote)
        {
            return string.Format("[{0}] {1}", quote.QuoteId, quote.QuoteText);
        }

        private string ListQuotes()
        {
            var quotes = Quotes.GetAllQuotes();
            var pasteData = new StringBuilder();

            foreach (var quote in quotes)
            {
                pasteData.AppendLine(FormatQuote(quote));
            }

            string response = PostPasteBin("Botwoon Quotes", pasteData.ToString());

            return string.Format("Quote list posted at {0}", response);
        }

        private static string GetInsultTarget(string sender, string fromChannel, string parameters)
        {
            string insultTo = fromChannel == "#dessyreqt" || fromChannel == "#botwoon" ? sender : fromChannel.Remove(0, 1);

            if (parameters.Length > 2)
            {
                insultTo = parameters;
            }

            insultTo = Regex.Replace(insultTo, @"[!;:,.<>?/\\\[\]@#$%^&*()+=-]", "");

            if (insultTo.ToLower().Contains("botwoon"))
            {
                insultTo = sender;
            }

            return insultTo;
        }

        private IEnumerable<string> GetFightInfo(string parameters)
        {
            return FightInfo.Get(parameters);
        }

        private IEnumerable<string> OutputDrops(string parameters)
        {
            var retVal = new List<string>();
            List<DropTable> drops = Drops.GetDrops(parameters);

            if (parameters.ToUpper() == "BOTWOON")
            {
                retVal.Add("Drops for Botwoon (first round) - Left hole: 100.00");
            }
            else if (parameters.ToUpper() == "MOTHER BRAIN")
            {
                retVal.Add("Drops for Mother Brain - Hyper Beam: 100.00, Roasted Critters: 100.00");
            }
            else if (drops.Count == 0)
            {
                retVal.Add("Could not find enemy. Please look here: http://deanyd.net/sm/index.php?title=Enemy_item_drops");
                UnknownInput.Enter(string.Format("!drops {0}", parameters));
            }
            else
            {
                foreach (var drop in drops)
                {
                    var dropString = new StringBuilder();
                    dropString.AppendFormat("Drops for {0} - ", drop.Name);
                    if (drop.Nothing > 0) dropString.AppendFormat("Nothing: {0:0.00}, ", drop.Nothing);
                    if (drop.Energy > 0) dropString.AppendFormat("Energy: {0:0.00}, ", drop.Energy);
                    if (drop.BigEnergy > 0) dropString.AppendFormat("Big Energy: {0:0.00}, ", drop.BigEnergy);
                    if (drop.Missile > 0) dropString.AppendFormat("Missile: {0:0.00}, ", drop.Missile);
                    if (drop.SuperMissile > 0) dropString.AppendFormat("Super Missile: {0:0.00}, ", drop.SuperMissile);
                    if (drop.PowerBombs > 0) dropString.AppendFormat("Power Bombs: {0:0.00}, ", drop.PowerBombs);
                    dropString.Remove(dropString.Length - 2, 2);

                    retVal.Add(dropString.ToString());
                }
            }

            return retVal;
        }

        private int GetRandomNumber(string parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                return Randomizer.GetRandomizer().GetRandomNumber(999999);
            }

            var parameterArray = parameters.Split(' ');
            int upperBound;

            if (parameterArray.Length > 1)
            {

                if (int.TryParse(parameterArray[1], out upperBound))
                {
                    int lowerBound;

                    if (int.TryParse(parameterArray[0], out lowerBound))
                    {
                        return Randomizer.GetRandomizer().GetRandomNumber(lowerBound, upperBound);
                    }

                    return Randomizer.GetRandomizer().GetRandomNumber(upperBound);
                }

                if (int.TryParse(parameterArray[0], out upperBound))
                {
                    return Randomizer.GetRandomizer().GetRandomNumber(upperBound);
                }

                return Randomizer.GetRandomizer().GetRandomNumber(999999);
            }

            if (int.TryParse(parameterArray[0], out upperBound))
            {
                return Randomizer.GetRandomizer().GetRandomNumber(upperBound);
            }

            return Randomizer.GetRandomizer().GetRandomNumber(999999);
        }

        private static string GetRandomList(string parameters)
        {
            var array = parameters.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var list = array.Select(item => item.Trim()).ToList();

            Randomizer.GetRandomizer().RandomizeList(ref list);

            var retVal = new StringBuilder();

            foreach (var racer in list)
            {
                retVal.Append(racer);
                retVal.Append("; ");
            }

            retVal.Remove(retVal.Length - 2, 2);

            return retVal.ToString();
        }


        private static string PostPasteBin(string title, string pasteData)
        {
            var postData = new StringBuilder();
            postData.AppendFormat("api_option=paste");
            postData.AppendFormat("&api_dev_key={0}", ConfigurationManager.AppSettings["PastebinApiKey"]);
            postData.AppendFormat("&api_paste_code={0}", Uri.EscapeUriString(pasteData));
            postData.AppendFormat("&api_paste_private=1");
            postData.AppendFormat("&api_paste_name={0}", Uri.EscapeDataString(title));
            postData.AppendFormat("&api_paste_expire_date=10M");
            postData.AppendFormat("&api_paste_format=text");
            postData.AppendFormat("&api_user_key=");

            var encoding = new UTF8Encoding();
            var data = encoding.GetBytes(postData.ToString());

            //prepare web request...
            var pasteRequest = WebRequest.Create("http://pastebin.com/api/api_post.php");
            pasteRequest.Method = "POST";
            pasteRequest.ContentType = "application/x-www-form-urlencoded";
            pasteRequest.ContentLength = data.Length;

            //send request
            var newStream = pasteRequest.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            //
            var response = pasteRequest.GetResponse();
            var readStream = response.GetResponseStream();
            var reader = new StreamReader(readStream);
            return reader.ReadToEnd();
        }

        private static List<string> MatchCommand(string command)
        {
            if (command.Length < 2)
                return new List<string>();

            var commands = new[]
                               {
                                   "add",
                                   "addc",
                                   "addquote",
                                   "compliment",
                                   "delc",
                                   "delete",
                                   "delquote",
                                   "drops",
                                   "fight",
                                   "insult",
                                   "listquotes",
                                   "quote",
                                   "randlist",
                                   "randnum",
                                   "randomizelist",
                                   "randomlist",
                                   "randomnumber",
                                   "summon",
                               };

            return commands.Where(com => com.StartsWith(command.Trim().ToLower())).ToList();
        }

        private static IEnumerable<string> SendHelp()
        {
            const string helpText = @"Please read the documentation at http://pastebin.com/pKtFfC5j
In the meantime, here are some commands to get you started:
;remember {tag} {data} - stores {data} under {tag} for your username.
;recall {tag} - outputs all information under {tag} for your username in the order entered.
;forget {tag} {slot#} - forgets data in {slot#} under {tag} for your username. Leave out {slot#} to forget the whole tag.
;listtags - list all the tags stored under your username.";

            return helpText.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static string SummonToChannel(string parameters)
        {
            var toChannel = parameters.Split(' ')[0];

            if (toChannel.StartsWith("#"))
            {
                return toChannel;
            }

            return "";
        }
    }
}
