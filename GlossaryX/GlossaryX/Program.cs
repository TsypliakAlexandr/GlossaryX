using System;
using GlossaryX.lib;//using two libraries stored in GlossaryX\lib
using System.Collections.Generic;

namespace GlossaryX
{
    class Program
    {
        protected enum PresentOptions
        {
            Word_Meaning_Category = 1,
            Word_Meaning_Date = 2,
            Word_Meaning_Category_Date = 3
        }

        protected enum SortOptions
        {
            byMeanings = 1,
            byDate = 2,
            byCategory = 3
        }

        static void Main(string[] args)
        {
            //glossary datebase stored in GlossaryX\bin\Debug in file Glossary.mdf
            //setting connection properties 
            //change Data Source = to your local sqlexpress server address 
            string connectString = "Data Source=DESKTOP-C1JFPHG\\SQLEXPRESS01;AttachDbFilename=" + Environment.CurrentDirectory + "\\Glossary.mdf;Initial Catalog=GlossaryXX;Integrated Security=True;Connect Timeout=10";
            //connecting stored datebase to local sqlexpress server
            DateBase Glossary = new DateBase(connectString);
            Console.WriteLine("******\nWelcome to GlossaryX 1.0\n******");
            start:
            Console.WindowHeight = Console.LargestWindowHeight;
            Console.WindowWidth = Console.LargestWindowWidth;
            Console.WriteLine("Please, choose action:");
            Console.WriteLine("1.Show all Meanings from base.");
            Console.WriteLine("2.Add a word into glossary");
            Console.WriteLine("3.Find Meanings by category/markers/word/date");
            Console.WriteLine("4.Remove/find word in glossary");
            Console.WriteLine("5.Statistics by glossary");
            Console.WriteLine("6.Delete all Meanings from glossary");
            Console.WriteLine("7.Exit");
            switch (ConsoleRepresentation.ReadChoise(1, 7))
            {
                case 0:
                    Console.Clear();
                    Console.WriteLine("Wrong choise input!");
                    ConsoleRepresentation.BackToMenuMessage();
                    goto start;
                case 1:
                    ShowMeanings(Glossary);
                    ConsoleRepresentation.BackToMenuMessage();
                    goto start;
                case 2:
                    AddMeanings(Glossary);
                    ConsoleRepresentation.BackToMenuMessage();
                    goto start;
                case 3:
                    SearchMeanings(Glossary);
                    ConsoleRepresentation.BackToMenuMessage();
                    goto start;
                case 4:
                    RemoveWord(Glossary);
                    ConsoleRepresentation.BackToMenuMessage();
                    goto start;
                case 5:

                    ShowStatistic(Glossary);
                    ConsoleRepresentation.BackToMenuMessage();
                    goto start;
                case 6:
                    RemoveGlossary(Glossary);
                    ConsoleRepresentation.BackToMenuMessage();
                    goto start;
                case 7:
                    Console.WriteLine("Press enter to quit.");
                    Console.ReadLine();
                    break;
            }
        }

        private class Word
        {
            public string word { get; set; }
            public string meaning { get; set; }
            public string category { get; set; }
            public Word()
            {
                Console.Write("Word: ");
                string word = Convert.ToString(Console.ReadLine());
                Console.Write("Meaning: ");
                string meaning = Convert.ToString(Console.ReadLine());
                Console.Write("Category: ");
                string category = Convert.ToString(Console.ReadLine());
                if (!ConsoleRepresentation.IsTrueInput(category, 30) || !ConsoleRepresentation.IsTrueInput(meaning, 100) || !ConsoleRepresentation.IsTrueInput(word, 30))
                    throw new Exception();
                this.word = word;
                this.meaning = meaning;
                this.category = category;
            }
        }

        private static void ShowMeanings(DateBase Glossary)
        {
            Console.Clear();
            Console.WriteLine("How should the Meanings be sorted:\n");
            Console.WriteLine("1.By alphabet");
            Console.WriteLine("2.By date");
            Console.WriteLine("3.By category");
            switch (ConsoleRepresentation.ReadChoise(1, 3))
            {
                case 0:
                    Console.WriteLine("Wrong input.");
                    break;
                case 1:
                    //get IEnumerable of all words from glossary sorted by first letter of words, showing them in console in format word-meaning-category
                    ConsoleRepresentation.ShowWordByQueryResult(Glossary.GetAllMeaningsSorted((int)SortOptions.byMeanings), (int)PresentOptions.Word_Meaning_Category);
                    break;
                case 2:
                    ConsoleRepresentation.ShowWordByQueryResult(Glossary.GetAllMeaningsSorted((int)SortOptions.byDate), (int)PresentOptions.Word_Meaning_Category);
                    break;
                case 3:
                    ConsoleRepresentation.ShowWordByQueryResult(Glossary.GetAllMeaningsSorted((int)SortOptions.byCategory), (int)PresentOptions.Word_Meaning_Category);
                    break;
            }
        }

