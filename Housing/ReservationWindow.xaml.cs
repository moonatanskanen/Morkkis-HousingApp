using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using Mysqlx.Session;
using MySqlX.XDevAPI.Common;
using static HousingApp.ReservationConfirmationWindow;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for ReservationWindow.xaml
    /// </summary>
    public partial class ReservationWindow : Window
    {
        public ReservationWindow()
        {
            InitializeComponent();

            // Luodaan uusi kalenteri
            ReservationCalendar cal = new ReservationCalendar();
            cal.ChosenMonth = (ReservationCalendar.Month)DateTime.Now.Month;
            cal.ChosenYear = DateTime.Now.Year;
            cal.CalendarGrid = GridCalendar;
            this.DataContext = cal;

            CreateReservationCalendar();
        }

        /// <summary>
        /// Luo uuden varauskalenterin
        /// </summary>
        private void CreateReservationCalendar()
        {
            ReservationCalendar cal = this.DataContext as ReservationCalendar;
            cal.Days = new ObservableCollection<Button>();

            for (int d = 1; d <= 7; d++)
            {
                GridCalendar.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int w = 1; w <= 5; w++)
            {
                GridCalendar.RowDefinitions.Add(new RowDefinition());
            }

            // Create 31 buttons and place them in the grid
            for (int day = 1; day <= 31; day++)
            {
                var btn = new Button
                {
                    Content = day.ToString()
                };

                btn.FontSize = 18;
                btn.FontFamily = new FontFamily("Carlito");
                btn.Background = new SolidColorBrush(Colors.LightGray);
                btn.BorderBrush = new SolidColorBrush(Colors.Gray);
                btn.Click += SelectDateButtonClicked;

                // Calculate row and column
                int row = (day - 1) / 7;
                int column = (day - 1) % 7;

                // Assign Grid.Row and Grid.Column
                Grid.SetRow(btn, row);
                Grid.SetColumn(btn, column);

                // Add to grid
                GridCalendar.Children.Add(btn);
                cal.Days.Add(btn);
            }
            cal.UpdateDayButtonVisibility();
            cal.UpdateButtonColors();
        }

        /// <summary>
        /// Varauspäivän valinta
        /// </summary>
        private void SelectDateButtonClicked(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var cal = this.DataContext as ReservationCalendar;
            var date = new DateOnly(cal.ChosenYear, (int)cal.ChosenMonth, cal.Days.IndexOf(btn) + 1);

            // Kun valitaan varattu päivä, valitaan koko se varaus kerrallaan.
            if (((SolidColorBrush)btn.Background).Color == ReservationCalendar.ReservedColor)
            {
                var res = Reservation.FindReservationByDate(date);
                cal.StartDate = res.StartDate;
                cal.EndDate = res.EndDate;
                cal.ReservedSelect = true;
                cal.UpdateButtonColors();
                return;
            }

            if (cal.StartDate == null && cal.EndDate == null)
            {
                cal.StartDate = date;
            }
            else if(cal.StartDate != null && cal.EndDate == null)
            {
                // Jos valitulla varausvälillä on muitakin varauksia, ei valita mitään.
                if (Reservation.CheckForReservations(cal.StartDate.Value, date))
                {
                    MessageBox.Show("Valitulla alueella on jo varaus!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cal.StartDate = null;
                    cal.EndDate = null;
                    cal.ReservedSelect = false;
                    cal.UpdateButtonColors();
                    return;
                }

                if (date < cal.StartDate)
                {
                    MessageBox.Show("Valitse ensiksi aloituspäivämäärä, sen jälkeen lopetuspäivämäärä!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cal.StartDate = null;
                    cal.EndDate = null;
                    cal.ReservedSelect = false;
                    cal.UpdateButtonColors();
                    return;
                }

                cal.EndDate = date;
            }
            else if (cal.StartDate != null && cal.EndDate != null)
            {
                cal.StartDate = null;
                cal.EndDate = null;
                cal.ReservedSelect = false;
                cal.UpdateButtonColors();
                return;
            }

            cal.UpdateButtonColors();
            btn.Background = new SolidColorBrush(cal.SelectedColor);

        }

        /// <summary>
        /// Avataan varauksen luonti-ikkuna
        /// </summary>
        private void OpenAddReservationWindow(object sender, RoutedEventArgs e)
        {
            var cal = this.DataContext as ReservationCalendar;

            if (cal.ReservedSelect)
            {
                MessageBox.Show("Tämä aika on varattu.\nVoit muokata sitä 'Muuta Varausta' painikkeesta.", "Ilmoitus", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (cal.StartDate == null || cal.EndDate == null)
            {
                MessageBox.Show("Valitse varaukselle ensin aika kalenterista!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if(cal.StartDate < DateOnly.FromDateTime(DateTime.Today))
            {
                MessageBox.Show("Varauksen alkamispäivä ei voi olla aikaisempi kuin nykyinen päivä!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Reservation res = new Reservation();
            res.CabinID = HousingWindow.HousingID;
            res.Cabin = Cabin.FindCabin(HousingWindow.HousingID);
            res.StartDate = cal.StartDate.Value;
            res.EndDate = cal.EndDate.Value;
            res.Services.Add(new ReservationService());
            res.Services[0].Amount = (res.EndDate.DayNumber - res.StartDate.DayNumber + 1);
            res.Services[0].Name = "Vuokra";
            res.Services[0].Price = (decimal)res.Cabin.Price;
            res.Services[0].TotalPrice = (decimal)((res.EndDate.DayNumber - res.StartDate.DayNumber + 1) * res.Cabin.Price);
            res.OfficeID = MorkkisWindow.OfficeID;

            var addnew = new ReservationAddWindow(res);
            var result = addnew.ShowDialog();

            if (result == true)
            {
                cal.StartDate = null;
                cal.EndDate = null;
                cal.ReservedSelect = false;
                cal.UpdateButtonColors();
            }
        }

        /// <summary>
        /// Avataan varauksen poisto-ikkuna
        /// </summary>
        private void OpenCancelReservationWindow(object sender, RoutedEventArgs e)
        {
            var cal = this.DataContext as ReservationCalendar;

            if (cal.StartDate <= DateOnly.FromDateTime(DateTime.Today))
            {
                MessageBox.Show("Alkanutta varausta ei voi enää perua!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cal.StartDate == null || cal.EndDate == null || !cal.ReservedSelect)
            {
                MessageBox.Show("Valitse peruttava varaus kalenterista!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cancel = new ReservationCancelWindow(Reservation.FindReservationByDate(cal.StartDate.Value));
            var result = cancel.ShowDialog();

            if(result == true)
            {
                cal.StartDate = null;
                cal.EndDate = null;
                cal.ReservedSelect = false;
                cal.UpdateButtonColors();
            }
        }
        /// <summary>
        /// Avataan varauksen muutos-ikkuna
        /// </summary>
        private void OpenChangeReservationWindow(object sender, RoutedEventArgs e)
        {
            var cal = this.DataContext as ReservationCalendar;

            if (cal.StartDate == null || cal.EndDate == null || !cal.ReservedSelect)
            {
                MessageBox.Show("Valitse muokattava varaus kalenterista!", "Varoitus", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cal.StartDate <= DateOnly.FromDateTime(DateTime.Today))
            {
                var r = MessageBox.Show("Alkanutta varausta ei voi enää muokata!\n" +
                    "Haluatko tarkistaa varauksesta tehdyn laskun?", "Huomio", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (r == MessageBoxResult.Yes)
                {
                    Reservation res = Reservation.FindReservationByDate(cal.StartDate.Value);
                    Invoice invoice = new Invoice();

                    invoice.Date = res.StartDate.ToDateTime(TimeOnly.MinValue);
                    invoice.DueDate = invoice.Date.AddDays(14);
                    invoice.Address = res.Customer.Address;
                    invoice.Phone = res.Customer.Phone;
                    invoice.Email = res.Customer.Email;
                    invoice.FirstName = res.Customer.FirstName;
                    invoice.LastName = res.Customer.LastName;
                    invoice.Office = GetOfficeName(res.OfficeID);
                    invoice.Cabin = res.Cabin.Name;
                    invoice.Invalidated = false;
                    invoice.CustomerID = res.Customer.CustomerID;
                    invoice.ReservationID = res.ReservationID;
                    invoice.InvoiceRows = res.Services;
                    invoice.StartDate = res.Start;
                    invoice.EndDate = res.End;
                    invoice.OfficeID = res.OfficeID;
                    invoice.CabinID = res.CabinID;

                    foreach (var row in invoice.InvoiceRows)
                    {
                        invoice.TotalPrice += row.TotalPrice;
                    }

                    InvoicePrint print = new InvoicePrint(invoice);
                    print.ShowDialog();

                }

                return;
            }

            Reservation ress = Reservation.FindReservationByDate(cal.StartDate.Value);

            var addnew = new ReservationChangeWindow(ress);
            var result = addnew.ShowDialog();

            if (result == true)
            {
                cal.StartDate = null;
                cal.EndDate = null;
                cal.ReservedSelect = false;
                cal.UpdateButtonColors();
            }
        }

        /// <summary>
        /// Suljetaan ikkuna
        /// </summary>
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
            //DialogResult = false;
        }

        /// <summary>
        /// Näytetään tietoja ikkunasta
        /// </summary>
        private void ShowInfoMessage(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tässä ikkunassa voit hallita varauksia." +
                "\n\nKalenterissa näkyy harmaalla vapaat päivät.\nPunaisella varatut päivät.\nNykyinen valinta näkyy vihreänä." +
                "\n\nValitsemalla varatun päivän ja klikkaamalla muuta varaus, pääset muuttamaan varauksen tietoja." +
                "\nValitsemalla vapaan alueen ja klikkaamalla uusi varaus, pääset luomaan uuden varauksen.", "INFO", MessageBoxButton.OK);
        }

        /// <summary>
        /// Valitaan seuraava kalenterikuukausi
        /// </summary>
        private void NextMonth(object sender, RoutedEventArgs e)
        {
            var calendar = this.DataContext as ReservationCalendar;
            calendar.ChosenMonth += 1;
        }

        /// <summary>
        /// Valitaan edellinen kalenterikuukausi
        /// </summary>
        private void PreviousMonth(object sender, RoutedEventArgs e)
        {
            var calendar = this.DataContext as ReservationCalendar;
            calendar.ChosenMonth -= 1;
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
