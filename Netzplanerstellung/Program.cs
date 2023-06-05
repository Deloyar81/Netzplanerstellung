using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using System.Drawing;
using System.Drawing.Imaging;
using Fclp;

namespace Netzplanerstellung
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //input Pfad
            string inputPfad = String.Empty;
            
            //Output Pfad
            string outputPfad = String.Empty;

            //Command Line Parser initialisieren 
            var p = new FluentCommandLineParser();

            //Command Line Parser Kommandos belegen 
            p.Setup<string>('i', "input").Callback(value => inputPfad = value).WithDescription("Option ist notwendig. Definiert den Dateipfad der Eingabedatei (Format muss CSV sein).").Required();
            p.Setup<string>('o', "output").Callback(value => outputPfad = value).WithDescription("Option ist optional. Definiert den Pfad der ausgegeben Bilddatei an (zulässige Formate: JPG, PNG, BMP). .");

            p.SetupHelp("h", "help").Callback(text => Console.WriteLine(text));

            //Argumente anwwenden
            var result = p.Parse(args);

            //Abbruch des Programmes bei Hilfeaufruf
            if(result.HelpCalled)
            {
                Environment.Exit(0);
            }

            Netzplan netzplan = new Netzplan();
            List<string> csvLines = new List<string>();

            //Datei einlesen
            try
            {
                using (StreamReader sr = new StreamReader(inputPfad, Encoding.Default))
                {
                    while (sr.EndOfStream == false)
                    {
                        //Leerzeilen in Datei übergehen
                        string line = sr.ReadLine();

                        if (line.Length > 0)
                        {
                            csvLines.Add(line);
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Die Eingabe-Datei wurde nicht gefunden! Bitte überprüfen Sie den Dateinamen!");
                Environment.Exit(0);
            }


            //Kopfzeile entfernen
            try
            {
                csvLines.RemoveAt(0);

                //Zeilen einzelnd durchgehen...
                foreach (var line in csvLines)
                {
                    string[] csvLinesSplit = line.Split(';');

                    //...einzelne Teile der Zeile von Leerzeichen bereinigen...
                    foreach (var split in csvLinesSplit)
                    {
                        split.Trim();
                    }

                    //...und in Aufgabe schreiben
                    Teilaufgabe teilaufgabe = new Teilaufgabe();

                    teilaufgabe.Vorgang = csvLinesSplit[0];
                    teilaufgabe.Beschreibung = csvLinesSplit[1]
                        .Replace("ä", "&auml;")
                        .Replace("Ä", "&Auml;")
                        .Replace("ö", "&ouml;")
                        .Replace("Ö", "&Ouml;")
                        .Replace("ü", "&uuml;")
                        .Replace("Ü", "&Uuml;")
                        .Replace("ß", "&szlig;");
                    teilaufgabe.Dauer = Convert.ToInt32(csvLinesSplit[2]);

                    string[] vorgaengerSplit = csvLinesSplit[3].Split(',');

                    foreach (var vorgang in netzplan.netzplanKomplett)
                    {
                        foreach (var vorgaenger in vorgaengerSplit)
                        {
                            vorgaenger.Trim();

                            if (vorgang.Vorgang == vorgaenger)
                            {
                                teilaufgabe.vorgaenger.Add(vorgang);
                            }
                        }
                    }

                    //Aufgabe in den kompletten Plan einordnen
                    netzplan.netzplanKomplett.Add(teilaufgabe);
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine($"Die Eingabe-Datei entspricht nicht dem nötigen Format!");
                Environment.Exit(0);
            }

            //restliche Werte berechnen
            try
            {
                netzplan.BerechneZeitpunkte();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Knoten {0} konnte nicht berechnet werden. Bitte die entsprechenden Werte in der Eingabe-Datei überprüfen.", netzplan.fehlerhafterAufagbenteil);
            }

            //Ausgabe des Netzplanes
            try
            {
                //String für die Ausgabe zusammen bauen
                string diagramm = "digraph html {";
                string pfeile = "rankdir=LR; ";

                foreach (Teilaufgabe vorgang in netzplan.netzplanKomplett)
                {
                    //Werte jeder Teilaufgabe
                    diagramm += vorgang.Vorgang + "[shape = none, margin = 0, label = < <TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\"  CELLPADDING=\"4\" >" +
                        " <TR>" +
                        " <TD COLSPAN =\"2\" >FAZ = " + vorgang.FAZ + "</TD>" +
                        " <TD COLSPAN=\"2\" > FEZ = " + vorgang.FEZ + "</TD>" +
                        " </TR>" +
                        " <TR>" +
                        " <TD>" + vorgang.Vorgang + "</TD>" +
                        " <TD COLSPAN=\"3\" >" + vorgang.Beschreibung + "</TD>" +
                        " </TR>" +
                        " <TR>" +
                        " <TD COLSPAN=\"1\" >" + vorgang.Dauer + "</TD>" +
                        " <TD COLSPAN=\"1\">GP = " + vorgang.GP + "</TD>" +
                        " <TD COLSPAN=\"2\">FP = " + vorgang.FP + "</TD>" +
                        " </TR> " +
                        " <TR>" +
                        " <TD COLSPAN=\"2\">SAZ = " + vorgang.SAZ + "</TD>" +
                        " <TD COLSPAN=\"2\">SEZ = " + vorgang.SEZ + "</TD>" +
                        " </TR>" +
                        "</TABLE >>]; ";

                    //Zuordnung der Pfeile
                    foreach (Teilaufgabe b in vorgang.vorgaenger)
                    {
                        if (vorgang.GP == 0 && b.FP == 0)
                        {
                            pfeile += b.Vorgang + "->" + vorgang.Vorgang + "[color = \"red\"] ";
                        }
                        else
                        {
                            pfeile += b.Vorgang + "->" + vorgang.Vorgang + " ";
                        }
                    }
                }

                //Pfeile einfügen
                diagramm += pfeile + "}";

                //GraphViz 
                var getStartProcessQuery = new GetStartProcessQuery();
                var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
                var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

                var wrapper = new GraphGeneration(getStartProcessQuery,
                                      getProcessStartInfoQuery,
                                      registerLayoutPluginCommand);

                byte[] output = wrapper.GenerateGraph(diagramm, Enums.GraphReturnType.Png);

                //Ausgabedatei erzeugen
                using (Image image = Image.FromStream(new MemoryStream(output)))
                {
                    
                    //Ausgabeort überprüfen                    

                    //outputpfad gleich inputpfad setzen, wenn -o nicht angegeben
                    if (outputPfad == "")
                    {
                        string[] outputPfadSplit = inputPfad.Split('.');

                        for (int i = 0; i < outputPfadSplit.Count() - 1; i++)
                        {
                            outputPfad += outputPfadSplit[i] + ".";
                        }

                        outputPfad += ".png";

                        BestimmeDateiformat(inputPfad, outputPfad, image);
                    }

                    else if(outputPfad != "")
                    {
                        string outputPfad2 = String.Empty;

                        int start = inputPfad.LastIndexOf("\\") + 1;
                        int ende = inputPfad.Count();

                        DirectoryInfo di = new DirectoryInfo(inputPfad.Remove(start, ende - start));
                        if (di.Exists)
                        {
                            //outputName gleich inputName setzen, wenn -o nur Festplatte angegeben
                            if (outputPfad.Split('.').Count() == 1)
                            {
                                

                                outputPfad += inputPfad.Substring(start, ende - start);

                                //Dateiendung ersetzen
                                ErsetzeDateiendung(outputPfad, ref outputPfad2);

                                BestimmeDateiformat(inputPfad, outputPfad2, image);
                            }
                            else
                            {
                                //Dateiendung ersetzen
                                ErsetzeDateiendung(outputPfad, ref outputPfad2);

                                BestimmeDateiformat(inputPfad, outputPfad2, image);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Der AUsgabeort existiert nicht. Überprüfen Sie die Schreibweise!");
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ausgabe-Fehler");
            }
        }
        
        static void ErsetzeDateiendung(string outputPfad, ref string outputPfad2)
        {
            string[] outputPfadSplit2 = outputPfad.Split('.');

            for (int i = 0; i < outputPfadSplit2.Count() - 1; i++)
            {
                outputPfad2 += outputPfadSplit2[i] + ".";
            }

            outputPfad2 += "png";
        }

        static void BestimmeDateiformat(string inputPfad, string outputPfad2, Image image)
        {
            string outputFormat = String.Empty;

            //Dateiformat bestimmen
            string[] formatSplit = outputPfad2.Split('.');
            string format = formatSplit[formatSplit.Count() - 1].ToLower();

            //verschiedene Dateiformate ausgeben
            if (format == "jpg")
            {
                outputFormat = "jpg";
                image.Save(outputPfad2, ImageFormat.Jpeg);
            }
            else if (format == "png")
            {
                outputFormat = "png";
                image.Save(outputPfad2, ImageFormat.Png);
            }
            else if (format == "bmp")
            {
                outputFormat = "bmp";
                image.Save(outputPfad2, ImageFormat.Bmp);
            }
            Console.WriteLine($"Die Datei {inputPfad} wurde unter {outputPfad2} abgespeichert.");
        }
    }
}
