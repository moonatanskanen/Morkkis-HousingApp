using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static HousingApp.ChangeLocationInfo;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for ChangeLocationInfo.xaml
    /// </summary>
    public partial class ChangeLocationInfo : Window
    {

      
        public ChangeLocationInfo()
        {
            InitializeComponent();
           
            LoadLocations();
        }
        //Lataa Toimipisteen tiedot tietokannasta comboboxiin
        private void LoadLocations()
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();
                string query = "SELECT ToimipisteID, Nimi FROM Toimipiste";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var location = new Toimipiste
                        {
                            Id = reader.GetInt32("ToimipisteID"),
                            Nimi = reader.GetString("Nimi")
                        };
                        LocationComboBox.Items.Add(location);
                    }
                }
            }
        }
        

        //lataa valitun toimipisteen tiedot textboxeihin
        private void LoadLocationData(int toimipisteID)
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();
                string query = "SELECT Sahkoposti, Osoite, Puhelin FROM Toimipiste WHERE ToimipisteID = @id";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", toimipisteID);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            AddEmailBox.Text = reader["Sahkoposti"].ToString();
                            AddOsoiteBox.Text = reader["Osoite"].ToString();
                            AddPuhelinBox.Text = reader["Puhelin"].ToString();
                        }
                    }
                }
            }
        }
    
        //Tallentaa valitun toimipisteen muutokset tietokantaan
        private void SaveLocationData()
        {
            if (LocationComboBox.SelectedItem is Toimipiste selectedLocation)
            {
                using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
                {
                    conn.Open();
                    string query = "UPDATE Toimipiste SET Sahkoposti = @sahkoposti, Osoite = @osoite, Puhelin = @puhelin WHERE ToimipisteID = @id";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@sahkoposti", AddEmailBox.Text);
                        cmd.Parameters.AddWithValue("@osoite", AddOsoiteBox.Text);
                        cmd.Parameters.AddWithValue("@puhelin", AddPuhelinBox.Text);
                        cmd.Parameters.AddWithValue("@id", selectedLocation.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Toimipisteen tiedot päivitetty.");
            }
            else
            {
                MessageBox.Show("Valitse toimipiste ennen tallentamista.");
            }
        }
    
        //filter kenttä
        private void LocationComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LocationComboBox.SelectedItem is Toimipiste selectedLocation)
            {
                LoadLocationData(selectedLocation.Id);
            }
        }
        //Tallennus painike
        private void SaveLocationDataButtonClick(object sender, RoutedEventArgs e)
        {
            if(!AreAllTextBoxesFilled())
            {
                MessageBox.Show("Kentät eivät voi olla tyhjät");
                return;
            }
            
            SaveLocationData();

            AddEmailBox.Clear();
            AddOsoiteBox.Clear();
            AddPuhelinBox.Clear();
            LocationComboBox.SelectedIndex = -1;
        }

        public class Toimipiste
        {
            public int Id { get; set; }
            public string Nimi { get; set; }

            public override string ToString()
            {
                return Nimi; //Näytetään ComboBoxissa pelkkä nimi
            }
        }
        //exit painike
        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
        //metodi joka tarkistaa että onko kaikki kentät täytetty
        private bool AreAllTextBoxesFilled()
        {
            if (string.IsNullOrWhiteSpace(AddEmailBox.Text) || string.IsNullOrWhiteSpace(AddOsoiteBox.Text) || string.IsNullOrWhiteSpace(AddPuhelinBox.Text))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        //tiedot ikkunan message
        private void TiedotClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }
        //Puhelin kenttää ei voi syöttää muuta kuin numeroita
        private void PuhelinPreview(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }
    }


}
