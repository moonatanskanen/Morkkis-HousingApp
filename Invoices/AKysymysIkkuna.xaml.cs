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
    /// Interaction logic for AKysymysIkkuna.xaml
    /// </summary>
    public partial class AKysymysIkkuna : Window
    {
        public AKysymysIkkuna()
        {
            InitializeComponent();

        }

        private void EtsiAsiakasClick(object sender, RoutedEventArgs e) //etsi napin toiminto
        {
            InvoiceSQL invoiceSQL = new InvoiceSQL();

            if (invoiceSQL.FindCustomersInvoices(ATextBox.Text).Count == 0)
            {
                MessageBox.Show("Asiakasta ei löytynyt");
                return;
            }

            //luodaan uusi ikkuna ja annetaan parametriksi tekstiboxiin syötetyn asiakkaan sähköpostit
            var AsiakkaanLaskut = new AsiakasLaskut(invoiceSQL.FindCustomersInvoices(ATextBox.Text));
            AsiakkaanLaskut.ShowDialog();

            Close();

        }
    }
}
