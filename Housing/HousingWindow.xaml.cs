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
using HousingApp.Housing;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for HousingWindow.xaml
    /// </summary>
    public partial class HousingWindow : Window
    {
        // Asetetaan samalla kun ikkuna luodaan. Ovat nappuloihin koodattuja arvoja MorkkisWindowissa.
        public static int HousingID = 1;

        /// <summary>
        /// Ottaa parametrina mökkiID:n
        /// </summary>
        public HousingWindow(int id)
        {
            InitializeComponent();
            HousingID = id;
            this.DataContext = Cabin.FindCabin(HousingID);
        }

        /// <summary>
        /// Hakee mökin uudelleen tietokannasta, jotta muokkaukset päivittyvät ajallaan.
        /// </summary>
        private void Refresh()
        {
            this.DataContext = Cabin.FindCabin(HousingID);
        }

        /// <summary>
        /// Sulkee ikkunan
        /// </summary>
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Avaa uuden varausikkunan
        /// </summary>
        private void OpenReservationWindow(object sender, RoutedEventArgs e)
        {
            var reservation = new ReservationWindow();
            reservation.Show();
            Close();
        }

        /// <summary>
        /// Avaa mökki-info ikkunan
        /// </summary>
        private void OpenHousingInfoWindow(object sender, RoutedEventArgs e)
        {
            var housing = new HousingInfoWindow(this.DataContext as Cabin);
            housing.ShowDialog();

            Refresh();
        }

        /// <summary>
        /// Näyttää tietoja ikkunan toiminnasta
        /// </summary>
        private void ShowInfoMessage(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mökin tiedoista pääset vaihtamaan sen vuokrahintaa.\nMökin varauksista pääset luomaan/muuttamaan varauksia.", "INFO", MessageBoxButton.OK);
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
