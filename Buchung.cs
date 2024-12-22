using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VWA
{
   public class Buchung
    {
        public int ID { get; set; }
        public int BenutzerID { get; set; }
        public int AusruestungID { get; set; }
        public DateTime Buchungsbeginn {  get; set; }
		public DateTime Buchungsende { get; set; }
        public DateTime Erstellungsdatum_der_Buchung { get; set; }
        public DateTime Geaendert_am {  get; set; }
        public string Geaendert_von {  get; set; }


	}
}
