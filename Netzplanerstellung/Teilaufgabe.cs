using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Netzplanerstellung
{
    internal class Teilaufgabe
    {

        private string vorgang;
        private string beschreibung;
        private int dauer;
        private int fruehesterAnfangsZeitpunkt;
        private int fruehesterEndZeitpunkt;
        private int gesamtPuffer;
        private int freierPuffer;
        private int spaetesterAnfangsZeitpunkt;
        private int spaetesterEndZeitpunkt;

        public List<Teilaufgabe> vorgaenger = new List<Teilaufgabe>();


        public string Vorgang { get { return vorgang; } set { vorgang = value;} }
        public string Beschreibung { get { return beschreibung; } set { beschreibung = value; } }
        public int Dauer { get { return dauer; } set { dauer = value; } }
        public int FAZ { get { return fruehesterAnfangsZeitpunkt; } set { fruehesterAnfangsZeitpunkt = value; } }
        public int FEZ { get { return fruehesterEndZeitpunkt; } set { fruehesterEndZeitpunkt = value; } }
        public int GP { get { return gesamtPuffer; } set { gesamtPuffer = value; } }
        public int FP { get { return freierPuffer; } set { freierPuffer = value; } }
        public int SAZ { get { return spaetesterAnfangsZeitpunkt; } set { spaetesterAnfangsZeitpunkt = value; } }
        public int SEZ { get { return spaetesterEndZeitpunkt; } set { spaetesterEndZeitpunkt = value; } }

        

        

    }
}
