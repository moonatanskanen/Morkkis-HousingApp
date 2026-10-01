using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HousingApp.Services;
using System.Collections.ObjectModel;
using System.Media;
using System.Text.RegularExpressions;



namespace HousingApp
{
    public partial class ServiceWindow : Window
    {
        public ObservableCollection<ServiceClass> Services { get; set; } = new();

        public ServiceWindow()
        {
            InitializeComponent();

            // Haetaan kaikki palvelut tietokannasta ja lisätään ObservableCollectioniin
            foreach (var service in GetServices())
            {
                Services.Add(service);
            }

            //asetetaan palvelut ItemsControliin
            ServiceList.ItemsSource = Services;
        }

        //Metodi, joka sallii vain numeroiden ja pisteiden syötyt hinta kenttään
        private void OnlyNumbers(object sender, TextCompositionEventArgs e)
        {
            // Sallitaan 0-9 ja piste
            if (!Regex.IsMatch(e.Text, @"^[0-9.]$"))
            {
                SystemSounds.Beep.Play(); //piippaa
                e.Handled = true;         // estä merkki
            }

            var tb = (System.Windows.Controls.TextBox)sender;
            string future = tb.Text.Insert(tb.SelectionStart, e.Text);


            if (!Regex.IsMatch(future, @"^\d{0,8}(\.\d{0,2})?$"))
            {
                SystemSounds.Beep.Play(); //piippaa
                e.Handled = true;         //estä lisäys
            }
        }

        //Metodi, joka sallii palvelun nimikenttään vain kirjaimet, numerot, yhdysmerkin ja heittomerkin
        private void OnlyLetters(object sender, TextCompositionEventArgs e)
        {
            // \p{L} = mikä tahansa kirjain         
            if (!Regex.IsMatch(e.Text, @"^[\p{L}0-9' -]$"))
            {
                SystemSounds.Beep.Play();
                e.Handled = true;
            }
        }

        // Metodi jolla haetaan palvelut tietokannasta
        private List<ServiceClass> GetServices()
        {
            var services = new List<ServiceClass>(); //Lista johon kerätään palvelut
            int toimipisteID = MorkkisWindow.OfficeID; //Toimipiste id johon palvelut kuuluvat

            // Yhdistetään tietokantaan
            using var connection = new MySqlConnection(Repository.connectionDB);
            connection.Open();

            //SQL-kysely, jolla haetaan palvelut tietyltä toimipisteeltä (id, nimi ja hinta)
            string query = "SELECT PalveluID, Nimi, Hinta FROM Palvelut WHERE ToimipisteID = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", toimipisteID);

            using var reader = command.ExecuteReader();

            //Luetaan kaikki rivit tietokannasta ja lisätään listaan
            while (reader.Read())
            {
                services.Add(new ServiceClass
                {
                    ServiceID = reader.GetInt32("PalveluID"),
                    ServiceName = reader.GetString("Nimi"),
                    ServicePrice = reader.GetDecimal("Hinta")
                });
            }

            return services;// palautetaan lista käyttöliittymälle
        }

