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
    /// Interaction logic for MorkkisWindow.xaml
    /// </summary>          
    public partial class MorkkisWindow : Window
    {
        public static int OfficeID;

        public MorkkisWindow(int officeID)
        {
            InitializeComponent();
            OfficeID = officeID;

            var cabins = Cabin.FindAllOfficeCabins(OfficeID);
            Cabin1.Content = cabins[0].Name;
            Cabin2.Content = cabins[1].Name;
            Cabin3.Content = cabins[2].Name;

        }

        private void OpenServiceWindow(object sender, RoutedEventArgs e)
        {
            var service = new ServiceWindow();
            service.ShowDialog();
        }

        private void OpenInvoiceWindow(object sender, RoutedEventArgs e)
        {
            var invoice = new InvoiceWindow();
            invoice.ShowDialog();
        }

        private void OpenCustomerWindow(object sender, RoutedEventArgs e)
        {
            var customer = new CustomerWindow();
            customer.ShowDialog();
        }

        private void OpenAnalyticsWindow(object sender, RoutedEventArgs e)
        {
            var anal = new AnalyticsWindow();
            anal.ShowDialog();
        }

        private void OpenHousingWindow(object sender, RoutedEventArgs e)
        {
            var cabin = sender as Button;
            int id = -1;

            var cabinIDs = Cabin.FindAllOfficeCabins(OfficeID);

            switch (cabin.Name)
            {
                case "Cabin1":
                    id = cabinIDs[0].CabinID;
                    break;
                case "Cabin2":
                    id = cabinIDs[1].CabinID;
                    break;
                case "Cabin3":
                    id = cabinIDs[2].CabinID;
                    break;
            }

            var housing = new HousingWindow(id);
            housing.ShowDialog();
        }

        public void PoistuClick(object sender, RoutedEventArgs e)
        {
            var vastaus = MessageBox.Show("Olet sulkemassa ohjelmaa.\n Haluatko jatkaa?", "HUOMAUTUS!", MessageBoxButton.YesNo);

            if (vastaus == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
            else
            {
                return;
            }
        }

        private void TiedotClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }

    }
}
