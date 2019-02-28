using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;

namespace Botwoon.Data
{
    public class DropTable
    {
        public string Name { get; set; }
        public float Nothing { get; set; }
        public float Energy { get; set; }
        public float BigEnergy { get; set; }
        public float Missile { get; set; }
        public float SuperMissile { get; set; }
        public float PowerBombs { get; set; }

    }

    public class Drops
    {
        public static List<DropTable> GetDrops(string enemy)
        {
            var retVal = new List<DropTable>();
            using (var conn = new SqlCeConnection(ConfigurationManager.ConnectionStrings["Botwoon"].ConnectionString))
            using (var cmd = new SqlCeCommand("select * from smDrops where Name like @name", conn))
            {
                conn.Open();

                cmd.Parameters.AddWithValue("@name", string.Format("%{0}%", enemy));
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var drop = new DropTable();

                    drop.Name = reader["Name"].ToString();
                    drop.Nothing = float.Parse(reader["Nothing"].ToString());
                    drop.Energy = float.Parse(reader["Energy"].ToString());
                    drop.BigEnergy = float.Parse(reader["BigEnergy"].ToString());
                    drop.Missile = float.Parse(reader["Missile"].ToString());
                    drop.SuperMissile = float.Parse(reader["SuperMissile"].ToString());
                    drop.PowerBombs = float.Parse(reader["PowerBomb"].ToString());

                    retVal.Add(drop);
                }

                return retVal;
            }
        }
    }
}