        //Metodi, joka tallentaa muutetut hinnat tietokantaan ja sulkee ikkunan
        private void Save_Click(object sender, RoutedEventArgs e)
        {

            //jokaisella rivillä on oltava sekä nimi että hinta
            foreach (var s in Services)
            {
                string name = s.ServiceName?.Trim() ?? "";
                bool nameOk = !string.IsNullOrWhiteSpace(name);
                bool priceOk = s.ServicePrice > 0;

                if (!nameOk || !priceOk)    // puuttuu jotakin = virhe
                {
                    MessageBox.Show(
                        "Palvelussa puuttuvia tietoja.\n" +
                        "Täytä nimi ja hinta tai poista tyhjä palvelu.",
                        "Puuttuvat tiedot",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;                 // keskeytä koko tallennus
                }
            }

            //Yhteys tietokantaan
            using var connection = new MySqlConnection(Repository.connectionDB);
            connection.Open();

            int toimipisteID = MorkkisWindow.OfficeID; //Toimipisteen id

            //Käydään kaikki palvelut läpi
            foreach (var service in Services)
            {
                if (service.ServiceID == 0)
                {
                    // Jos id on 0 niin kyseessä on uusi palvelu, lisätään se tietokantaan
                    string insertQuery = "INSERT INTO Palvelut (Nimi, Hinta, ToimipisteID) VALUES (@nimi, @hinta, @toimipisteId)";
                    using var insertCmd = new MySqlCommand(insertQuery, connection);
                    insertCmd.Parameters.AddWithValue("@nimi", service.ServiceName);
                    insertCmd.Parameters.AddWithValue("@hinta", service.ServicePrice);
                    insertCmd.Parameters.AddWithValue("@toimipisteId", toimipisteID);
                    insertCmd.ExecuteNonQuery();
                }
                else
                {
                    // Jos id ei ole 0 niin palvelu on jo olemassa, pävitetään palvelun hinta
                    string updateQuery = "UPDATE Palvelut SET Hinta = @hinta WHERE PalveluID = @id";
                    using var updateCmd = new MySqlCommand(updateQuery, connection);
                    updateCmd.Parameters.AddWithValue("@hinta", service.ServicePrice);
                    updateCmd.Parameters.AddWithValue("@id", service.ServiceID);
                    updateCmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Tiedot tallennettu");
            Close();
        }

        //Metodi, joka lisää uuden tyhjän kentän uuden palvelun lisäystä varten
        private void AddNewService_Click(object sender, RoutedEventArgs e)
        {
            Services.Add(new ServiceClass
            {
                ServiceID = 0,
                ServiceName = "",
                ServicePrice = 0,
            });
        }

        // Poista palvelu metodi
        private void DeleteService_Click(object sender, RoutedEventArgs e)
        {
            //Selvitetään mitä nappia painettiin ja minkä palvelun se liittyy
            if (sender is Button button && button.DataContext is ServiceClass service)
            {
                //Varmistetaan messageboxissa käyttäjältä halutaanko palvelu varmasti poistaa
                var result = MessageBox.Show(
                    $"Poistetaanko palvelu \"{service.ServiceName}\"?",
                    "Vahvista poisto",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;

                // Jos palvelu ei ole vielä tallennettu tietokantaan, poistetaan vain käyttöliittymästä
                if (service.ServiceID == 0)
                {
                    Services.Remove(service);
                    return;
                }

                //Haetaan kaikki varaukset, joissa palvelu on mukana
                var reservationIds = GetAllReservationIDs(service.ServiceID);
                //Haetaan niistä ne, jotka ovat tulevaisuudessa.
                var futureReservationIds = GetFutureReservationIDs(service.ServiceID);

                using var connection = new MySqlConnection(Repository.connectionDB);
                connection.Open();

                //Jos palvelua käytetään jossain varauksessa
                if (reservationIds.Any())
                {
                    foreach (var varausId in reservationIds)
                    {
                        //Poistetaan palvelu varaus-palvelu-taulusta aina
                        using (var delCmd = new MySqlCommand(
                            "DELETE FROM Varauksen_palvelut WHERE VarausID = @varausId AND PalveluID = @palveluId",
                            connection))
                        {
                            delCmd.Parameters.AddWithValue("@varausId", varausId);
                            delCmd.Parameters.AddWithValue("@palveluId", service.ServiceID);
                            delCmd.ExecuteNonQuery();
                        }

                        //Jos varaus on tuleva, mitätöidään vanha lasku ja luodaan uusi ilman poistettua palvelua
                        if (futureReservationIds.Contains(varausId))
                        {
                            //Mitätöidään vanha lasku
                            using (var mitaCmd = new MySqlCommand(
                                "UPDATE Lasku SET Mitatoitu = 1 WHERE VarausID = @varausId AND Mitatoitu = 0",
                                connection))
                            {
                                mitaCmd.Parameters.AddWithValue("@varausId", varausId);
                                mitaCmd.ExecuteNonQuery();
                            }

                            //Haetaan mitätöidyn laskun eräpäivä
                            DateTime erapaiva;
                            using (var dueCmd = new MySqlCommand(
                                "SELECT Erapaiva FROM Lasku WHERE VarausID = @varausId AND Mitatoitu = 1 ORDER BY LaskuID DESC LIMIT 1",
                                connection))
                            {
                                dueCmd.Parameters.AddWithValue("@varausId", varausId);
                                
                                erapaiva = Convert.ToDateTime(dueCmd.ExecuteScalar());
                            }

                            //luodaan uusi lasku samoilla tiedoilla mutta ilman poistettua palvelua
                            using (var newInv = new MySqlCommand(@"
                                INSERT INTO Lasku (
                                Paivamaara, Erapaiva, Osoite, Puhelin, Sahkoposti,
                                Etunimi, Sukunimi, Toimipiste, Mokki,
                                Mitatoitu, AsiakasID, VarausID
                                )
                                SELECT
                                CURDATE(), @erapaiva, Osoite, Puhelin, Sahkoposti,
                                Etunimi, Sukunimi, Toimipiste, Mokki,
                                0, AsiakasID, VarausID
                                FROM Lasku
                                WHERE VarausID = @varausId AND Mitatoitu = 1
                                ORDER BY LaskuID DESC LIMIT 1;",
                                connection))
                            {
                                newInv.Parameters.AddWithValue("@varausId", varausId);
                                newInv.Parameters.AddWithValue("@erapaiva", erapaiva);
                                newInv.ExecuteNonQuery();
                            }

                            //Kopioidaan jäljelle jääneet laskurivit uudelle laskulle
                            using (var copyCmd = new MySqlCommand(@"
                                INSERT INTO Laskurivi (Tuotenimi, Hinta, Maara, LaskuID)
                                SELECT Tuotenimi, Hinta, Maara, LAST_INSERT_ID()
                                FROM Laskurivi
                                WHERE LaskuID = (
                                SELECT LaskuID
                                FROM Lasku
                                WHERE VarausID = @varausId AND Mitatoitu = 1
                                ORDER BY LaskuID DESC LIMIT 1
                                )
                                AND Tuotenimi <> @poistettuNimi;",
                                connection))
                            {
                                copyCmd.Parameters.AddWithValue("@varausId", varausId);
                                copyCmd.Parameters.AddWithValue("@poistettuNimi", service.ServiceName);
                                copyCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    MessageBox.Show(
                        "Palvelu poistettiin tulevista varauksista.\n" +
                        "Asiakkaille lähetetään ilmoitus automaattisesti.",
                        "Ilmoitus",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                //Lopuksi poistetaan palvelu itse taulusta
                using (var deleteCmd = new MySqlCommand(
                    "DELETE FROM Palvelut WHERE PalveluID = @id",
                    connection))
                {
                    deleteCmd.Parameters.AddWithValue("@id", service.ServiceID);
                    deleteCmd.ExecuteNonQuery();
                }

                Services.Remove(service);
            }
        }

        //Metodi, joka hakee kaikki varaukset, joissa palvelua käytetään
        private List<int> GetAllReservationIDs(int serviceId)
        {
            var reservationIds = new List<int>(); //Lista, johon kerätään varaus-IDt

            using var connection = new MySqlConnection(Repository.connectionDB);
            connection.Open();

            //Haetaan varaukset
            const string sql = @"
                SELECT DISTINCT v.VarausID
                FROM Varauksen_palvelut vp
                JOIN Varaus v       ON vp.VarausID = v.VarausID
                WHERE vp.PalveluID = @id;";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", serviceId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                reservationIds.Add(reader.GetInt32("VarausID"));
            }

            return reservationIds;
        }

        //Metodi joka palauttaa tulevien varausten ID:t, joissa palvelu käytössä
        private List<int> GetFutureReservationIDs(int serviceId)
        {
            var reservationIds = new List<int>();//Listaan kerätään varaus ID:t

            using var connection = new MySqlConnection(Repository.connectionDB);
            connection.Open();

            const string sql = @"
                SELECT DISTINCT v.VarausID
                FROM Varauksen_palvelut vp
                JOIN Varaus v
                ON vp.VarausID = v.VarausID
                WHERE vp.PalveluID = @id
                AND v.Alkupaivamaara > CURDATE();";

            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", serviceId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                reservationIds.Add(reader.GetInt32("VarausID"));
            }

            return reservationIds;//Palautetaan lista
        }
        private void TiedotClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }
    }
}
