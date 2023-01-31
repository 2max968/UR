using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class GCodeParser
{
    // Liste mit den geparsten GCode-Befehlen
    public List<GCodeLine> Lines { get; private set; }

    private GCodeParser()
    {
        Lines = new List<GCodeLine>();
    }

    // Funktion, welche eine G-Code Datei in Textform parst
    public static GCodeParser ParseString(string text)
    {
        // Text in Zeilen aufteilen, indem der Text an den Zeilenumbrüchen getrennt wird
        string[] lines = text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        // Neues Parser-Objekt erstellen
        var parser = new GCodeParser();
        
        foreach (string line in lines)
        {
            try
            {
                // Aktuelle Zeile parsen und zur Liste hinzufügen
                // Das tatsächliche Parsen der Zeile findet im Konstruktor von GCodeLine statt
                var parsedLine = new GCodeLine(parser.Lines.LastOrDefault(), line);
                parser.Lines.Add(parsedLine);
            }
            catch (GCodeLine.EmptyLineException)
            {
                // Fehler wärend des Parsens der Zeile ignorieren
            }
        }

        return parser;
    }

    // Funktion, welche eine G-Code Datei in Form eines Datenstroms parst
    public static GCodeParser ParseString(Stream stream)
    {
        // Neues Parser-Objekt erstellen
        var parser = new GCodeParser();
        // Neuen StreamReader erstellen, welcher den Datenstrom Zeilenweise lesen kann
        using var reader = new StreamReader(stream);
        // Den Datenstrom zeilenweise lesen, bis er zuende ist
        while(!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            try
            {
                // Aktuelle Zeile parsen und zur Liste hinzufügen
                // Das tatsächliche Parsen der Zeile findet im Konstruktor von GCodeLine statt
                var parsedLine = new GCodeLine(parser.Lines.LastOrDefault(), line);
                parser.Lines.Add(parsedLine);
            }
            catch (GCodeLine.EmptyLineException)
            {
                // Fehler wärend des Parsens der Zeile ignorieren
            }
        }

        return parser;
    }
}