using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;

namespace HousingApp.Invoices
{
    public class InvoiceSQL
    {
        public const string connectionDB = "Server=127.0.0.1; Port=3306; User ID=opiskelija; Pwd=opiskelija1; Database=MorkkisDB";

        public ObservableCollection<Invoice> GetInvoices() //hakee laskut
        {
            using (MySqlConnection conn = new MySqlConnection(connectionDB))
            {
                conn.Open();

                //haetaan toimipisteen nimi, jonka ID on käyttäjän syöttämä tunnus
                MySqlCommand ToimipisteenHaku = new MySqlCommand("SELECT Nimi FROM Toimipiste WHERE ToimipisteID=@toimipiste;", conn);
                ToimipisteenHaku.Parameters.AddWithValue("@toimipiste", MorkkisWindow.OfficeID);
                var datareader = ToimipisteenHaku.ExecuteReader();

                datareader.Read();
                string ToimipisteN = datareader.GetString("Nimi"); //tallennetaan nimi muuttujaan
                datareader.Close();

                //luodaan SQL kysely, jossa haetaan kaikki tiedot laskusta, jonka toimipisteen nimi on sama kuin käyttäjän syöttämä toimipiste
                MySqlCommand LaskujenHaku = new MySqlCommand("SELECT * FROM Lasku WHERE Toimipiste=@toimipiste;", conn);
                LaskujenHaku.Parameters.AddWithValue("@toimipiste",ToimipisteN);
                var dr = LaskujenHaku.ExecuteReader();

                //tallennetaan tähän laskut joissa ei ole vielä laskuriveja mutta muut tiedot löytyy
                ObservableCollection<Invoice> AlmostInvoice = new ObservableCollection<Invoice>(); 

                //kaydaan lapi laskuja niin pitkaan kun niita on jaljella
                while (dr.Read())
                {
                    //luodaan ilmentyma laskusta
                    Invoice lasku = new Invoice();

                    //tallennetaan tietokannassa olevat laskun tiedot koodin laskun tietoihin
                    lasku.LaskuID = dr.GetInt32("LaskuID");
                    lasku.AsiakasID = dr.GetInt32("AsiakasID");
                    lasku.Paivamaara = DateOnly.FromDateTime(dr.GetDateTime("Paivamaara"));
                    lasku.Erapaiva = DateOnly.FromDateTime(dr.GetDateTime("Erapaiva"));
                    lasku.AOsoite = dr.GetString("Osoite");
                    lasku.APuhelin = dr.GetString("Puhelin");
                    lasku.ASahkoposti = dr.GetString("Sahkoposti");
                    lasku.AEtunimi = dr.GetString("Etunimi");
                    lasku.ASukunimi = dr.GetString("Sukunimi");
                    lasku.Toimipistenimi = dr.GetString("Toimipiste");
                    lasku.MokkiNimi = dr.GetString("Mokki");
                    lasku.Mitatoitu = dr.GetBoolean("Mitatoitu");

                    //lisataan listaan luotu lasku, johon tiedot on tallennettu
                    
                    AlmostInvoice.Add(lasku);
                }

                foreach (Invoice l in AlmostInvoice) //käy läpi laskut
                {                  
                    //etsii laskurivit laskun ID:n perusteella
                    l.Laskurivit = FindRows(l.LaskuID);
                }
                return AlmostInvoice;                
            }                       
        }

        public ObservableCollection<InvoiceRow> FindRows(int ID) //hakee laskurivit
        {
            //luodaan lista laskuriveista
            ObservableCollection<InvoiceRow> Rows = new ObservableCollection<InvoiceRow>();

            using (MySqlConnection conn = new MySqlConnection(connectionDB))
            {
                conn.Open();

                //haetaan syötettyä laskun ID:tä vastaavat rivit ja niiden tiedot
                MySqlCommand RowInfos = new MySqlCommand("SELECT Tuotenimi, Hinta, Maara FROM Laskurivi WHERE Laskurivi.LaskuID = @id;", conn);
                RowInfos.Parameters.AddWithValue("@id", ID);
                var datareader = RowInfos.ExecuteReader();

                //tallennetaan rivin tiedot listaan
                while (datareader.Read())
                {
                    InvoiceRow row = new InvoiceRow();
                    row.Tuotenimi = datareader.GetString("Tuotenimi");
                    row.Hinta = datareader.GetFloat("Hinta");
                    row.Maara = datareader.GetInt32("Maara");

                    //lisataan valmis rivi aikaisemmin luotuun listaan
                    Rows.Add(row);
                }
                //riveja kaydaan lapi yksi kerrallaan
            }

            return Rows;
        }

