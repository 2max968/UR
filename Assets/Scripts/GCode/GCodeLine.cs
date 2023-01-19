using System.Globalization;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GCodeLine
{
    private static readonly char[] whitespaces = new[] { ' ', '\t' };

    public string Line { get; private set; }
    public int GCodeNum { get; private set; }
    public char GCodeChar { get; private set; }
    public Dictionary<char, double> Parameters { get; private set; }
    public bool IsMovement { get; private set; } = false;
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Z { get; private set; }
    public double FeedRate { get; private set; }
    public double Extrusion { get; private set; }
    public bool RelativeMovement { get; private set; }
    public bool RelativeE { get; private set; }
    public double HotendTemperature { get; private set; }
    public double BedTemperature { get; private set; }
    public bool WaitForHotendTemperature { get; private set; }
    public bool WaitForBedTemperatur { get; private set; }
    public double Unit { get; private set; } = 1;
    /// <summary>
    /// Fan Speed, Range: [0, 1]
    /// </summary>
    public double FanSpeed { get; private set; } = 0;

    public GCodeLine(GCodeLine? lastLine, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new EmptyLineException();

        Line = text;
        X = lastLine?.X ?? 0;
        Y = lastLine?.Y ?? 0;
        Z = lastLine?.Z ?? 0;
        FeedRate = lastLine?.FeedRate ?? 0;
        Extrusion = 0;
        RelativeMovement = lastLine?.RelativeMovement ?? false;
        RelativeE = lastLine?.RelativeE ?? false;
        HotendTemperature = lastLine?.HotendTemperature ?? 0;
        BedTemperature = lastLine?.BedTemperature ?? 0;
        WaitForHotendTemperature = false;
        WaitForBedTemperatur = false;
        Parameters = new Dictionary<char, double>();

        int commentStart = text.IndexOf(';');
        if (commentStart >= 0)
            text = text.Substring(0, commentStart);
        text = text.Trim();
        if (string.IsNullOrWhiteSpace(text))
            throw new EmptyLineException();

        GCodeChar = text[0];
        int gIndEnd = text.IndexOfAny(whitespaces);
        if (gIndEnd < 0) gIndEnd = text.Length;
        try
        {
            GCodeNum = byte.Parse(text.Substring(1, gIndEnd - 1), CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            Debug.Break();
        }

        string[] parameterStrings = text.Substring(gIndEnd)
            .Split(whitespaces, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parameterStrings)
        {
            Parameters.Add(p[0], double.Parse(p.Substring(1), CultureInfo.InvariantCulture));
        }

        if (GCodeChar == 'G')
        {
            if (GCodeNum == 0 || GCodeNum == 1) // Linear Movement
            {
                IsMovement = true;
                if (RelativeMovement)
                {
                    if (Parameters.ContainsKey('X')) X += Parameters['X'] * Unit;
                    if (Parameters.ContainsKey('Y')) Y += Parameters['Y'] * Unit;
                    if (Parameters.ContainsKey('Z')) Z += Parameters['Z'] * Unit;
                }
                else
                {
                    if (Parameters.ContainsKey('X')) X = Parameters['X'] * Unit;
                    if (Parameters.ContainsKey('Y')) Y = Parameters['Y'] * Unit;
                    if (Parameters.ContainsKey('Z')) Z = Parameters['Z'] * Unit;
                }

                if (Parameters.ContainsKey('E'))
                {
                    if (RelativeE) Extrusion += Parameters['E'];
                    else Extrusion = Parameters['E'];
                }

                if (Parameters.ContainsKey('F')) FeedRate = Parameters['F'];
            }
            else if (GCodeNum == 28) // Homing
            {
                if (Parameters.ContainsKey('X')) X = 0;
                if (Parameters.ContainsKey('Y')) Y = 0;
                if (Parameters.ContainsKey('Z')) Z = 0;
                if (!Parameters.ContainsKey('X') && !Parameters.ContainsKey('Y') && !Parameters.ContainsKey('Z'))
                    X = Y = Z = 0;
            }
            else if (GCodeNum == 90) // Absolute Movement
            {
                RelativeMovement = false;
                RelativeE = false;
            }
            else if (GCodeNum == 91) // Relative Movement
            {
                RelativeMovement = true;
                RelativeE = true;
            }
            else if (GCodeNum == 21) // Set unit to millimeters
            {
                Unit = 1;
            }
            else if (GCodeNum == 20) // Set unit to inches
            {
                Unit = 25.4;
            }
            else if (GCodeNum == 92) // Set current position
            {
                // Warning: ignored
            }
            else
            {
                Debug.LogWarning($"Unknown G-Step: {Line}");
                //throw new Exception($"Unknown G-Step: {Line}");
            }
        }
        else if (GCodeChar == 'M')
        {
            if (GCodeNum == 104) // Set Hotend Temperature, don't wait
            {
                HotendTemperature = Parameters['S'];
            }
            else if (GCodeNum == 109) // Set Hotend Temperature, wait
            {
                HotendTemperature = Parameters['S'];
                WaitForHotendTemperature = true;
            }
            else if (GCodeNum == 140) // Set Bed Temperature, don't wait
            {
                BedTemperature = Parameters['S'];
            }
            else if (GCodeNum == 190) // Set Bed Temperature, wait
            {
                BedTemperature = Parameters['S'];
                WaitForBedTemperatur = true;
            }
            else if (GCodeNum == 106)
            {
                if (Parameters.ContainsKey('S'))
                    FanSpeed = Parameters['S'] / 255.0;
                else
                    FanSpeed = 1;
            }
            else if (GCodeNum == 107)
            {
                FanSpeed = 0;
            }
            else if (GCodeNum == 82) // E Absolute
            {
                RelativeE = false;
            }
            else if (GCodeNum == 83) // E Relative
            {
                RelativeE = true;
            }
            else if (GCodeNum == 18 || GCodeNum == 84) // Disable Motors
            {
                // Ignore
            }
            else if (GCodeNum == 204) // Set Starting Speed
            {
                // Ignore
            }
            else
            {
                Debug.LogWarning($"Unknown M-Step: {Line}");
                //throw new Exception($"Unknown M-Step: {Line}");
            }
        }
        else if(GCodeChar == 'T')
        {
            // Ignore
        }
        else
        {
            Debug.LogWarning($"Unknown Step-Type: {Line}");
            throw new Exception($"Unknown Step-Type: {Line}");
        }
    }

    public override string ToString()
    {
        return $"X: {X}, Y:{Y}, Z:{Z}, E:{Extrusion}, F:{FeedRate}, Hotend:{HotendTemperature}�C, Bed:{BedTemperature}�C";
    }

    public class EmptyLineException : Exception
    {
        public EmptyLineException() : base("The line is empty")
        {
        }
    }
}