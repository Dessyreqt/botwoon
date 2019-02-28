using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Linq;
using System.Text;

namespace Botwoon.Data
{
    public class UnknownInput
    {
        public static void Enter(string input)
        {
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("insert into UnknownInput (Input, InputDate) values (@input, @inputDate)", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@input", input);
                cmd.Parameters.AddWithValue("@inputDate", DateTime.UtcNow);

                cmd.ExecuteNonQuery();
            }
        }

    }
}
