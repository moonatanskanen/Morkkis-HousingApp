using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingApp.Services
{
    public class ServiceClass
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public decimal ServicePrice { get; set; }


        //Tarkistaa onko palvelu jo olemassa tietokannassa
        public bool IsExisting
        {
            get
            {
                return ServiceID != 0;
            }
        }
    }
}
