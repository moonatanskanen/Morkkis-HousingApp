using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HousingApp
{
    /// <summary>
    /// Interaction logic for ReservationConfirmationWindow.xaml
    /// </summary>
    public partial class ReservationConfirmationWindow : Window
    {
        /// <summary>
        /// Ylläpitää laskun tietoja
        /// </summary>
        public class Invoice
        {
            public int InvoiceID { get; set; }
            public DateTime Date { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName => $"{FirstName} {LastName}";
            public string Office { get; set; }
            public string Cabin { get; set; }
            public bool Invalidated { get; set; }
            public int CustomerID { get; set; }
            public int ReservationID { get; set; }
            public int OfficeID { get; set; }
            public int CabinID { get; set; }
            public ObservableCollection<ReservationService> InvoiceRows { get; set; }
            public decimal TotalPrice { get; set; }
        }

        Invoice invoice = new Invoice();

        /// <summary>
        /// Ottaa parametrina varauksen
        /// </summary>
        public ReservationConfirmationWindow(Reservation res)
        {
            InitializeComponent();

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

            foreach(var r in invoice.InvoiceRows)
            {
                invoice.TotalPrice += r.TotalPrice;
            }

            this.DataContext = invoice;
        }

        /// <summary>
        /// Hakee toimipisteen nimen ToimipisteID:llä
        /// </summary>
        public static string GetOfficeName(int ID)
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findOffice = new MySqlCommand("SELECT Nimi FROM Toimipiste WHERE ToimipisteID=@id", conn);
                findOffice.Parameters.AddWithValue("@id", ID);

                var reader = findOffice.ExecuteReader();
                reader.Read();
                
                return reader.GetString("Nimi");
            }
        }

        /// <summary>
        /// Tallenna muutokset ja tulosta varauksen tiedot
        /// </summary>
        private void ConfirmAndExit(object sender, RoutedEventArgs e)
        {

            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                Invoice i = invoice;


                // Tallenna lasku..
                // Tallenna varauksen_palvelut..
                // Tallenna laskurivit..
                MySqlCommand findReservation = new MySqlCommand("SELECT * FROM Varaus WHERE VarausID=@resID", conn);
                findReservation.Parameters.AddWithValue("@resID", i.ReservationID);
                var original = findReservation.ExecuteReader();

                if (original.Read())
                {
                    Reservation.DeleteReservation(i.ReservationID);
                }

                original.Close();



                MySqlCommand saveReservation = new MySqlCommand("INSERT INTO varaus(alkupaivamaara, loppupaivamaara, asiakasID, mokkiID, toimipisteID) " +
                "VALUES (@start, @end, @cusID, @cabID, @ofID);", conn);
                saveReservation.Parameters.AddWithValue("@start", i.StartDate);
                saveReservation.Parameters.AddWithValue("@end", i.EndDate);
                saveReservation.Parameters.AddWithValue("@cusID", i.CustomerID);
                saveReservation.Parameters.AddWithValue("@cabID", i.CabinID);
                saveReservation.Parameters.AddWithValue("@ofID", i.OfficeID);
                saveReservation.ExecuteNonQuery();
                long reservationID = saveReservation.LastInsertedId;

                MySqlCommand saveInvoice = new MySqlCommand("INSERT INTO lasku " +
                    "(paivamaara, erapaiva, osoite, puhelin, sahkoposti, etunimi, sukunimi,  toimipiste, mokki, mitatoitu, asiakasID, varausID) " +
                    "VALUES (@pv, @ev, @ad, @ph, @em, @fn, @ln, @tp, @cb, @mt, @cid, @rid);", conn);
                saveInvoice.Parameters.AddWithValue("@pv", i.Date);
                saveInvoice.Parameters.AddWithValue("@ev", i.DueDate);
                saveInvoice.Parameters.AddWithValue("@ad", i.Address);
                saveInvoice.Parameters.AddWithValue("@ph", i.Phone);
                saveInvoice.Parameters.AddWithValue("@em", i.Email);
                saveInvoice.Parameters.AddWithValue("@fn", i.FirstName);
                saveInvoice.Parameters.AddWithValue("@ln", i.LastName);
                saveInvoice.Parameters.AddWithValue("@tp", i.Office);
                saveInvoice.Parameters.AddWithValue("@cb", i.Cabin);
                saveInvoice.Parameters.AddWithValue("@mt", i.Invalidated);
                saveInvoice.Parameters.AddWithValue("@cid", i.CustomerID);
                saveInvoice.Parameters.AddWithValue("@rid", reservationID);
                saveInvoice.ExecuteNonQuery();
                long invoiceID = saveInvoice.LastInsertedId;


                foreach (var s in i.InvoiceRows)
                {
                    MySqlCommand saveRow = new MySqlCommand("INSERT INTO laskurivi(tuotenimi, hinta, maara, laskuID) " +
                        "VALUES(@name, @total, @amount, @invoiceID)", conn);
                    saveRow.Parameters.AddWithValue("@name", s.Name);
                    saveRow.Parameters.AddWithValue("@total", s.TotalPrice);
                    saveRow.Parameters.AddWithValue("@amount", s.Amount);
                    saveRow.Parameters.AddWithValue("@invoiceID", invoiceID);

                    saveRow.ExecuteNonQuery();

                    if (s.Name == "Vuokra")
                        continue;

                    MySqlCommand saveService = new MySqlCommand("INSERT INTO varauksen_palvelut(palveluID, varausID) VALUES(@serviceID, @reservationID);", conn);
                    saveService.Parameters.AddWithValue("@serviceID", s.ServiceID);
                    saveService.Parameters.AddWithValue("@reservationID", reservationID);

                    saveService.ExecuteNonQuery();
                }
            }


            // Tallenna kaikki tiedot tietokantaan.
            MessageBox.Show("Varauksen tiedot tallennettu onnistuneesti.");
            
            if(CBpaper.IsChecked == true)
            {
                InvoicePrint print = new InvoicePrint(invoice);
                print.ShowDialog();
            }
            else
            {
                EmailBillPrintWindow print = new EmailBillPrintWindow(invoice);
                print.ShowDialog();
            }


            DialogResult = true;
        }

        /// <summary>
        /// Hylkää tiedot ja sulje ikkuna
        /// </summary>
        private void DiscardAndExit(object sender, RoutedEventArgs e)
        {
            // Palaa takaisin tallentamatta mitään.
            MessageBox.Show("Varauksen tietoja ei tallennettu.");
            DialogResult = false;
        }

        /// <summary>
        /// Näytä ikkunan tiedot
        /// </summary>
        private void ShowInfoMessage(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tässä voit varmistaa että varauksen ja laskun tiedot ovat varmasti oikein.", "INFO", MessageBoxButton.OK);
        }

        /// <summary>
        /// Valitse tulostettavan laskun tyyppi
        /// </summary>
        private void ChooseInvoiceType(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;

            if(checkbox.Name == "CBpaper")
            {
                CBpaper.IsChecked = true;
                CBemail.IsChecked = false;
            }
            else
            {
                CBpaper.IsChecked = false;
                CBemail.IsChecked = true;
            }

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
