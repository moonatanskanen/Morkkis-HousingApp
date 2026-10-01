using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingApp.Invoices
{
    //laskuluokka
    public class Invoice : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        //kaikki laskuun tulevat tiedot
        public int LaskuID { get; set; }
        public int AsiakasID { get; set; }
        public int VarausID { get; set; }

        public DateOnly Paivamaara { get; set; }
        public DateOnly Erapaiva { get; set; }

        public string AOsoite { get; set; }
        public string APuhelin { get; set; }
        public string ASahkoposti { get; set; }
        public string AEtunimi { get; set; }
        public string ASukunimi { get; set; }

        public string Toimipistenimi { get; set; }
        public string MokkiNimi { get; set; }

        private bool mitatoity;
        InvoiceSQL invoiceSQL = new InvoiceSQL();
        public bool Mitatoitu
        {

            get
            {
                return mitatoity;

            }
            set
            {
                mitatoity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Mitatoitu"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LopullinenHinta"));
            }
        }

        public ObservableCollection<InvoiceRow> Laskurivit { get; set; }

        private string lopullinenhinta;
        public string LopullinenHinta
        {
            get
            {
                if(Mitatoitu==true)
                {
                    return "Lasku mitätöity";

                }
                if (Laskurivit == null)
                {
                    return "0";
                }
                //lasketaan laskun lopullinen hinta
                float yht = 0;
                foreach (var item in Laskurivit)
                {
                    //käydään kaikkien laskurivien hinnat läpi ja lasketaan ne yhteen
                    yht += item.Hinta;
                }
                return MathF.Round(yht, 2).ToString() + " €";
            }

            set //jos lasku on mitätöity, lopullista hintaa ei ole
            {
                if (Mitatoitu == true)
                {
                    lopullinenhinta = "Lasku mitätöity";
                    return;
                }
                lopullinenhinta = value;
            }
        }
    }

    public class InvoiceRow //laskurivi
    {
        public int LaskuriviID { get; set; }
        public string Tuotenimi { get; set; } //palvelu
        public float Hinta { get; set; }
        public int Maara { get; set; }
        public int Laskutunnus { get; set; }

    }
}
