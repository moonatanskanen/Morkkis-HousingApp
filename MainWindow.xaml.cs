using MySql.Data.MySqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Repository.CreateDatabase();
            LoadOffices();

            // Testaus että database varmasti toimii.
            //var names = Repository.TestDatabase();
            //MessageBox.Show(names);

        }

        //Metodi joka hakee kaikki toimipisteet tietokannasta ja lisää ComboBoxiin
        private void LoadOffices()
        {
            string connectionString = "Server=127.0.0.1; Port=3306; User ID=opiskelija; Pwd=opiskelija1; Database=MorkkisDB";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                //Haetaan toimipisteiden ID:t ja nimet 
                string query = "SELECT ToimipisteID, nimi FROM Toimipiste";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //Uusi toimipiste-olio
                        var office = new Toimipiste
                        {
                            Id = reader.GetInt32("ToimipisteID"),
                            Nimi = reader.GetString("nimi")
                        };
                        //Lisätään toimipiste ComboBoxiin
                        OfficeComboBox.Items.Add(office);
                    }
                }
            }
        }

        //Metodi joka avaa Morkkis-ikkunan kun käyttäjä painaa jatka
        private void OpenMorkkisWindow(object sender, RoutedEventArgs e)
        {
            if (OfficeComboBox.SelectedItem is Toimipiste selectedOffice)
            {
                int selectedId = selectedOffice.Id;
                var morkkis = new MorkkisWindow(selectedId);
                morkkis.Show();
                Close();
            }
            else
            {   //Jos toimipistettä ei ole valittu, ilmoitetaan siitä messageboxis
                MessageBox.Show("Valitse ensin toimipiste jatkaaksesi.", "Huomautus", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //Tuki-napin metodi josta aukeaa MessageBox
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
            "Ongelmatilanteissa ota yhteyttä asiakastukeen:\n\n" +
            "Sähköposti: morkkistuki@gmail.com\n" +
            "Puhelin: 040 123 4567(palvelemme 24 / 7)",
            "Asiakastuki",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        }

        private void OpenChangeLocationInfo(object sender, RoutedEventArgs e)
        {
            var changeLocationInfo = new ChangeLocationInfo();
            changeLocationInfo.ShowDialog();
        }
    }

    //Toimipiste luokka
    public class Toimipiste
    {
        public int Id { get; set; }
        public string Nimi { get; set; }


        public override string ToString()
        {
            return Nimi; //Näytetään ComboBoxissa pelkkä nimi
        }
    }
}