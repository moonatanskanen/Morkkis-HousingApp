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
    /// Interaction logic for InvoiceWindow.xaml
    /// </summary>
    public partial class InvoiceWindow : Window
    {
        InvoiceSQL invoiceSQL = new InvoiceSQL();
        public InvoiceWindow()
        {
            
            InitializeComponent();

            
            dg.ItemsSource = invoiceSQL.GetInvoices();

        }

        private void AvaaClick(object sender, RoutedEventArgs e) //avaa laskunäkymän
        {
            //tallennetaan painetun napin tiedot muuttujaan (tiedetään mitä nappia on painettu ja missä se sijaitsee)
            var nappula = sender as Button;

            var NappulaParent = VisualTreeHelper.GetParent(nappula); //haetaan nappulan parent ja tallennetaan se muuttujaan

            //jos nappulaparent ei ole null ja se ei ole datagridcell, siirrytään while loopin sisälle
            while (NappulaParent != null && !(NappulaParent is DataGridCell))
            {
                //otetaan hierarkiassa vielä ylempi parent aikaisemmalle ja tallennetaan se uudestaan muuttujaan
                NappulaParent = VisualTreeHelper.GetParent(NappulaParent);
            }

            var IsoinParent = VisualTreeHelper.GetParent(NappulaParent);//tallennetaan muuttujaan nappulaparentissa olevan datagridcellin parent

            //haetaan niin pitkään uutta parenttia, kunnes isoinparent on datagridrow
            while (IsoinParent != null && !(IsoinParent is DataGridRow))
            {
                IsoinParent = VisualTreeHelper.GetParent(IsoinParent);
            }

            var DataRivi = IsoinParent as DataGridRow; //asetetaan datagridrow muuttujaan

            var item = DataRivi.Item; //otetaan talteen rivin item ja tallennetaan se muuttujaan

            if (item is Invoice lasku) //varmistetaan että item on lasku
            {
                var LaskuNakyma = new LaskuIkkuna(lasku);
                LaskuNakyma.ShowDialog();
                dg.ItemsSource = invoiceSQL.GetInvoices();
            }
        }

        public void PoistuClick(object sender, RoutedEventArgs e) //poistu napin toiminto
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

        private void AsiakasHakuClick(object sender, RoutedEventArgs e) //asiakashaku napin toiminto
        {
            var AsiakasKysymys = new AKysymysIkkuna();
            AsiakasKysymys.ShowDialog();
            dg.ItemsSource = invoiceSQL.GetInvoices();
        }

        private void AikahakuClick(object sender, RoutedEventArgs e) //päivämäärien perusteella haku toiminto
        {
            var MikaAikaVali = new AikaIkkuna();
            MikaAikaVali.ShowDialog();
            dg.ItemsSource = invoiceSQL.GetInvoices();
        }


        private void TiedotClick(object sender, RoutedEventArgs e) //tieto menuitem click
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }
    }
}
