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
    /// Interaction logic for AikaIkkuna.xaml
    /// </summary>
    public partial class AikaIkkuna : Window
    {
        public AikaIkkuna()
        {
            InitializeComponent();
        }

        private void HaeClick(object sender, RoutedEventArgs e) //hae napin toiminto
        {
            InvoiceSQL invoiceSQL = new InvoiceSQL();

            //luodaan uusi ikkuna ja annetaan sille parametreiksi valitut päivämäärät
            var AikaLaskut = new LaskutAjalta((DateTime)alkupaiva.SelectedDate, (DateTime)loppupaiva.SelectedDate);
            
            if (loppupaiva.SelectedDate < alkupaiva.SelectedDate) //virheenhallintaa
            {
                MessageBox.Show("Loppupäivä ei voi olla aikaisemmin kuin alkupäivä");
                return;
            }
            
            AikaLaskut.ShowDialog();
            Close();

        }
    }
}
