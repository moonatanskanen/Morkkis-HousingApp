using HousingApp.Invoices;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HousingApp.Housing
{
    /// <summary>
    /// Ylläpitää varauksen tietoja
    /// </summary>
    public class Reservation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // VarausID primary key
        public int ReservationID { get; set; }
        // AsiakasID foreign key
        public int CustomerID { get; set; }
        // MokkiID foreign key
        public int CabinID { get; set; }
        // ToimipisteID foreign key
        public int OfficeID { get; set; }
        // Alkupaivamaara
        private DateOnly startdate;
        public DateOnly StartDate 
        {
            get { return startdate; }
            set 
            { 
                startdate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartDate"));
                Start = startdate.ToDateTime(TimeOnly.MinValue);

                // Laskee vuokralle hinnan varauspäivien mukaan
                foreach(var s in Services)
                {
                    if(s.Name == "Vuokra")
                    {
                        s.Amount = (EndDate.DayNumber - StartDate.DayNumber + 1);
                        s.TotalPrice = (decimal)((EndDate.DayNumber - StartDate.DayNumber + 1) * Cabin.Price);
                        PropertyChanged?.Invoke(s, new PropertyChangedEventArgs("TotalPrice"));
                        PropertyChanged?.Invoke(s, new PropertyChangedEventArgs("Amount"));
                    }
                }
            }
        }
        // Loppupaivamaara
        private DateOnly enddate;
        public DateOnly EndDate 
        {
            get { return enddate; }
            set 
            {
                enddate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EndDate"));
                End = enddate.ToDateTime(TimeOnly.MinValue);

                // Laskee vuokralle hinnan varauspäivien mukaan
                foreach (var s in Services)
                {
                    if (s.Name == "Vuokra")
                    {
                        s.Amount = (EndDate.DayNumber - StartDate.DayNumber + 1);
                        s.TotalPrice = (decimal)((EndDate.DayNumber - StartDate.DayNumber + 1) * Cabin.Price);
                        PropertyChanged?.Invoke(s, new PropertyChangedEventArgs("TotalPrice"));
                        PropertyChanged?.Invoke(s, new PropertyChangedEventArgs("Amount"));

                    }
                }
            }
        }

        //Päivämäräät myös DateTimena bindingien helpottamista varten
        private DateTime start;
        public DateTime Start
        {
            get { return start; }
            set
            {
                start = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Start"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartDate"));
            }
        }

        private DateTime end;
        public DateTime End
        {
            get { return end; }
            set
            {
                end = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("End"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EndDate"));
            }
        }


        private ReservationCustomer customer;
        public ReservationCustomer Customer 
        {
            get
            {
                return customer;
            }
            set
            {
                customer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Customer"));
            }
        }
        public Cabin Cabin { get; set; }

        // Lista kaikista palveluista, joita varaukseen kuuluu
        public ObservableCollection<ReservationService> Services { get; set; } = new ObservableCollection<ReservationService>();

        /// <summary>
        /// Etsii kaikki valitun kalenterikuukauden mökkivaraukset tietyssä toimipisteessä
        /// </summary>
        public static ObservableCollection<Reservation> FindMonthlyCabinReservations(int year, int month)
        {
            ObservableCollection<Reservation> res = new ObservableCollection<Reservation>();
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                var reservations = FindAllCabinReservations();
                res = reservations;

                
                foreach (Reservation r in reservations)
                {
                    if(r.StartDate.Year == year && r.StartDate.Month == month || r.EndDate.Year == year && r.EndDate.Month == month) 
                    { 
                        res.Add(r); 
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Etsii kaikki toimipisteen mökkivaraukset
        /// </summary>
        public static ObservableCollection<Reservation> FindAllCabinReservations()
        {
            ObservableCollection<Reservation> reservations = new ObservableCollection<Reservation>();
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                //MessageBox.Show($"Mökki ID: {HousingWindow.HousingID}");

                MySqlCommand findReservations = new MySqlCommand("SELECT * FROM Varaus WHERE ToimipisteID=@ID AND MokkiID=@CabID;", conn);
                findReservations.Parameters.AddWithValue("@ID", MorkkisWindow.OfficeID);
                findReservations.Parameters.AddWithValue("@CabID", HousingWindow.HousingID);
                var reader = findReservations.ExecuteReader();

                while (reader.Read())
                {

                    Reservation r = new Reservation();
                    r.ReservationID = reader.GetInt32("VarausID");
                    r.CustomerID = reader.GetInt32("AsiakasID");
                    r.CabinID = reader.GetInt32("MokkiID");
                    r.OfficeID = reader.GetInt32("ToimipisteID");
                    r.StartDate = DateOnly.FromDateTime(reader.GetDateTime("Alkupaivamaara"));
                    r.EndDate = DateOnly.FromDateTime(reader.GetDateTime("Loppupaivamaara"));

                    r.Customer = ReservationCustomer.FindCustomerByID(reader.GetInt32("AsiakasID"));
                    r.Cabin = Cabin.FindCabin(reader.GetInt32("MokkiID"));
                    r.Services = ReservationService.FindReservationServices(reader.GetInt32("VarausID"));

                    //MessageBox.Show($"Varaus ID:lla {r.ReservationID} löydetty!");


                    reservations.Add(r);
                }
            }

            return reservations;
        }

        /// <summary>
        /// Etsii varauksen tietyllä päivämäärällä
        /// </summary>
        public static Reservation FindReservationByDate(DateOnly date)
        {
            Reservation res = new Reservation();

            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findReservation = new MySqlCommand("SELECT * FROM Varaus WHERE ToimipisteID=@ID AND MokkiID=@CabID AND @date BETWEEN Alkupaivamaara AND Loppupaivamaara;", conn);
                findReservation.Parameters.AddWithValue("@ID", MorkkisWindow.OfficeID);
                findReservation.Parameters.AddWithValue("@CabID", HousingWindow.HousingID);
                findReservation.Parameters.AddWithValue("@date", date.ToDateTime(TimeOnly.MinValue));
                var reader = findReservation.ExecuteReader();

                reader.Read();

                res.ReservationID = reader.GetInt32("VarausID");
                res.CustomerID = reader.GetInt32("AsiakasID");
                res.CabinID = reader.GetInt32("MokkiID");
                res.OfficeID = reader.GetInt32("ToimipisteID");

                res.StartDate = DateOnly.FromDateTime(reader.GetDateTime("Alkupaivamaara"));
                res.EndDate = DateOnly.FromDateTime(reader.GetDateTime("Loppupaivamaara"));

                res.Customer = ReservationCustomer.FindCustomerByID(reader.GetInt32("AsiakasID"));
                res.Cabin = Cabin.FindCabin(reader.GetInt32("MokkiID"));
                res.Services = ReservationService.FindReservationServices(reader.GetInt32("VarausID"));

            }

            return res;
        }

        /// <summary>
        /// Tarkistaa onko valitulla aikavälillä varauksia
        /// </summary>
        public static bool CheckForReservations(DateOnly start, DateOnly end)
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findReservation = new MySqlCommand("SELECT * FROM Varaus WHERE ToimipisteID=@ID AND MokkiID=@CabID " +
                    "AND NOT (Loppupaivamaara < @datestart OR Alkupaivamaara > @dateend)", conn);
                findReservation.Parameters.AddWithValue("@ID", MorkkisWindow.OfficeID);
                findReservation.Parameters.AddWithValue("@CabID", HousingWindow.HousingID);
                findReservation.Parameters.AddWithValue("@datestart", start.ToDateTime(TimeOnly.MinValue));
                findReservation.Parameters.AddWithValue("@dateend", end.ToDateTime(TimeOnly.MinValue));

                var reader = findReservation.ExecuteReader();

                if (reader.Read()) return true;
            }

            return false;
        }

        /// <summary>
        /// Tarkistaa onko valitulla aikavälillä varauksia, jolla ei ole parametrina syötetty VarausID
        /// </summary>
        public static bool CheckForOtherReservations(DateTime start, DateTime end, int reservationID)
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findReservation = new MySqlCommand("SELECT * FROM Varaus WHERE ToimipisteID=@ID AND MokkiID=@CabID AND NOT VarausID=@resID " +
                    "AND NOT (Loppupaivamaara < @datestart OR Alkupaivamaara > @dateend)", conn);
                findReservation.Parameters.AddWithValue("@resID", reservationID);
                findReservation.Parameters.AddWithValue("@ID", MorkkisWindow.OfficeID);
                findReservation.Parameters.AddWithValue("@CabID", HousingWindow.HousingID);
                findReservation.Parameters.AddWithValue("@datestart", start);
                findReservation.Parameters.AddWithValue("@dateend", end);

                var reader = findReservation.ExecuteReader();

                if (reader.Read()) return true;
            }

            return false;
        }

        /// <summary>
        /// Poistetaan varaus tietokannasta ID:n perusteella
        /// </summary>
        public static void DeleteReservation(int reservationID)
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();
                var tr = conn.BeginTransaction();

                MySqlCommand invalidateInvoice = new MySqlCommand("UPDATE Lasku SET Mitatoitu=1 WHERE VarausID=@ID", conn, tr);
                MySqlCommand deleteServices = new MySqlCommand("DELETE FROM Varauksen_Palvelut WHERE VarausID=@ID", conn, tr);
                MySqlCommand deleteReservation = new MySqlCommand("DELETE FROM Varaus WHERE VarausID=@ID", conn, tr);

                invalidateInvoice.Parameters.AddWithValue("@ID", reservationID);
                deleteServices.Parameters.AddWithValue("@ID", reservationID);
                deleteReservation.Parameters.AddWithValue("@ID", reservationID);

                invalidateInvoice.ExecuteReader().Close();
                deleteServices.ExecuteReader().Close();
                deleteReservation.ExecuteReader().Close();

                tr.Commit();
            }
        }

        /// <summary>
        /// Poistetaan varaukselta laskurivi palveluID:n perusteella
        /// </summary>
        public void DeleteReservationRow(int serviceID)
        {
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();
                //var tr = conn.BeginTransaction();

                // Lasku pitää mitätöidä muutosten jälkeen kun luodaan uusi.

                MySqlCommand deleteServiceRow = new MySqlCommand("DELETE FROM Varauksen_Palvelut WHERE VarausID=@ID AND PalveluID=@deleteID", conn);
                deleteServiceRow.Parameters.AddWithValue("@ID", ReservationID);
                deleteServiceRow.Parameters.AddWithValue("@deleteID", serviceID);

                deleteServiceRow.ExecuteReader().Close();

                //tr.Commit();
            }
        }
    }

    /// <summary>
    /// Ylläpitää tietoja varauksen asiakkaasta
    /// </summary>
    public class ReservationCustomer
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        /// <summary>
        /// Etsii asiakkaan AsiakasID:n perusteella
        /// </summary>
        public static ReservationCustomer FindCustomerByID(int ID)
        {
            ReservationCustomer cus = new ReservationCustomer();

            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findCustomer = new MySqlCommand("SELECT * FROM Asiakas WHERE AsiakasID=@ID;", conn);
                findCustomer.Parameters.AddWithValue("@ID", ID);
                var reader = findCustomer.ExecuteReader();

                reader.Read();

                cus.Email = reader.GetString("Sahkoposti");
                cus.Address = reader.GetString("Osoite");
                cus.LastName = reader.GetString("Sukunimi");
                cus.FirstName = reader.GetString("Etunimi");
                cus.Phone = reader.GetString("Puhelin");
                cus.CustomerID = reader.GetInt32("AsiakasID");
            }

            return cus;
        }

    }

    /// <summary>
    /// Ylläpitää tietoja varauksen palvelusta
    /// </summary>
    public class ReservationService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int ServiceID { get; set; }
        public int OfficeID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        private decimal total = 0;
        public decimal TotalPrice 
        { 
            get { return total; }
            set
            {
                total = Amount * Price;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalPrice"));
            }
        }
        private int amount = 0;
        public int Amount 
        { 
            get { return amount; }
            set
            {
                amount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalPrice"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Amount"));
            }
        }

        public bool Delete { get; set; } = false;

        public ReservationService()
        {
            ServiceID = 0;
            OfficeID = 0;
            Name = "";
            Price = 0;
            TotalPrice = 0;
        }

        /// <summary>
        /// Etsii kaikki varauksen palvelut VarausID:llä
        /// </summary>
        public static ObservableCollection<ReservationService> FindReservationServices(int reservationID)
        {
            ObservableCollection<ReservationService> services = new ObservableCollection<ReservationService>();

            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                MySqlCommand findServices = new MySqlCommand("SELECT * FROM laskurivi WHERE laskuid IN (SELECT laskuid FROM lasku WHERE varausid=@ID AND mitatoitu=0);", conn);
                findServices.Parameters.AddWithValue("@ID", reservationID);

                var reader = findServices.ExecuteReader();

                while (reader.Read())
                {
                    ReservationService s = new ReservationService();

                    int id = 0;
                    using (MySqlConnection c2 = new MySqlConnection(Repository.connectionDB))
                    {
                        c2.Open();
                        MySqlCommand findID = new MySqlCommand("SELECT PalveluID, ToimipisteID FROM palvelut WHERE Nimi=@name", c2);
                        findID.Parameters.AddWithValue("@name", reader.GetString("Tuotenimi"));
                        var r = findID.ExecuteReader();
                        if(r.Read())
                        {
                            id = r.GetInt32("PalveluID");
                        }
                        r.Close();
                    }

                    s.ServiceID = id;
                    //s.OfficeID = reader.GetInt32("ToimipisteID");
                    s.Name = reader.GetString("Tuotenimi");
                    s.Amount = reader.GetInt32("Maara");
                    s.Price = reader.GetDecimal("Hinta") / s.amount; // Yksittäishinta
                    s.TotalPrice = reader.GetDecimal("Hinta");

                    services.Add(s);
                }
            }

            return services;
        }
    } 
    
}
