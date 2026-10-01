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
using HousingApp.Invoices;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for LaskuIkkuna.xaml
    /// </summary>
    public partial class LaskuIkkuna : Window
    {
        public LaskuIkkuna(Invoice invoice) //ottaa vastaan laskun
        {
            InitializeComponent();

            this.DataContext = invoice;
            dgrows.ItemsSource = invoice.Laskurivit;
        }

        private void PoistuClick(object sender, RoutedEventArgs e) //poistu napin toiminto
        {
            var vastaus = MessageBox.Show("Olet sulkemassa ikkunaa.\n Haluatko jatkaa?", "HUOMAUTUS!", MessageBoxButton.YesNo);

            if (vastaus == MessageBoxResult.Yes)
            {
                Close();
            }
            else
            {
                return;
            }
        }
        private void TiedotClick(object sender, RoutedEventArgs e) //tiedot napin toiminto
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }
    }
}
