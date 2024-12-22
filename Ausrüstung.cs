using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VWA
{
   public class Ausruestung
    {
        public int ID { get; set; } 
        public string? ArtDerAusruestung { get; set; }
		public string? Marke { get; set; }
		public string? Model { get; set; }
		public string? Zustand { get; set; }
		public byte[] Foto { get; set; }
	}
}
