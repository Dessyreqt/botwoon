using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Botwoon.Data;

namespace BotwoonIrcBot
{
    public class Trigger
    {
        public string MatchPattern { get; set; }
        public string IgnorePattern { get; set; }
        public string SenderPattern { get; set; }
        public string IgnoreSenderPattern { get; set; }
        public string ChannelPattern { get; set; }
        public string IgnoreChannelPattern { get; set; }
        public bool TrustedSenderOnly { get; set; }
        public int Frequency { get; set; }
        public string Response { get; set; }
        public Func<string, string, string, bool> CustomResponse { get; set; }

        public string GetResponse(string fromChannel, string sender, string text, bool isTrustedUser)
        {
            if (string.IsNullOrWhiteSpace(Response))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(MatchPattern))
            {
                MatchPattern = ".*";
            }
            if (string.IsNullOrWhiteSpace(IgnorePattern))
            {
                IgnorePattern = "^$";
            }
            if (string.IsNullOrWhiteSpace(SenderPattern))
            {
                SenderPattern = ".*";
            }
            if (string.IsNullOrWhiteSpace(IgnoreSenderPattern))
            {
                IgnoreSenderPattern = "^$";
            }
            if (string.IsNullOrWhiteSpace(ChannelPattern))
            {
                ChannelPattern = ".*";
            }
            if (string.IsNullOrWhiteSpace(IgnoreChannelPattern))
            {
                IgnoreChannelPattern = "^$";
            }

            if (Frequency == 0)
            {
                Frequency = 100;
            }

            var isTextMatch = Regex.IsMatch(text, MatchPattern, RegexOptions.IgnoreCase);
            var isTextMatchIgnore = Regex.IsMatch(text, IgnorePattern, RegexOptions.IgnoreCase);
            var isSenderMatch = Regex.IsMatch(sender, SenderPattern, RegexOptions.IgnoreCase);
            var isSenderMatchIgnore = Regex.IsMatch(sender, IgnoreSenderPattern, RegexOptions.IgnoreCase);
            var isChannelMatch = Regex.IsMatch(fromChannel, ChannelPattern, RegexOptions.IgnoreCase);
            var isChannelMatchIgnore = Regex.IsMatch(fromChannel, IgnoreChannelPattern, RegexOptions.IgnoreCase);
            var hitFrequency = Randomizer.GetRandomizer().GetRandomNumber(100) <= Frequency;
            var senderValid = !TrustedSenderOnly || isTrustedUser;

            if (isTextMatch && !isTextMatchIgnore && isSenderMatch && !isSenderMatchIgnore && isChannelMatch && !isChannelMatchIgnore && hitFrequency && senderValid)
            {
                return FormatResponse(fromChannel, sender, text);
            }

            return null;
        }

        private string FormatResponse(string fromChannel, string sender, string text)
        {
            var retVal = Response;

            retVal = retVal.Replace("{sender}", sender);
            retVal = retVal.Replace("{SENDER}", sender.ToUpper());
            retVal = retVal.Replace("{streamer}", fromChannel.Remove(0, 1));
            retVal = retVal.Replace("{STREAMER}", fromChannel.Remove(0, 1).ToUpper());
            retVal = retVal.Replace("{oatsavgceres}", OatsCeres.GetAvgTime());

            foreach (Match match in Regex.Matches(retVal, "{randtime (?<Time>\\d+)}"))
            {
                var randTime = Randomizer.GetRandomizer().GetRandomTime(int.Parse(match.Groups["Time"].Value));

                retVal = retVal.Replace(match.Value, randTime.Hours > 0 ? randTime.ToString("h':'mm':'ss") : randTime.ToString("m':'ss"));
            }

            foreach (Match match in Regex.Matches(retVal, "{randnum (?<UpperBound>\\d+)}"))
            {
                var randNum = Randomizer.GetRandomizer().GetRandomNumber(int.Parse(match.Groups["UpperBound"].Value));

                retVal = retVal.Replace(match.Value, randNum.ToString());
            }

            foreach (Match match in Regex.Matches(retVal, "{randnum (?<LowerBound>\\d+) (?<UpperBound>\\d+)}"))
            {
                var randNum = Randomizer.GetRandomizer().GetRandomNumber(int.Parse(match.Groups["LowerBound"].Value), int.Parse(match.Groups["UpperBound"].Value));

                retVal = retVal.Replace(match.Value, randNum.ToString());
            }

            return retVal;
        }

    }
}
