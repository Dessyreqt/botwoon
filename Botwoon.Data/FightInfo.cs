using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace Botwoon.Data
{

    public class FightInfo
    {
        public static List<string> Get(string monster)
        {
            var retVal = new List<string>();
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select * from FF6Fight where Alias = @name", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@name", monster);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    retVal.Add(string.Format("{0} - {1}", reader["Name"], reader["Tactics"]));
                }

                if (retVal.Count == 0)
                {
                    retVal.Add("Enemy not found, please use the name as shown in FF6.");
                }

                return retVal;
            }
        }
    }
}
