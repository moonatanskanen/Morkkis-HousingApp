using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace HousingApp.Housing
{
    /// <summary>
    /// Varauskalenteri
    /// </summary>
    public class ReservationCalendar : INotifyPropertyChanged
    {
        // Varattu väri
        public static Color ReservedColor = Color.FromArgb(74, 255, 0, 0);

        private Color selectedc = Color.FromArgb(74, 0, 150, 0);
        public Color SelectedColor
        {
            get
            {
                if(!ReservedSelect) return Color.FromArgb(74, 0, 150, 0);
                if(ReservedSelect) return Color.FromArgb(74, 0, 0, 150);

                return selectedc;
            }
            set
            {
                selectedc = value;
            }
        }

        // Vapaana väri
        public static Color AvailableColor = Color.FromArgb(74, 200, 200, 200);

        // Onko aikavalinta varattu
        public bool ReservedSelect { get; set; } = false;

        // Alku ja loppupäivä
        public DateOnly? StartDate { get; set; } = null;
        public DateOnly? EndDate { get; set; } = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Valittu kalenterikuukausi
        public enum Month
        {
            Tammikuu = 1,
            Helmikuu,
            Maaliskuu,
            Huhtikuu,
            Toukokuu,
            Kesäkuu,
            Heinäkuu,
            Elokuu,
            Syyskuu,
            Lokakuu,
            Marraskuu,
            Joulukuu,
        }

        private Month _month;
        public Month ChosenMonth
        {
            get
            {
                return _month;
            }
            set
            {

                if ((int)value > 12)
                {
                    value = Month.Tammikuu;
                    ChosenYear += 1;
                }
                else if ((int)value < 1)
                {
                    value = Month.Joulukuu;
                    ChosenYear -= 1;
                }

                _month = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateString"));
                UpdateDayButtonVisibility();
                UpdateButtonColors();
            }
        }

        // Valittu vuosi
        public int ChosenYear = 2025;

        private string datestring = "";
        public string DateString {
            get
            {
                return ChosenMonth.ToString() + " " + ChosenYear;
            }
            set
            {
                DateString = ChosenMonth.ToString() + " " + ChosenYear;
            } 
        }

        public ObservableCollection<Button> Days { get; set; }
        public Grid CalendarGrid { get; set; }


        /// <summary>
        /// Näytä kalenterin nappulat vain valitussa kuukaudessa oleville päiville
        /// </summary>
        public void UpdateDayButtonVisibility()
        {
            if (Days == null) return;

            int daysInMonth = DateTime.DaysInMonth(ChosenYear, (int)ChosenMonth);

            for(int i = 0; i <= 30; i++)
            {
                if (i+1 > daysInMonth)
                {
                    Days[i].Visibility = System.Windows.Visibility.Hidden;
                }
                else
                {
                    Days[i].Visibility = System.Windows.Visibility.Visible;
                }
            }
        }



        /// <summary>
        /// Päivitä nappuloiden värit varauksien ja valinnan mukaan
        /// </summary>
        public void UpdateButtonColors()
        {
            var res = Reservation.FindAllCabinReservations();
            if (res == null) return;
            if (Days == null) return;

            foreach (Button button in Days)
            {
                button.Background = new SolidColorBrush(AvailableColor);
            }

            foreach (Reservation r in res)
            {
                int sMonth = r.StartDate.Month;
                int sYear = r.StartDate.Year;
                int eMonth = r.EndDate.Month;
                int eYear = r.EndDate.Year;
                int start = r.StartDate.Day;
                int end = r.EndDate.Day;
                int months = ((eYear - sYear) * 12) + (eMonth - sMonth);

                // Kuukausia enemmän kuin yksi.
                if (months > 0)
                {
                    int chosenMonth = (int)ChosenMonth;
                    int chosenYear = ChosenYear;

                    if (chosenMonth == sMonth && chosenYear == sYear)
                    {
                        // Ensimmäinen kuukausi. Valitse aloituspäivästä kuun loppuun.
                        int daysInMonth = DateTime.DaysInMonth(sYear, (int)sMonth);
                        for (int j = start - 1; j <= daysInMonth - 1; j++)
                        {
                            Days[j].Background = new SolidColorBrush(ReservedColor);
                        }
                    }
                    else if (chosenMonth == eMonth && chosenYear == eYear)
                    {
                        // Viimeinen kuukausi. Valitse kuun alusta EndDateen.
                        for (int j = 0; j <= end - 1; j++)
                        {
                            Days[j].Background = new SolidColorBrush(ReservedColor);
                        }
                    }
                    else
                    {
                        //if (chosenMonth > StartDate.Value.Month && chosenMonth < EndDate.Value.Month && (chosenYear == StartDate.Value.Year || chosenYear == EndDate.Value.Year)
                        int daysInMonth = DateTime.DaysInMonth(chosenYear, chosenMonth);
                        if (sYear == eYear)
                        {
                            if (chosenMonth > sMonth && chosenMonth < eMonth && chosenYear == sYear)
                            {
                                for (int j = 0; j <= daysInMonth - 1; j++)
                                {
                                    Days[j].Background = new SolidColorBrush(ReservedColor);
                                }
                            }
                        }
                        else if (sYear != eYear)
                        {
                            // Viimeinen vuosi. Ei viimeinen kuukausi, täytä kaikki.
                            if (chosenMonth < eMonth && chosenYear == eYear)
                            {
                                for (int j = 0; j <= daysInMonth - 1; j++)
                                {
                                    Days[j].Background = new SolidColorBrush(ReservedColor);
                                }
                            }
                            // Ensimmäinen vuosi. Enemmän kuin aloituskuukausi, täytä kaikki
                            else if (chosenMonth > sMonth && chosenYear == sYear)
                            {
                                for (int j = 0; j <= daysInMonth - 1; j++)
                                {
                                    Days[j].Background = new SolidColorBrush(ReservedColor);
                                }
                            }
                            // Välivuosi. Täytä kaikki
                            else if (chosenYear > sYear && chosenYear < eYear)
                            {
                                for (int j = 0; j <= daysInMonth - 1; j++)
                                {
                                    Days[j].Background = new SolidColorBrush(ReservedColor);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if ((int)ChosenMonth == sMonth && ChosenYear == sYear)
                    {
                        for (int i = start - 1; i <= end - 1; i++)
                        {
                            Days[i].Background = new SolidColorBrush(ReservedColor);
                        }
                    }
                }
            }



            if (StartDate != null && EndDate != null)
            {
                int months = ((EndDate.Value.Year - StartDate.Value.Year) * 12) + (EndDate.Value.Month - StartDate.Value.Month);

                // Kuukausia enemmän kuin yksi.
                if (months > 0)
                {
                    int chosenMonth = (int)ChosenMonth;
                    int chosenYear = ChosenYear;

                    if (chosenMonth == StartDate.Value.Month && chosenYear == StartDate.Value.Year)
                    {
                        // Ensimmäinen kuukausi. Valitse aloituspäivästä kuun loppuun.

                        int daysInMonth = DateTime.DaysInMonth(StartDate.Value.Year, (int)StartDate.Value.Month);
                        for (int j = StartDate.Value.Day - 1; j <= daysInMonth - 1; j++)
                        {
                            Days[j].Background = new SolidColorBrush(SelectedColor);
                        }
                    }
                    else if (chosenMonth == EndDate.Value.Month && chosenYear == EndDate.Value.Year)
                    {
                        // Viimeinen kuukausi. Valitse kuun alusta EndDateen.
                        for (int j = 0; j <= EndDate.Value.Day - 1; j++)
                        {
                            Days[j].Background = new SolidColorBrush(SelectedColor);
                        }
                    }
                    else 
                    {
                        //if (chosenMonth > StartDate.Value.Month && chosenMonth < EndDate.Value.Month && (chosenYear == StartDate.Value.Year || chosenYear == EndDate.Value.Year)
                        int daysInMonth = DateTime.DaysInMonth(chosenYear, chosenMonth);
                        if (StartDate.Value.Year == EndDate.Value.Year)
                        {
                            if (chosenMonth > StartDate.Value.Month && chosenMonth < EndDate.Value.Month && chosenYear == StartDate.Value.Year) 
                            {
                                for (int j = 0; j <= daysInMonth - 1; j++)
                                {
                                    Days[j].Background = new SolidColorBrush(SelectedColor);
                                }
                            }
                        }
                        else if(StartDate.Value.Year != EndDate.Value.Year)
                        {
                            // Viimeinen vuosi. Ei viimeinen kuukausi, täytä kaikki.
                            if(chosenMonth < EndDate.Value.Month && chosenYear == EndDate.Value.Year)
                            {
                                for (int j = 0; j <= daysInMonth - 1; j++)
                                {
                                    Days[j].Background = new SolidColorBrush(SelectedColor);
                                }
                            }
                            // Ensimmäinen vuosi. Enemmän kuin aloituskuukausi, täytä kaikki
                            else if(chosenMonth > StartDate.Value.Month && chosenYear == StartDate.Value.Year)
                            {
                                for (int j = 0; j <= daysInMonth - 1; j++)
                                {
                                    Days[j].Background = new SolidColorBrush(SelectedColor);
                                }
                            }
                            // Välivuosi. Täytä kaikki
                            else if(chosenYear > StartDate.Value.Year && chosenYear < EndDate.Value.Year)
                            {
                                for (int j = 0; j <= daysInMonth - 1; j++)
                                {
                                    Days[j].Background = new SolidColorBrush(SelectedColor);
                                }   
                            }
                        }
                    }
                }
                else
                {
                    if ((int)ChosenMonth == StartDate.Value.Month && ChosenYear == StartDate.Value.Year)
                    {
                        for (int i = StartDate.Value.Day - 1; i <= EndDate.Value.Day - 1; i++)
                        {
                            Days[i].Background = new SolidColorBrush(SelectedColor);
                        }
                    }
                }
            }
        }
    }
}
