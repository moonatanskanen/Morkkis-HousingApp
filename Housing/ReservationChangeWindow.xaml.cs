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

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for ReservationChangeWindow.xaml
    /// </summary>
    public partial class ReservationChangeWindow : Window
    {
        /// <summary>
        /// Ottaa parametrina muutettavan varauksen tiedot
        /// </summary>
        public ReservationChangeWindow(Reservation? reservation)
        {
            InitializeComponent();
            this.DataContext = reservation;
        }

        /// <summary>
        /// Avaa varmistus ikkunan varauksen muutokselle
        /// </summary>
        private void OpenReservationConfirmationWindow(object sender, RoutedEventArgs e)
        {
            var reservation = this.DataContext as Reservation;

            // Tässä pitää tarkistaa että varauksen päivät eivät löydy tietokannasta varattuna.
            if (Reservation.CheckForOtherReservations(reservation.Start, reservation.End, reservation.ReservationID))
            {
                MessageBox.Show("Valitulla aikavälillä on jo toinen varaus!\nValitse varauspäivät uudelleen.", "Huomio", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (reservation.Start > reservation.End)
            {
                MessageBox.Show("Varauksen lopetuspäivä ei voi olla ennen aloituspäivää!\nValitse varauspäivät uudelleen.", "Huomio", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (reservation.Start < DateTime.Today)
            {
                MessageBox.Show("Varauksen aloituspäivä ei voi olla menneisyydessä!\nValitse varauspäivät uudelleen.", "Huomio", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            reservation.StartDate = DateOnly.FromDateTime(reservation.Start);
            reservation.EndDate = DateOnly.FromDateTime(reservation.End);

            var confirm = new ReservationConfirmationWindow(reservation);
            var result = confirm.ShowDialog();

            if (result == true)
            {
                DialogResult = true;
                //Close();
            }
        }

        /// <summary>
        /// Hylkää muutetut tiedot ja sulkee ikkunan
        /// </summary>
        private void DiscardAndClose(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Oletko varma että haluat poistua?\n\nTämä kumoaa kaikki tehdyt muutokset.", "Huomio", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(result == MessageBoxResult.Yes)
            {
                DialogResult = false;
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Näyttää ikkunan tiedot
        /// </summary>
        private void ShowInfoMessage(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tässä voit muuttaa varauksen tiedot oikeaksi.", "INFO", MessageBoxButton.OK);
        }

        /// <summary>
        /// Avaa uusi-asiakas ikkunan
        /// </summary>
        private void OpenNewCustomerWindow(object sender, RoutedEventArgs e)
        {
            var customer = new CustomerNewCustomer();
            customer.ShowDialog();
        }

        /// <summary>
        /// Poistaa palvelurivin varauksesta
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
        /// Lisää palvelurivin varaukseen
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
