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
    /// Interaction logic for HousingInfoWindow.xaml
    /// </summary>
    public partial class HousingInfoWindow : Window
    {
        /// <summary>
        /// Konstruktori ottaa mökin parametrinä
        /// </summary>
        public HousingInfoWindow(Cabin cabin)
        {
            InitializeComponent();
            this.DataContext = cabin;
        }

        /// <summary>
        /// Näyttää infoa ikkunasta
        /// </summary>
        private void ShowInfoMessage(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tässä ikkunassa voit muuttaa valitun mökin vuorokausihintaa.", "INFO", MessageBoxButton.OK);
        }

        /// <summary>
        /// Tallentaa muutetut tiedot ja poistuu ikkunasta
        /// </summary>
        private void SaveAndExit(object sender, RoutedEventArgs e)
        {
            Cabin cab = this.DataContext as Cabin;

            if (cab.Price <= 0)
            {
                MessageBox.Show("Syötä mökille hinta, joka on korkeampi kuin 0!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            cab.SaveCabin();

            DialogResult = true;
        }

        /// <summary>
        /// Peru muutokset ja poistu ikkunasta
        /// </summary>
        private void DiscardAndExit(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
