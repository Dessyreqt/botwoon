using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace Botwoon.Data
{
    public class Quote
    {
        public int QuoteId { get; set; }
        public string QuoteText { get; set; }
    }

    public class Quotes
    {
        public static void Add(string text)
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("insert into Quotes (QuoteText) values (@text)", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@text", text);
                cmd.ExecuteNonQuery();
            }
        }

        public static string Get(int quoteId)
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select * from Quotes where QuoteId = @quoteId", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@quoteId", quoteId);

                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return reader["QuoteText"].ToString();
                }
            }

            return null;
        }

        public static Quote GetRandom()
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select * from Quotes order by newid()", conn))
            {
                conn.Open();

                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Quote { QuoteId = int.Parse(reader["QuoteId"].ToString()), QuoteText = reader["QuoteText"].ToString() };
                }
            }

            return null;
        }

        public static int GetCount()
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select count(*) as QuoteCount from Quotes", conn))
            {
                conn.Open();

                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return int.Parse(reader["QuoteCount"].ToString());
                }
            }

            return 0;
        }

        public static bool Validate(string parameters)
        {
            var retVal = true;

            if (parameters.StartsWith("/"))
            {
                retVal = false;
            }
            if (parameters.StartsWith("."))
            {
                retVal = false;
            }

            return retVal;
        }

        public static void Delete(int quoteId)
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("delete from Quotes where QuoteId = @quoteId", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@quoteId", quoteId);

                cmd.ExecuteNonQuery();
            }

            RenumberTable();
        }

        public static void RenumberTable()
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            {
                using (var cmd = new SqlCeCommand("alter table Quotes drop constraint PK_Quotes", conn))
                {
                    conn.Open();

                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCeCommand("alter table Quotes drop column QuoteId", conn))
                {
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCeCommand("alter table Quotes add column QuoteId bigint identity(1,1)", conn))
                {
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = new SqlCeCommand("alter table quotes add constraint PK_Quotes primary key (QuoteId)", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<Quote> GetAllQuotes()
        {
            var retVal = new List<Quote>();

            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select * from Quotes order by QuoteId", conn))
            {
                conn.Open();

                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    retVal.Add(new Quote { QuoteId = int.Parse(reader["QuoteId"].ToString()), QuoteText = reader["QuoteText"].ToString() }) ;
                }
            }

            return retVal;
        }
    }
}
