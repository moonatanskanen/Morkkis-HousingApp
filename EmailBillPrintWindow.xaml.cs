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
using static HousingApp.ReservationConfirmationWindow;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for EmailBillPrintWindow.xaml
    /// </summary>
    public partial class EmailBillPrintWindow : Window
    {
        public EmailBillPrintWindow(Invoice invoice)
        {
            InitializeComponent();

            this.DataContext = invoice;
        }
    }
}
