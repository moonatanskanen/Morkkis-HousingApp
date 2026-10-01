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
using HousingApp;
using static HousingApp.ReservationConfirmationWindow;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for InvoicePrint.xaml
    /// </summary>
    public partial class InvoicePrint : Window
    {
        public InvoicePrint(Invoice invoice)
        {
            InitializeComponent();

            this.DataContext = invoice;
            SetInvoiceGridValues();
        }

        private void SetInvoiceGridValues()
        {
            var grid = InvoiceRowsGrid;
            var ic = this.DataContext as Invoice;

            // Luodaan uudet labelit jokaiselle laskurivissä löytyvälle tiedolle.
            for (int i = 0; i < ic.InvoiceRows.Count; i++)
            {
                Label name = new Label();
                Label id = new Label();
                Label amount = new Label();
                Label price = new Label();
                Label total = new Label();

                name.HorizontalAlignment = HorizontalAlignment.Left;
                name.VerticalAlignment = VerticalAlignment.Top;
                name.FontSize = 16;
                name.Content = ic.InvoiceRows[i].Name;

                id.HorizontalAlignment = HorizontalAlignment.Left;
                id.VerticalAlignment = VerticalAlignment.Top;
                id.FontSize = 16;
                id.Content = ic.InvoiceRows[i].ServiceID;

                amount.HorizontalAlignment = HorizontalAlignment.Left;
                amount.VerticalAlignment = VerticalAlignment.Top;
                amount.FontSize = 16;
                amount.Content = ic.InvoiceRows[i].Amount;

                price.HorizontalAlignment = HorizontalAlignment.Left;
                price.VerticalAlignment = VerticalAlignment.Top;
                price.FontSize = 16;
                price.Content = ic.InvoiceRows[i].Price;

                total.HorizontalAlignment = HorizontalAlignment.Left;
                total.VerticalAlignment = VerticalAlignment.Top;
                total.FontSize = 16;
                total.Content = ic.InvoiceRows[i].TotalPrice;

                // Gridin sijainnit
                Grid.SetRow(name, i); Grid.SetColumn(name, 0);
                Grid.SetRow(id, i); Grid.SetColumn(id, 1);
                Grid.SetRow(amount, i); Grid.SetColumn(amount, 2);
                Grid.SetRow(price, i); Grid.SetColumn(price, 3);
                Grid.SetRow(total, i); Grid.SetColumn(total, 4);

                // Lisätään gridin childeiksi
                grid.Children.Add(name);
                grid.Children.Add(id);
                grid.Children.Add(amount);
                grid.Children.Add(price);
                grid.Children.Add(total);
            }
        }
    }
}
