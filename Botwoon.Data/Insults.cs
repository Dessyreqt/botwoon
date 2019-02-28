using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace Botwoon.Data
{
    public class Insults
    {
        public static string Get()
        {
            return string.Format("{0} {1} {2}", GetRandomInsult(1), GetRandomInsult(2), GetRandomInsult(3));
        }

        private static string GetRandomInsult(int pos)
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select * from Insults where Position = @position order by newid()", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@position", pos);

                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return reader["Word"].ToString();
                }
            }

            return null;
        }
    }
}