        public ObservableCollection<Invoice> FindCustomersInvoices(string sposti) //haetaan kaikki laskut joissa on tietyn asiakkaan sposti
        {
            ObservableCollection<Invoice> CustomersInvoices = new ObservableCollection<Invoice>();

            using (MySqlConnection conn = new MySqlConnection(connectionDB))
            {
                conn.Open();

                //Haetaan laskut, joista löytyy asiakkaan sähköposti
                MySqlCommand FindInvoices = new MySqlCommand("SELECT * FROM Lasku WHERE Lasku.Sahkoposti = @sahkoposti;", conn);
                FindInvoices.Parameters.AddWithValue("@sahkoposti", sposti);
                var dr = FindInvoices.ExecuteReader();


                while (dr.Read())
                {
                    Invoice lasku = new Invoice();

                    //tallennetaan tietokannassa olevat laskun tiedot koodin laskun tietoihin
                    lasku.LaskuID = dr.GetInt32("LaskuID");
                    lasku.AsiakasID = dr.GetInt32("AsiakasID");
                    lasku.Paivamaara = DateOnly.FromDateTime(dr.GetDateTime("Paivamaara"));
                    lasku.Erapaiva = DateOnly.FromDateTime(dr.GetDateTime("Erapaiva"));
                    lasku.AOsoite = dr.GetString("Osoite");
                    lasku.APuhelin = dr.GetString("Puhelin");
                    lasku.ASahkoposti = dr.GetString("Sahkoposti");
                    lasku.AEtunimi = dr.GetString("Etunimi");
                    lasku.ASukunimi = dr.GetString("Sukunimi");
                    lasku.Toimipistenimi = dr.GetString("Toimipiste");
                    lasku.MokkiNimi = dr.GetString("Mokki");
                    lasku.Mitatoitu = dr.GetBoolean("Mitatoitu");

                    //lisataan valmis lasku aikaisemmin luotuun listaan
                    CustomersInvoices.Add(lasku);
                }

                foreach (Invoice l in CustomersInvoices) //käy läpi laskut
                {
                    //etsii laskurivit laskun ID:n perusteella
                    l.Laskurivit = FindRows(l.LaskuID);
                }
            }
            return CustomersInvoices;
        }

        public ObservableCollection<Invoice> FindTimeInvoices(DateTime alkamispaiva, DateTime lopetuspaiva) //haetaan kaikki laskut joissa on tietty päivämäärä
        {
            ObservableCollection<Invoice> TimeInvoices = new ObservableCollection<Invoice>();

            using (MySqlConnection conn = new MySqlConnection(connectionDB))
            {
                Invoice invoice = new Invoice();
                conn.Open();

                //haetaan toimipisteen nimi, jonka ID on käyttäjän syöttämä tunnus
                MySqlCommand ToimipisteenHaku = new MySqlCommand("SELECT Nimi FROM Toimipiste WHERE ToimipisteID=@toimipiste;", conn);
                ToimipisteenHaku.Parameters.AddWithValue("@toimipiste", MorkkisWindow.OfficeID);
                var datareader = ToimipisteenHaku.ExecuteReader();

                datareader.Read();
                string ToimipisteN = datareader.GetString("Nimi"); //tallennetaan nimi muuttujaan
                datareader.Close();


                //Haetaan laskut, joiden päivämäärä on haun alku- ja loppupäivämäärien välillä
                MySqlCommand FindInvoices = new MySqlCommand("SELECT * FROM Lasku WHERE Toimipiste=@toimipiste AND (Paivamaara BETWEEN @startdate AND @enddate);", conn);
                FindInvoices.Parameters.AddWithValue("@startdate", alkamispaiva);
                FindInvoices.Parameters.AddWithValue("@enddate", lopetuspaiva);
                FindInvoices.Parameters.AddWithValue("@toimipiste", ToimipisteN);

                var dr = FindInvoices.ExecuteReader();

                //tallennetaan laskun tiedot
                while (dr.Read())
                {
                    Invoice lasku = new Invoice();

                    //tallennetaan tietokannassa olevat laskun tiedot koodin laskun tietoihin
                    lasku.LaskuID = dr.GetInt32("LaskuID");
                    lasku.AsiakasID = dr.GetInt32("AsiakasID");
                    lasku.Paivamaara = DateOnly.FromDateTime(dr.GetDateTime("Paivamaara"));
                    lasku.Erapaiva = DateOnly.FromDateTime(dr.GetDateTime("Erapaiva"));
                    lasku.AOsoite = dr.GetString("Osoite");
                    lasku.APuhelin = dr.GetString("Puhelin");
                    lasku.ASahkoposti = dr.GetString("Sahkoposti");
                    lasku.AEtunimi = dr.GetString("Etunimi");
                    lasku.ASukunimi = dr.GetString("Sukunimi");
                    lasku.Toimipistenimi = dr.GetString("Toimipiste");
                    lasku.MokkiNimi = dr.GetString("Mokki");
                    lasku.Mitatoitu = dr.GetBoolean("Mitatoitu");

                    //lisataan valmis rivi aikaisemmin luotuun listaan
                    TimeInvoices.Add(lasku);
                }
                foreach (Invoice l in TimeInvoices) //käy läpi laskut
                {
                    //etsii laskurivit laskun ID:n perusteella
                    l.Laskurivit = FindRows(l.LaskuID);
                }
            }
            return TimeInvoices;
        }

        public void TallennaMitatointi(int ID, bool mitatointi) //laskun mitätöinti
        {
            using (MySqlConnection conn = new MySqlConnection(connectionDB))
            {
                conn.Open();

                //muutetaan mitätöinnin boolean, jotta lasku on merkattu mitätöidyksi
                MySqlCommand MitatoituTallennus = new MySqlCommand("UPDATE Lasku SET Mitatoitu = @Mitatointi WHERE LaskuID=@id;", conn);
                MitatoituTallennus.Parameters.AddWithValue("@Mitatointi", mitatointi);
                MitatoituTallennus.Parameters.AddWithValue("@id", ID);
                var datareader = MitatoituTallennus.ExecuteNonQuery();
            }               
        }
    }
}
