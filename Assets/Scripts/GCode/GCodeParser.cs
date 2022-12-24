using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class GCodeParser
{
    public List<GCodeLine> Lines { get; private set; }

    private GCodeParser()
    {
        Lines = new List<GCodeLine>();
    }

    public static GCodeParser ParseString(string text)
    {
        string[] lines = text.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        var parser = new GCodeParser();
        foreach (string line in lines)
        {
            try
            {
                var parsedLine = new GCodeLine(parser.Lines.LastOrDefault(), line);
                parser.Lines.Add(parsedLine);
            }
            catch (GCodeLine.EmptyLineException)
            {

            }
        }

        return parser;
    }

    public static GCodeParser ParseString(Stream stream)
    {
        var parser = new GCodeParser();
        using var reader = new StreamReader(stream);
        while(!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            try
            {
                var parsedLine = new GCodeLine(parser.Lines.LastOrDefault(), line);
                parser.Lines.Add(parsedLine);
            }
            catch (GCodeLine.EmptyLineException)
            {

            }
        }

        return parser;
    }
}