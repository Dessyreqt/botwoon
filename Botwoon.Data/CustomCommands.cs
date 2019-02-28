using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Botwoon.Data
{
    public class CustomCommands
    {
        public static void Add(string command, string text)
        {
            Delete(command);

            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("insert into CustomCommands (Name, Text) values (@name, @text)", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@name", command);
                cmd.Parameters.AddWithValue("@text", text);
                cmd.ExecuteNonQuery();
            }
        }

        public static void Delete(string command)
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("delete from CustomCommands where Name = @name", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@name", command);
                cmd.ExecuteNonQuery();
            }
        }

        public static string Get(string command)
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select * from CustomCommands where Name = @name", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@name", command);

                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return ProcessOutput(reader["Text"].ToString());
                }
            }

            return null;
        }

        private static string ProcessOutput(string output)
        {
            var newOutput = output;

            foreach (Match match in Regex.Matches(newOutput, "{randtime (?<Time>\\d+)}"))
            {
                var randTime = Randomizer.GetRandomizer().GetRandomTime(int.Parse(match.Groups["Time"].Value));

                newOutput = newOutput.Replace(match.Value, randTime.Hours > 0 ? randTime.ToString("h':'mm':'ss") : randTime.ToString("m':'ss"));
            }

            foreach (Match match in Regex.Matches(newOutput, "{randnum (?<UpperBound>\\d+)}"))
            {
                var randNum = Randomizer.GetRandomizer().GetRandomNumber(int.Parse(match.Groups["UpperBound"].Value));

                newOutput = newOutput.Replace(match.Value, randNum.ToString());
            }

            foreach (Match match in Regex.Matches(newOutput, "{randnum (?<LowerBound>\\d+) (?<UpperBound>\\d+)}"))
            {
                var randNum = Randomizer.GetRandomizer().GetRandomNumber(int.Parse(match.Groups["LowerBound"].Value), int.Parse(match.Groups["UpperBound"].Value));

                newOutput = newOutput.Replace(match.Value, randNum.ToString());
            }

            return newOutput;
        }
    }
}
