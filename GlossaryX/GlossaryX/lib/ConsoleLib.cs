using System;
using System.Collections.Generic;
using System.Globalization;


namespace GlossaryX.lib
{
    class ConsoleRepresentation: DateBase
    {
        protected ConsoleRepresentation (string connectString):base(connectString)
        {

        }

        public static void BackToMenuMessage()
        {
            Console.WriteLine("\n\nPress Enter to get back to menu...");
            Console.ReadLine();
            Console.Clear();
        }

        public static int ReadChoise(int lhs, int rhs)
        {
            string StringChoise = Console.ReadLine();
            int IntChoise = 0;
            try
            {
                IntChoise = Convert.ToInt32(StringChoise);
                if (IntChoise >= lhs && IntChoise <= rhs)
                {
                    return IntChoise;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return IntChoise;
            }
        }

        public static bool IsTrueInput(string input, int length)
        {
            if (input.Length > length || input.Length < 2)
            {
                Console.WriteLine("Too long or too short input.");
                return false;
            }
            else
            {
                return true;
            }

        }

        public static string CellParser(string cell, int length, char filling)
        {//converts string view of cell into the inputed length filling free space with char filling
            if (cell.Length < length)
            {
                int spacesNumber = length - cell.Length;
                for (int i = 0; i < spacesNumber; i++)
                {
                    cell += filling;
                }
            }
            return cell;
        }

        public static void ShowWordByQueryResult(IEnumerable<Meanings> query, int PresentOption)
        {//show in console information from IEnumerable Meanings in proper format
            switch (PresentOption)
            {
                case 1:
                    foreach (var word in query)
                    {
                        Console.WriteLine("{0} \t{1} \t{2}", CellParser(word.Word, 30, '.'), CellParser(word.Meaning, 100, '.'), CellParser(word.Category, 30, '.'));
                    }
                    break;
                case 2:
                    foreach (var word in query)
                    {
                        Console.WriteLine("{0} \t{1} \t{2} ", CellParser(word.Word, 30, '.'), CellParser(word.Meaning, 100, '.'), word.EditDate.ToString("g", DateTimeFormatInfo.InvariantInfo));
                    }
                    break;
                case 3:
                    foreach (var word in query)
                    {
                        Console.WriteLine("{0} \t{1} \t{2} \t{3} ", CellParser(word.Word, 30, '.'), CellParser(word.Meaning, 100, '.'), CellParser(word.Category, 30, '.'), word.EditDate.ToString("g", DateTimeFormatInfo.InvariantInfo));
                    }
                    break;
            }
        }

        public static List<string> ReadMarkers(int numberOfMarkers)
        {//read inputed number of markers 
            List<string> markers = new List<string>();
            for (int i = 0; i < numberOfMarkers; i++)
            {
                Marker://списк маркеров вернуть 
                Console.WriteLine("Marker {0}: ", (i + 1));
                markers.Add(Convert.ToString(Console.ReadLine()));
                if (!IsTrueInput(markers[i], 30))
                {
                    Console.WriteLine("Wrong input.\nReset marker.");
                    goto Marker;
                }
            }
            return markers;
        }

    }
}
