using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VWA
{
    public class Benutzer
    {
        public int ID { get; set; }
        public string Nachname { get; set; }
        public string Vorname { get; set; }
        public string Email { get; set; }
        public string Benutzername {get; set;}
        public string Geaendert_Von {get; set;}
        public DateTime Geaendert_Am {  get; set;}
     }
}
