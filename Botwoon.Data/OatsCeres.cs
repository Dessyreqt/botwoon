using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace Botwoon.Data
{
    public class OatsCeres
    {
        private static readonly OatsCeres instance = new OatsCeres();

        public bool ActiveCeres { get; set; }

        private OatsCeres()
        {   
        }

        public static OatsCeres GetOatsCeres()
        {
            return instance;
        }

        public static string GetAvgTime()
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select time from oatsceres order by insertdate desc", conn))
            {
                conn.Open();

                var reader = cmd.ExecuteReader();
                var weightTable = new[] { 4, 10, 10, 12, 10, 7, 11, 9, 11, 22 };
                var sum = 0;
                var weightSum = 0;

                foreach (int weight in weightTable)
                {
                    if (reader.Read())
                    {
                        sum += int.Parse(reader["Time"].ToString()) * weight;
                        weightSum += weight;
                    }
                }

                return (sum / weightSum).ToString();
            }
        }

        public static void AddTime(string time)
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("insert into OatsCeres (Time, InsertDate) values (@time, getdate())", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@time", time);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
