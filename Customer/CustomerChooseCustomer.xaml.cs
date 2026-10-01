using HousingApp.Housing;
using HousingApp.Services;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for CustomerChooseCustomer.xaml
    /// </summary>
    public partial class CustomerChooseCustomer : Window
    {
        public ObservableCollection<Customer> 
        Customers { get; set; } = new ObservableCollection<Customer>();
        
        private List<Customer> allCustomers = new List<Customer>();

        //Valittu asiakas, jonka tiedot listataan CustomerWindow ikkunaan
        public Customer SelectedCustomer { get; set; }

        public CustomerChooseCustomer()
        {
            InitializeComponent();
            DataContext = this;
            Customers = LoadCustomers();
            allCustomers = LoadCustomers().ToList();
            ApplyFilter("");
        }

        public class Customer
        {
            public int AsiakasID { get; set; }
            public string Etunimi { get; set; }
            public string Sukunimi { get; set; }
            public string Sahkoposti { get; set; }
            public string Osoite { get; set; }
            public string Puhelin { get; set; }

            //Etunimi ja sukunimi yhdessä
            public string FullName => $"{Etunimi} {Sukunimi}";
        }

        //Palauttaa kaikki asiakkaat tietokannasta observableCollectionina ja ne on LoadCustomers metodissa
        public ObservableCollection<Customer> LoadCustomers()
        {
            //collection kaikille asiakkaille
            ObservableCollection<Customer> allCustomers = new ObservableCollection<Customer>();
            //haetaan yhteys tietokantaan
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                // SQL-kysely, jolla haetaan asiakkaiden etu ja sukunimet
                MySqlCommand cmd = new MySqlCommand("SELECT AsiakasID, Etunimi, Sukunimi FROM Asiakas;", conn);
                
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    //luetaan ja lisätään tiedot kokoelmaan
                    while (reader.Read())
                    {
                        Customer customer = new Customer
                        {
                            AsiakasID = reader.GetInt32("AsiakasID"),
                            Etunimi = reader.GetString("Etunimi"),
                            Sukunimi = reader.GetString("Sukunimi")
                        };

                        allCustomers.Add(customer);
                    }
                }
            }

            //palauttaa kaikki etu ja sukunimet
            return allCustomers;
        }
        //teksti kenttä
        private void FilterField(object sender, TextChangedEventArgs e)
            
        {
            //filteroi tekstin (isoilla kirjaimilla ei väliä)
            string filter = SearchBox.Text.ToLower();
            //filtrointi tapahtuu
            ApplyFilter(filter);


        }

        private void ApplyFilter(string filter)
        {
            //tyhjennetään listan näkymä, jotta voidaan tulostaa haluttu lista (vain kerran)
            Customers.Clear();
            //haetaan jokainen asiakas "allCustomers" listasta
            foreach (var customer in allCustomers)
            {
                //jos haettu etunimi tai sukunimi on listassa, niin se jää näkyviin listaan
                if (customer.FullName.ToLower().Contains(filter))
                {
                    Customers.Add(customer);
                }
            }
        }

        //nappia painamalla saadaan haettu asiakas "selectedCustomer" muuttujaksi
        private void AcceptSelectedCustomerButton(object sender, RoutedEventArgs e)
            //hyväksy button
        {
            //kunhan "selectedCustomer" on valittu ja on valinta. Suljetaan myös ikkuna
            if (CustomerListBox.SelectedItem is Customer customer)
            {
                SelectedCustomer = customer;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                //virhe viesti jos asiakasta ei ole valittu
                MessageBox.Show("Valitse ensin asiakas");
            }

        }
        private void TiedotClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }
    }
}
