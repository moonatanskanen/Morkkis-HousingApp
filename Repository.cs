using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingApp
{
    public static class Repository
    {
        // Connection strings for the database.
        public const string connection = "Server=127.0.0.1; Port=3306; User ID=opiskelija; Pwd=opiskelija1;";
        public const string connectionDB = "Server=127.0.0.1; Port=3306; User ID=opiskelija; Pwd=opiskelija1; Database=MorkkisDB";

        // Creates a new database.
        public static void CreateDatabase()
        {
            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                conn.Open();

                var tr = conn.BeginTransaction();
                // Reads the SQL creation file to a string.
                string createScript = File.ReadAllText("morkkis.sql");
                MySqlCommand createDatabase = new MySqlCommand(createScript, conn);

                createDatabase.ExecuteNonQuery();

                tr.Commit();
            }
        }

        public static String TestDatabase()
        {
            String names = "";

            using (MySqlConnection conn = new MySqlConnection(connectionDB))
            {
                conn.Open();

                // Reads the SQL creation file to a string.
                MySqlCommand createDatabase = new MySqlCommand("SELECT * FROM Asiakas", conn);

                var reader = createDatabase.ExecuteReader();

                while (reader.Read())
                {
                    names += reader.GetString("Etunimi") + "\n";
                }
            }

            return names;
        }
    }
}
