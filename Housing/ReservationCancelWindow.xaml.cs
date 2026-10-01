using HousingApp.Housing;
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

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for ReservationCancelWindow.xaml
    /// </summary>
    public partial class ReservationCancelWindow : Window
    {
        /// <summary>
        /// Ottaa parametrina varauksen
        /// </summary>
        public ReservationCancelWindow(Reservation reservation)
        {
            InitializeComponent();
            this.DataContext = reservation;
        }

        /// <summary>
        /// Poistaa varauksen tietokannasta
        /// </summary>
        private void DeleteReservation(object sender, RoutedEventArgs e)
        {
            var res = this.DataContext as Reservation;
            Reservation.DeleteReservation(res.ReservationID);
            DialogResult = true;
        }

        /// <summary>
        /// Sulkee ikkunan muuttamatta tietoja
        /// </summary>
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Näyttää ikkunan tiedot
        /// </summary>
        private void ShowInfoMessage(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tässä voit varmistaa poistettavan varauksen tiedot.", "INFO", MessageBoxButton.OK);
        }

        /// <summary>
        /// Näyttää ohjelman tiedot
        /// </summary>
        private void TiedotClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }
    }
}
