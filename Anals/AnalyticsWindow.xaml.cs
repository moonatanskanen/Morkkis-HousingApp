using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
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
    /// Interaction logic for AnalyticsWindow.xaml
    /// </summary>
    public partial class AnalyticsWindow : Window
    {
        ObservableCollection<Infos> infos = new ObservableCollection<Infos>();


        public AnalyticsWindow()
        {
            InitializeComponent();

        }

        /// <summary>
        /// hakee kaikki tiedot tietyltä toimipisteeltä 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchAllInfo_Click(object sender, RoutedEventArgs e)
        {
            infos.Clear();

            // täyttää gridin
            InfosGrid.ItemsSource = GetAnalyticsInfo(MorkkisWindow.OfficeID);


            // laskee varattujen mökkien määrän
            cabinstxt.Text = infos.Count().ToString();

            // laskee palveluiden määrän  
            int count = 0;


            for (int i = 0; i < infos.Count; i++)
            {
                // Luodaan kaikista palveluista string-lista ja poistetaan whitespacet.
                string[] service = infos[i].Service.Split(',')
                                                   .Select(s => s.Trim())
                                                   .ToArray();
                string final = "Ei palveluita";

                // Loopataan kaikkien palveluiden läpi
                for (int s = 0; s <= service.Length - 1; s++)
                {
                    // Jos palvelun nimi ei ole vuokra, niin lisätään se listaan.
                    if (service[s] != "Vuokra")
                    {
                        if (final == "Ei palveluita") final = string.Empty;
                        final += service[s];

                        // Viimeisellä kierrolla ei pilkkua, eikä myöskään jos seuraava on viimeinen ja vuokra!
                        if (s == service.Length - 1 || (s == service.Length - 2 && service[s + 1] == "Vuokra"))
                        {
                            continue;
                        }

                        // Lisätään pilkku palveluiden väliin
                        final += ", ";
                    }
                }

                // Ei lasketa palveluiden määrää jos niitä ei ole.
                if (final != "Ei palveluita")
                {
                    // Lasketaan lopullinen määrä palveluita
                    count += final.Split(",").Count();
                }

                // Varmistetaan että service tekstistä on poistettu vuokra
                infos[i].Service = final;
            }


            servicestxt.Text = count.ToString();

        }

        /// <summary>
        /// hakee varaukset tietyltä aikaväliltä 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchSelectedInfo_Click(object sender, RoutedEventArgs e)
        {
            infos.Clear();

            if (StartingDate.SelectedDate != null && EndingDate.SelectedDate != null)
            {
                DateTime start = StartingDate.SelectedDate.Value;
                DateTime end = EndingDate.SelectedDate.Value;

                if (start > end)
                {
                    MessageBox.Show("Alkupäivä ei voi olla loppupäivän jälkeen.");
                    return;
                }

                // täyttää gridin
                InfosGrid.ItemsSource = GetAnalyticsInfoDates(MorkkisWindow.OfficeID, start, end);

                // laskee varattujen mökkien määrän
                cabinstxt.Text = infos.Count().ToString();

                // laskee palveluiden määrän  
                int count = 0;

                for (int i = 0; i < infos.Count; i++)
                {
                    // Luodaan kaikista palveluista string-lista ja poistetaan whitespacet.
                    string[] service = infos[i].Service.Split(',')
                                                       .Select(s => s.Trim())
                                                       .ToArray();
                    string final = "Ei palveluita";

                    // Loopataan kaikkien palveluiden läpi
                    for(int s = 0; s <=service.Length-1; s++)
                    {
                        // Jos palvelun nimi ei ole vuokra, niin lisätään se listaan.
                        if (service[s] != "Vuokra")
                        {
                            if (final == "Ei palveluita") final = string.Empty;
                            final += service[s];

                            // Viimeisellä kierrolla ei pilkkua, eikä myöskään jos seuraava on viimeinen ja vuokra!
                            if(s == service.Length - 1 || (s == service.Length - 2 && service[s+1] == "Vuokra"))
                            {
                                continue;
                            }

                            // Lisätään pilkku palveluiden väliin
                            final += ", ";
                        }
                    }

                    // Ei lasketa palveluiden määrää jos niitä ei ole.
                    if (final != "Ei palveluita")
                    {
                        // Lasketaan lopullinen määrä palveluita
                        count += final.Split(",").Count();
                    }

                    // Varmistetaan että service tekstistä on poistettu vuokra
                    infos[i].Service = final;
                }

                servicestxt.Text = count.ToString();
            }
            else
            {

                MessageBox.Show("Valitse molemmat päivämäärät!");
            }
        }

        /// <summary>
        /// hakee varausten tiedot tietokannasta, mökin nimi, alkupäivämäärä, loppupäivämäärä, palvelut
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ObservableCollection<Infos> GetAnalyticsInfo(int ID)
        {

            // SQL kysely
            string sql = @"SELECT l.mokki, v.alkupaivamaara, v.loppupaivamaara, 
                          IFNULL(GROUP_CONCAT(lr.tuotenimi SEPARATOR ', '), 'Ei palveluja') AS ServiceName
                          FROM lasku l, Varaus v, laskurivi lr, mokki m
                          WHERE l.laskuID = lr.laskuID
                          AND l.varausID = v.varausID 
                          AND m.mokkiid = v.mokkiid 
                          AND m.toimipisteID = @ID
                          AND l.mitatoitu = 0
                          GROUP BY m.nimi, v.alkupaivamaara, v.loppupaivamaara
                          ORDER BY v.alkupaivamaara";



            // avataan tietokantayhteys
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                // komento, joka sisältää SQL-kyselyn
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                // lisätään parametrina tuleva toimipisteID
                cmd.Parameters.AddWithValue("@ID", ID);

                // suorittaa kyselyn
                var dr = cmd.ExecuteReader();

                // käy tiedot läpi ja täyttää kokoelman 
                while (dr.Read())
                {

                    Infos info = new Infos();
                    info.CabinName = dr.GetString("mokki");
                    info.StartingDate = dr.GetDateTime("alkupaivamaara");
                    info.EndingDate = dr.GetDateTime("loppupaivamaara");
                    info.Service = dr.GetString("ServiceName");

                    // lisää tiedot kokoelmaan
                    infos.Add(info);

                }
            }

            return infos;
        }

        /// <summary>
        /// hakee varausten tiedot tietokannasta päivämäärien perusteella, mökin nimi, alkupäivämäärä, loppupäivämäärä, palvelut
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="startingDate"></param>
        /// <param name="EndingDate"></param>
        /// <returns></returns>
        public ObservableCollection<Infos> GetAnalyticsInfoDates(int ID, DateTime startingDate, DateTime EndingDate)
        {


            // SQL kysely
            string sql = @"SELECT l.mokki, v.alkupaivamaara, v.loppupaivamaara, 
                          GROUP_CONCAT(lr.tuotenimi SEPARATOR ', ') AS ServiceName
                          FROM lasku l, Varaus v, laskurivi lr, mokki m
                          WHERE l.laskuID = lr.laskuID
                          AND l.varausID = v.varausID 
                          AND m.mokkiid = v.mokkiid 
                          AND m.toimipisteID = @ID
                          AND v.alkupaivamaara >= @alkupaivamaara
                          AND v.loppupaivamaara <= @loppupaivamaara
                          AND l.mitatoitu = 0
                          GROUP BY m.nimi, v.alkupaivamaara, v.loppupaivamaara
                          ORDER BY v.alkupaivamaara"; 
            
                    //@"SELECT m.nimi, v.alkupaivamaara, v.loppupaivamaara,
                    //              GROUP_CONCAT(p.nimi SEPARATOR ', ') AS ServiceName
                    //              FROM Mokki m, Varaus v, varauksen_palvelut vp, palvelut p
                    //              WHERE p.palveluID = vp.palveluID
                    //              AND vp.varausID = v.varausID 
                    //              AND m.mokkiid = v.mokkiid 
                    //              AND m.toimipisteID = @ID
                    //              AND v.alkupaivamaara >= @alkupaivamaara
                    //              AND v.loppupaivamaara <= @loppupaivamaara
                    //              GROUP BY m.nimi, v.alkupaivamaara, v.loppupaivamaara
                    //              ORDER BY v.alkupaivamaara";


            // avataan tietokantayhteys
            using (MySqlConnection conn = new MySqlConnection(Repository.connectionDB))
            {
                conn.Open();

                // komento, joka sisältää SQL-kyselyn
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                // lisätään parametrit
                cmd.Parameters.AddWithValue("@ID", ID);
                cmd.Parameters.AddWithValue("@alkupaivamaara", startingDate);
                cmd.Parameters.AddWithValue("@loppupaivamaara", EndingDate);

                // suorittaa kyselyn
                var dr = cmd.ExecuteReader();

                // käy tiedot läpi ja täyttää kokoelman 
                while (dr.Read())
                {

                    Infos info = new Infos();
                    info.CabinName = dr.GetString("mokki");
                    info.StartingDate = dr.GetDateTime("alkupaivamaara");
                    info.EndingDate = dr.GetDateTime("loppupaivamaara");
                    info.Service = dr.GetString("ServiceName");

                    // lisää tiedot kokoelmaan
                    infos.Add(info);
                }
            }
            return infos;
        }

        /// <summary>
        /// sulkee ikkunan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PoistuClick(object sender, RoutedEventArgs e)
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
        private void TiedotClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mörkkis versio 1.0\nTekijät: Moona, Aura, Elmeri, Henrik ja Joona", "Tietoa", MessageBoxButton.OK);
        }

        private void OnlyDateChar(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9.]$"))
            {
                SystemSounds.Beep.Play();
                e.Handled = true;
            }
        }
    }
}

