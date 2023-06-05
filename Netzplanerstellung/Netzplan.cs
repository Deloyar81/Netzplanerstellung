using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netzplanerstellung
{
    internal class Netzplan
    {
        public List<Teilaufgabe> netzplanKomplett { get; set; }
        public string fehlerhafterAufagbenteil;

        public Netzplan()
        {
            netzplanKomplett = new List<Teilaufgabe>();
        }

        //Alle fehlendne Werte innerhalb der Teilaufgaben berechnen


        public void BerechneZeitpunkte()
        {
            foreach (var aufgabe in netzplanKomplett)
            {
                //möglichen fehlerahften Aufagebnteil benennen
                fehlerhafterAufagbenteil = aufgabe.Vorgang;

                //FAZ berechnen

                //Wenn Teilaufgabe der erste Knoten ist, setze FAZ auf 0
                if (aufgabe.vorgaenger.Count == 0)
                {
                    aufgabe.FAZ = 0;
                }

                //ansonsten auf den höchsten Endzeitpunkt der Vorgänger
                if (aufgabe.vorgaenger.Count != 0)
                {
                    aufgabe.FAZ = aufgabe.vorgaenger.Max(x => x.FEZ);
                }

                //FEZ berechnen
                aufgabe.FEZ = aufgabe.FAZ + aufgabe.Dauer;
            }

            //SEZ und SAZ berechnen
            for (int i = netzplanKomplett.Count - 1; i >= 0; i--)
            {
                //letzter Knoten berechnen
                if (netzplanKomplett.Count - 1 == i)
                {
                    netzplanKomplett[i].SEZ = netzplanKomplett[i].FEZ;
                    netzplanKomplett[i].SAZ = netzplanKomplett[i].SEZ - netzplanKomplett[i].Dauer;
                }

                //alle anderen Knoten berechnen
                foreach (var teil in netzplanKomplett[i].vorgaenger)
                {
                    teil.SEZ = netzplanKomplett[i].SAZ;
                    teil.SAZ = teil.SEZ - teil.Dauer;

                    //FP berechnen
                    teil.FP = netzplanKomplett[i].FAZ - teil.FEZ;
                }
            }

            //GP berechnen
            foreach (var aufgabe in netzplanKomplett)
            {
                aufgabe.GP = aufgabe.SAZ - aufgabe.FAZ;
            }
        }
    }
}
