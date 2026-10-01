using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HousingApp.Customer;
using MySql.Data.MySqlClient;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for CustomerWindow.xaml
    /// </summary>
    /// 



    public partial class CustomerWindow : Window
    {
        //tallennetaan valitun asiakkaa ID tähän muuttujaan
        private int currentAsiakasID;
        public CustomerWindow()
        {
            InitializeComponent();
        }


        //Avaa ikkunan joss valitaan asiakas.
        private void OpenCustomerChooseCustomerWindow(object sender, RoutedEventArgs e)
        {
            var customerWindow = new CustomerChooseCustomer();
            //valittu asiakas on result. result valitaan ikkunassa jonka tämä avaa
            bool? result = customerWindow.ShowDialog();

            // Jos asiakas löytyy haetaan suoraan sen tiedot LoadCustomerDetails metodilla (ja annetaan sille etu- ja sukunimi)
            if (result == true && customerWindow.SelectedCustomer != null)
            {

                //LoadCustomerDetails(customerWindow.SelectedCustomer.Etunimi, customerWindow.SelectedCustomer.Sukunimi);
                LoadCustomerDetails(customerWindow.SelectedCustomer.AsiakasID);
            }
        }

        //Haetaan tiedot etu- ja sukunimen mukaan (selecterCustomer mukaan)
        //private void LoadCustomerDetails(string etunimi, string sukunimi)
        private void LoadCustomerDetails(int asiakasID)
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();
                //SQL kysely hakee asiakkaan tiedot asiakas ID:n perusteella
                string query = "SELECT AsiakasID, Etunimi, Sukunimi, Sahkoposti, Osoite, Puhelin FROM Asiakas WHERE AsiakasID = @asiakasID";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    //asiakas ID parametri jonka pohjalta tietoja lähdetään hakemaan
                    cmd.Parameters.AddWithValue("@asiakasID", asiakasID);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        //luetaan tietokantaa
                        if (reader.Read())
                        {
                            //viedään tiedot käyttöliittymän ikkunaan
                            //asiakasID pitää muutta int tyyppiseksi
                            currentAsiakasID = Convert.ToInt32(reader["AsiakasID"]);
                            EtunimiBox.Text = reader["Etunimi"].ToString();
                            SukunimiBox.Text = reader["Sukunimi"].ToString();
                            SahkopostiBox.Text = reader["Sahkoposti"].ToString();
                            OsoiteBox.Text = reader["Osoite"].ToString();
                            PuhelinBox.Text = reader["Puhelin"].ToString();
                        }
                        else
                        {
                            //virhe ilmoitus
                            MessageBox.Show("Asiakasta ei löytynyt.");
                        }
                    }
                }
            }
        }
        //tallennetaan tiedot tietokantaan metodi
        private void SaveCustomerDetails()
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                //SQL kysely hakee asiakkaan tiedot asiakasID perusteella
                string query = @"UPDATE Asiakas SET Etunimi = @etunimi, Sukunimi = @sukunimi, Sahkoposti = @sahkoposti, Osoite = @osoite, Puhelin = @puhelin  WHERE AsiakasID = @asiakasID";
                //
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    //Käyttöliittymässä olevat muutokset lisätään SQL tietokantaan
                    cmd.Parameters.AddWithValue("@etunimi", EtunimiBox.Text);
                    cmd.Parameters.AddWithValue("@sukunimi", SukunimiBox.Text);
                    cmd.Parameters.AddWithValue("@sahkoposti", SahkopostiBox.Text);
                    cmd.Parameters.AddWithValue("@osoite", OsoiteBox.Text);
                    cmd.Parameters.AddWithValue("@puhelin", PuhelinBox.Text);
                    cmd.Parameters.AddWithValue("@asiakasID", currentAsiakasID);
                    //suorita komento
                    cmd.ExecuteNonQuery();

                }
            }
        }

        //tallennus button
        private void SaveButton(object sender, RoutedEventArgs e)
        {
            //tarkistaa että onko asiakas valittu
            if (currentAsiakasID == 0)
            {
                MessageBox.Show("Valitse asiakas ennen kuin haluat tallentaa tiedot.");
                return;
            }
            //tarkistaa että kaikki textboxit on täytetty
            if (!AreAllTextBoxesFilled())
            {
                MessageBox.Show("Täytä kaikki tiedot");
                return;
            }


            //tallennetaan tiedot
            SaveCustomerDetails();

            MessageBox.Show("Asiakkaan tiedot tallennettu.");

            //tyhjennetään teksti textiboxeista kun tiedot on tallennettu
            EtunimiBox.Clear();
            SukunimiBox.Clear();
            SahkopostiBox.Clear();
            OsoiteBox.Clear();
            PuhelinBox.Clear();

            //current asiaks ID laitetan 0, jolloin joudutaan valitsemaan uusi asiakas
            currentAsiakasID = 0;

        }

        //exit menuitem
        public void PoistuClick(object sender, RoutedEventArgs e)
        {
            var vastaus = MessageBox.Show("Olet sulkemassa ikkunaa.\n Haluatko jatkaa?", "HUOMAUTUS!", MessageBoxButton.YesNo);

            if (vastaus == MessageBoxResult.Yes)
            {
                Close();
            }
            else
            {
                return;
            }
        }
        private void TiedotClick(object sender, RoutedEventArgs e) //tiedot menuitem
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }

        private void OpenNewCustomerWindow(object sender, RoutedEventArgs e)
        {
            //true false lisäys
            var newCustomerWindow = new CustomerNewCustomer();
            newCustomerWindow.ShowDialog();
        }
        //tarkistus joka tarkistaa että kaikki textboxit on täytetty
        private bool AreAllTextBoxesFilled()
        {
            if (string.IsNullOrWhiteSpace(EtunimiBox.Text) || string.IsNullOrWhiteSpace(SukunimiBox.Text) || string.IsNullOrWhiteSpace(SahkopostiBox.Text) ||
            string.IsNullOrWhiteSpace(OsoiteBox.Text) || string.IsNullOrWhiteSpace(PuhelinBox.Text))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void EtunimiBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //tarkistaa että vain sallitut merkit voidaan syöttää textboxeihin. Muuteen teksin syöttö lopetetaan
            e.Handled = !Regex.IsMatch(e.Text, "^[a-zA-ZåäöÅÄÖ-]$");
        }

        private void SukunimiBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[a-zA-ZåäöÅÄÖ-]$");
        }

        private void PuhelinBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }
    }
}