        private static void AddMeanings(DateBase Glossary)
        {
            Console.Clear();
            Console.WriteLine("***Word insertion***");
            Word newWord;
            try
            {
                //reads word-meaning-category from console
                newWord = new Word();
            }
            catch
            {
                Console.WriteLine("Wrong input");
                return;
            }
            Console.WriteLine("How many markers do you want to add?\n(number of markers 0 - 3)");
            //reads from console number 0 - 3, in case of wrong input returns 0 
            int numberOfMarkers = ConsoleRepresentation.ReadChoise(0, 3);
            Console.Clear();
            //gets from glossary db number of words in category, by category name 
            Console.WriteLine("The category {0} contains {1} Meanings\n\n", newWord.category, Glossary.GetNumberOfMeaningsInCategory(newWord.category));
            Console.WriteLine("Word: {0}\nMeaning: {1}\nCategory: {2}\nWithout markers jet.", newWord.word, newWord.meaning, newWord.category);
            Console.WriteLine("\n\nConfirm adding?");
            YesNo:
            Console.WriteLine("\n\n1.Yes\n2.No");
            switch (ConsoleRepresentation.ReadChoise(1, 2))
            {
                case 0:
                    Console.WriteLine("Input 1 or 2...");
                    goto YesNo;
                case 1:
                    int wordId = 0;
                    try
                    {
                        //try add word to glossary, recording the date and time of its adding
                        Glossary.InsertWord(newWord.word, newWord.meaning, newWord.category);
                        //get this word id from glossary
                        wordId = Glossary.GetIdByWord(newWord.word);
                        Console.WriteLine("The word added.");
                    }
                    catch
                    {
                        Console.WriteLine("This word is already in glossary");
                    }
                    //reads markers from console, as many as user chosed before 
                    List<string> markers = ConsoleRepresentation.ReadMarkers(numberOfMarkers);
                    foreach (string marker in markers)
                    {
                        //adds marker to markers table by word id
                        Glossary.stroredProcedures.InsertMarker(wordId, marker);
                    }
                    YesNo1:
                    Console.WriteLine("Do you want to add more Meanings to this category with same markers?");
                    Console.WriteLine("\n\n1.Yes\n2.No");
                    switch (ConsoleRepresentation.ReadChoise(1, 2))
                    {
                        case 0:
                            Console.WriteLine("Input 1 or 2...");
                            goto YesNo1;
                        case 1:
                            Console.Clear();
                            Console.WriteLine("*Adding Meanings into*" +
                                "\nCategory: " + newWord.category);
                            Console.WriteLine("With markers:");
                            foreach (string marker in markers)
                            {
                                Console.Write(marker + "\t");
                            }
                            //reading from console word and meaning 
                            Console.WriteLine("\nWord: ");
                            newWord.word = Convert.ToString(Console.ReadLine());
                            Console.WriteLine("Meaning: ");
                            newWord.meaning = Convert.ToString(Console.ReadLine());
                            Console.WriteLine("\n\nPress 1 to confirm adding.");
                            if (ConsoleRepresentation.ReadChoise(0, 1) == 1)
                            {
                                try
                                {
                                    //adding to glossary word and meaning to previously inputed category
                                    Glossary.InsertWord(newWord.word, newWord.meaning, newWord.category);
                                    //geting new word id 
                                    wordId = Glossary.GetIdByWord(newWord.word);
                                    foreach (string marker in markers)
                                    {
                                        //adding previosly inputed markers by new word id
                                        Glossary.stroredProcedures.InsertMarker(wordId, marker);
                                    }
                                    Console.WriteLine("Adding word success.");
                                }
                                catch
                                {
                                    Console.WriteLine("This word is already in DB!");
                                    Console.WriteLine("Adding word abort.");
                                }

                            }
                            else
                            {
                                Console.WriteLine("Adding word abort.");
                            }
                            goto YesNo1;
                        case 2:
                            break;
                    }
                    break;
                case 2:
                    Console.WriteLine("Adding the word canceled...");
                    break;
            }
        }

