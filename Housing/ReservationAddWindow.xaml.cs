using HousingApp.Housing;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static HousingApp.Housing.ReservationCalendar;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for ReservationAddWindow.xaml
    /// </summary>
    public partial class ReservationAddWindow : Window
    {
        private bool Changing = false;

        /// <summary>
        /// Ottaa parametrina varauksen
        /// </summary>
        public ReservationAddWindow(Reservation? reservation)
        {
            InitializeComponent();

            /*
            Changing = changing;
            if (Changing)
            {
                cMail.Visibility = Visibility.Hidden;
                cNew.Visibility = Visibility.Hidden;
                cFind.Visibility = Visibility.Hidden;
                SearchBox.Visibility = Visibility.Hidden;
            }*/

            if (reservation != null)
            {
                this.DataContext = reservation;
            }
            else
            {

            }
        }

        /// <summary>
        /// Avaa varauksen varmistusikkunan
        /// </summary>
        private void OpenReservationConfirmationWindow(object sender, RoutedEventArgs e)
        {
            var reservation = this.DataContext as Reservation;

            if(reservation.Customer == null)
            {
                MessageBox.Show("Valitse varaukselle asiakas!", "VAROITUS", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var confirm = new ReservationConfirmationWindow(reservation);
            var result = confirm.ShowDialog();

            if (result == true)
            {
                DialogResult = true;
                //Close();
            }
        }

        /// <summary>
        /// Hylkää tiedot ja sulkee ikkunan
        /// </summary>
        private void DiscardAndClose(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Oletko varma että haluat poistua?\n\nTämä kumoaa kaikki tehdyt muutokset.", "Huomio", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DialogResult = false;
            }
            else
            {
                return;
            }

        }

        /// <summary>
        /// Näyttää tietoja ikkunan toiminnasta
        /// </summary>
        private void ShowInfoMessage(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tässä voit muuttaa varauksen tiedot oikeaksi.", "INFO", MessageBoxButton.OK);
        }

        /// <summary>
        /// Avaa luo-asiakas ikkunan
        /// </summary>
        private void OpenNewCustomerWindow(object sender, RoutedEventArgs e)
        {
            if (Changing) return;

            var customer = new CustomerNewCustomer();
            customer.ShowDialog();
        }

        /// <summary>
        /// Etsii asiakkaan tiedot sähköpostilla
        /// </summary>
        private void FindCustomerByEmail(object sender, RoutedEventArgs e)
        {
            if (Changing) return;

            Button btn = sender as Button;
            string email = SearchBox.Text;

            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findCustomer = new MySqlCommand("SELECT * FROM Asiakas WHERE Sahkoposti=@email;", conn);
                findCustomer.Parameters.AddWithValue("@email", email);
                var reader = findCustomer.ExecuteReader();

                if (reader.Read())
                {
                    ReservationCustomer c = new ReservationCustomer();
                    c.FirstName = reader.GetString("Etunimi");
                    c.LastName = reader.GetString("Sukunimi");
                    c.Email = reader.GetString("Sahkoposti");
                    c.Address = reader.GetString("Osoite");
                    c.Phone = reader.GetString("Puhelin");
                    c.CustomerID = reader.GetInt32("AsiakasID");

                    var res = this.DataContext as Reservation;
                    res.Customer = c;

                    MessageBox.Show("Asiakas löydetty.", "Ilmoitus", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var result = MessageBox.Show("Asiakasta ei löytynyt.\n\nHaluatko lisätä uuden asiakkaan?", "Ilmoitus", 
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.No) return;

                    var customer = new CustomerNewCustomer();
                    customer.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Poistaa palvelurivejä varauksesta
        /// </summary>
        private void DeleteRows(object sender, RoutedEventArgs e)
        {
            Reservation reservation = this.DataContext as Reservation;
            ObservableCollection<ReservationService> temp = new ObservableCollection<ReservationService>();
            foreach (ReservationService item in DataRows.ItemsSource)
            {
                if (!item.Delete)
                {
                    temp.Add(item);
                    continue;
                }

                if (item.Name == "Vuokra")
                {
                    temp.Add(item);
                    MessageBox.Show("Vuokraa ei voi poistaa laskuriveiltä!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                var res = MessageBox.Show($"Haluatko poistaa tuotteen {item.Name} varauksesta?", "Varmistus", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    // TÄMÄ PITÄÄ KUTSUA SAMALLA KUN LASKU HYVÄKSYTÄÄN. MUUTEN TULEE ONGELMIA
                    //reservation.DeleteReservationRow(item.ServiceID);
                }
                else
                {
                    temp.Add(item);
                }
            }

            reservation.Services = temp;
            this.DataContext = reservation;
            DataRows.ItemsSource = reservation.Services;
        }

        /// <summary>
        /// Lisätään uusi palvelurivi varaukselle
        /// </summary>
        private void AddNewRow(object sender, RoutedEventArgs e)
        {
            ObservableCollection<ReservationService> res = (ObservableCollection<ReservationService>)DataRows.ItemsSource;
            ReservationAddRowWindow addRow = new ReservationAddRowWindow(ref res);
            addRow.ShowDialog();
        }

        /// <summary>
        /// Näyttää tietoja ohjelmasta
        /// </summary>
        private void TiedotClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }
    }
}
