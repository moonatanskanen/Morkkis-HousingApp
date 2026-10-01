using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingApp.Customer
{
    internal class Customer
    {
        public string Etunimi { get; set; }
        public string Sukunimi { get; set; }
        public string Sahkoposti { get; set; }
        public string Osoite { get; set; }
        public string Puhelin { get; set; }

        public string FullName => $"{Etunimi} {Sukunimi}";

    }
}
