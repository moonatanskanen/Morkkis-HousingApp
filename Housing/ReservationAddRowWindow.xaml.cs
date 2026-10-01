using HousingApp.Housing;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for ReservationAddRowWindow.xaml
    /// </summary>
    public partial class ReservationAddRowWindow : Window
    {
        ObservableCollection<Product> products;
        ObservableCollection<ReservationService> Services;

        /// <summary>
        /// Ottaa parametrina referenssin palvelulistasta
        /// </summary>
        public ReservationAddRowWindow(ref ObservableCollection<ReservationService> services)
        {
            InitializeComponent();

            Services = services;

            products = FindAllProducts();

            ProductBox.ItemsSource = products;
            this.DataContext = new ReservationService();
        }

        /// <summary>
        /// Etsii kaikki toimipisteen palvelut
        /// </summary>
        private ObservableCollection<Product> FindAllProducts()
        {
            ObservableCollection<Product> products = new ObservableCollection<Product>();
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findProducts = new MySqlCommand("SELECT * FROM palvelut WHERE ToimipisteID=@ID", conn);
                findProducts.Parameters.AddWithValue("@ID", MorkkisWindow.OfficeID);
                var reader = findProducts.ExecuteReader();

                while (reader.Read())
                {
                    Product p = new Product();
                    p.ServiceID = reader.GetInt32("PalveluID");
                    p.OfficeID= reader.GetInt32("ToimipisteID");
                    p.Name = reader.GetString("Nimi");
                    p.Price = reader.GetDecimal("Hinta");

                    products.Add(p);
                }
            }

            return products;
        }

        /// <summary>
        /// Ylläpitää palvelun tietoja
        /// </summary>
        private class Product : INotifyPropertyChanged
        {
            public int ServiceID { get; set; }
            public int OfficeID { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            
            private int amount = 0;
            public int Amount 
            {
                get 
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
                    return amount; 
                } 
                set 
                {
                    amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
        }

        /// <summary>
        /// Hyväksyy valitun palvelurivin varaukseen ja sulkee ikkunan
        /// </summary>
        private void AcceptNewRow(object sender, RoutedEventArgs e)
        {
            Product? p = ProductBox.SelectedItem as Product;
            if (p == null) return;

            ReservationService service = this.DataContext as ReservationService;
            service.OfficeID = MorkkisWindow.OfficeID;
            service.ServiceID = p.ServiceID;
            service.Price = p.Price;
            service.Delete = false;
            service.Name = p.Name;
            service.TotalPrice = service.Price * service.Amount;

            if (service.Amount > 0)
            {
                if (Services.Any(s => s.Name == service.Name))
                {
                    MessageBox.Show("Palvelulla on jo laskurivi!\nJos haluat vaihtaa palvelun määrää poista ensin vanha rivi.", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Services.Add(service);
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Määrän pitää olla korkeampi kuin yksi!", "Huomio", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        /// <summary>
        /// Hylkää uuden laskurivin ja sulkee ikkunan
        /// </summary>
        private void DiscardNewRow(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
