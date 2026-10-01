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
using HousingApp.Customer;
using System.Text.RegularExpressions;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for CustomerNewCustomer.xaml
    /// </summary>
    public partial class CustomerNewCustomer : Window
    {
        public CustomerNewCustomer()
        {
            InitializeComponent();
        }
        //metodi jolla lisätään uusi asiakas tietokantaan
        private void AddNewCustomerInfo()
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();
                // SQL kysely jolla voidaan lisätä asiakas tietokantaan
                string query = @"INSERT INTO Asiakas (Etunimi, Sukunimi, Sahkoposti, Osoite, Puhelin)
                         VALUES (@etunimi, @sukunimi, @sahkoposti, @osoite, @puhelin)";
                //käytetään textboxin tietoja asiakkaan tietojen lisäykseen
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@etunimi", AddEtunimiBox.Text);
                    cmd.Parameters.AddWithValue("@sukunimi", AddSukunimiBox.Text);
                    cmd.Parameters.AddWithValue("@sahkoposti", AddSahkopostiBox.Text);
                    cmd.Parameters.AddWithValue("@osoite", AddOsoiteBox.Text);
                    cmd.Parameters.AddWithValue("@puhelin", AddPuhelinBox.Text);

                    cmd.ExecuteNonQuery();
                }
            }

        }
        //button toiminto joka lisää asiakkaan tiedot tietokantaan
        private void AddCustomerDetailsButton(object sender, RoutedEventArgs e)
        {
            //tarkistetaan että onko kaikki kentät täytetty
            if (AreAllTextBoxesFilled() == false)
            {
                MessageBox.Show("Täytä kaikki kentät");
            }
            else
            {
                AddNewCustomerInfo();
                MessageBox.Show("Asiakas on lisätty tietokantaan");

                //tyhjennetään teksti textiboxeista kun tiedot on tallennettu
                AddEtunimiBox.Clear();
                AddSukunimiBox.Clear();
                AddSahkopostiBox.Clear();
                AddOsoiteBox.Clear();
                AddPuhelinBox.Clear();
            }
        }

        private void ExitWindowButton(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        //metodi joka tarkistaa että kaikki textboxit on täytetty
        private bool AreAllTextBoxesFilled()
        {
            if (string.IsNullOrWhiteSpace(AddEtunimiBox.Text) || string.IsNullOrWhiteSpace(AddSukunimiBox.Text) || string.IsNullOrWhiteSpace(AddSahkopostiBox.Text) ||
            string.IsNullOrWhiteSpace(AddOsoiteBox.Text) || string.IsNullOrWhiteSpace(AddPuhelinBox.Text))
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
        private void TiedotClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }
    }
}
