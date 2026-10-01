using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingApp.Housing
{
    /// <summary>
    /// Luokka mökkitietojen ylläpitämiseen
    /// </summary>
    public class Cabin
    {
        // MokkiID primary key
        public int CabinID { get; set; }
        // ToimipisteID foreign key
        public int OfficeID { get; set; }
        // Nimi
        public string Name { get; set; }
        // Vuorokausihinta
        public float Price { get; set; }

        /// <summary>
        /// Etsii kaikki mökit valitusta toimipisteestä
        /// </summary>
        static public ObservableCollection<Cabin> FindAllOfficeCabins(int officeID)
        {
            ObservableCollection<Cabin> cabins = new ObservableCollection<Cabin>();

            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findCabins = new MySqlCommand("SELECT * FROM Mokki WHERE ToimipisteID=@id ORDER BY MokkiID ASC", conn);
                findCabins.Parameters.AddWithValue("@id", officeID);

                var reader = findCabins.ExecuteReader();
                while (reader.Read())
                {
                    var cab = new Cabin(reader);
                    cabins.Add(cab);
                }

                return cabins;
            }
        }

        /// <summary>
        /// Etsii mökin mökkiIDllä
        /// </summary>
        static public Cabin FindCabin(int ID)
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findCabin = new MySqlCommand("SELECT * FROM Mokki WHERE MokkiID=@id", conn);
                findCabin.Parameters.AddWithValue("@id", ID);

                var reader = findCabin.ExecuteReader();
                reader.Read();
                var cab = new Cabin(reader);

                return cab;
            }
        }

        /// <summary>
        /// Tallentaa mökin vuorokausihinnan muutoksen tietokantaan
        /// </summary>
        public void SaveCabin()
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findCabin = new MySqlCommand("UPDATE Mokki SET Vuorokausihinta=@price WHERE MokkiID=@id", conn);
                findCabin.Parameters.AddWithValue("@price", Price);
                findCabin.Parameters.AddWithValue("@id", CabinID);

                findCabin.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Lukijalla varustettu konstruktori
        /// </summary>
        public Cabin(MySqlDataReader reader)
        {
            CabinID = reader.GetInt32("MokkiID");
            OfficeID = reader.GetInt32("ToimipisteID");
            Name = reader.GetString("Nimi");
            Price = reader.GetFloat("Vuorokausihinta");
        }

        public Cabin()
        {
            CabinID = 0;
            OfficeID = 0;
            Name = "";
            Price = 0;
        }
    }
}