        private static void SearchMeanings(DateBase Glossary)
        {
            Console.WriteLine("***Glossary search***");
            Console.WriteLine("1.By category");
            Console.WriteLine("2.By markers");
            Console.WriteLine("3.By date");
            switch (ConsoleRepresentation.ReadChoise(1, 3))
            {
                case 0:
                    Console.WriteLine("Choose number from 1 to 3");
                    break;
                case 1:
                    Console.Clear();
                    Console.WriteLine("*Search by category*");
                    Console.Write("Category: ");
                    string categorySearch = Console.ReadLine();
                    //search words with matched category in glossary, represents it in console in format word - meaning - date 
                    ConsoleRepresentation.ShowWordByQueryResult(Glossary.GetMeaningsByCategory(categorySearch), (int)PresentOptions.Word_Meaning_Date);
                    break;
                case 2:
                    Console.WriteLine("*Search by markers*");
                    Console.WriteLine("How many markers do you want to use for search \n(number of markers 0 - 3)");
                    //read number of markers, by which use want to search
                    int numberOfMarkersForSearch = ConsoleRepresentation.ReadChoise(1, 3);
                    //read markers from console 
                    List<string> markers = ConsoleRepresentation.ReadMarkers(numberOfMarkersForSearch);
                    switch (numberOfMarkersForSearch)
                    {
                        //seach words in glossary by string markers and represents it in console
                        case 1:
                            ConsoleRepresentation.ShowWordByQueryResult(Glossary.SearchByMarker(markers[0]), (int)PresentOptions.Word_Meaning_Category);
                            break;
                        case 2:
                            ConsoleRepresentation.ShowWordByQueryResult(Glossary.SearchByMarker(markers[0], markers[1]), (int)PresentOptions.Word_Meaning_Category);
                            break;
                        case 3:
                            ConsoleRepresentation.ShowWordByQueryResult(Glossary.SearchByMarker(markers[0], markers[1], markers[2]), (int)PresentOptions.Word_Meaning_Category);
                            break;
                    }
                    break;
                case 3:
                    Console.Clear();
                    Console.WriteLine("*Search by date*");
                    Console.Write("Date:\n(dd.mm.yyyy) ");
                    DateTime date = DateTime.Now;
                    try
                    {
                        date = Convert.ToDateTime(Console.ReadLine());
                        //try to show all words, which were added at inputed date
                        ConsoleRepresentation.ShowWordByQueryResult(Glossary.GetMeaningsByDate(date), (int)PresentOptions.Word_Meaning_Category);
                    }
                    catch
                    {
                        Console.WriteLine("Wrong date input...\nPress Enter to try again.");
                        Console.ReadLine();
                    }
                    break;
            }
        }

        private static void RemoveWord(DateBase Glossary)
        {
            Console.Clear();
            Console.WriteLine("***Word search***");
            Console.Write("\n\nWord: ");
            string wordRM = Convert.ToString(Console.ReadLine());
            if (!ConsoleRepresentation.IsTrueInput(wordRM, 30))
            {
                Console.WriteLine("Wrong input");
                ConsoleRepresentation.BackToMenuMessage();
                return;
            }
            Console.WriteLine("Press 1 to delete it?");
            if (ConsoleRepresentation.ReadChoise(0, 1) == 1)
            {
                //removes word and information about it marking from glossary 
                Glossary.stroredProcedures.DeleteWord(wordRM);
            }
        }

        private static void ShowStatistic(DateBase Glossary)
        {
            Console.WriteLine("1.Statistic by markers.");
            Console.WriteLine("2.Statistic by categories.");
            Console.WriteLine("3.Statistic by days.");
            switch (ConsoleRepresentation.ReadChoise(1, 3))
            {
                case 0:
                    Console.WriteLine("Wrong input");
                    break;
                case 1:
                    Console.Clear();
                    Console.WriteLine("*Statistic by markers*\n");
                    Console.WriteLine("Marker\t Number of words");
                    //show the table with markers and number of words marked by it
                    Glossary.StatisticByMarkers();
                    break;
                case 2:
                    Console.Clear();
                    Console.WriteLine("*Statistic by categories*\n");
                    //show the table with categories and number of words in
                    Console.WriteLine("Category\t Number of words");
                    Glossary.StatisticByCategories();
                    break;
                case 3:
                    Console.Clear();
                    Console.WriteLine("*Statistic by days*");
                    //show the table with date and number of words marked by it
                    Console.WriteLine("Date\t Number of words");
                    Glossary.StatisticByDays();
                    break;
            }
        }

        private static void RemoveGlossary(DateBase Glossary)
        {
            {
                //deletes all words from glossary datebase 
                Glossary.Delete();
            }
        }
    }
}