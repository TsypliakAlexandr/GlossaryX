using System;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Data.Linq;
using System.Reflection;
using System.Globalization;

namespace GlossaryX.lib
{
    [Table(Name = "StatisticByMarkers")]
    public class StatisticByMarkers
    {
        [Column(Name = "Marker")]
        public string Marker { get; set; }
        [Column(Name = "Number of Meanings")]
        public int Number { get; set; }
    }

    [Table(Name = "StatisticByDays")]
    public class StatisticByDays
    {
        [Column(Name = "Date")]
        public DateTime Date { get; set; }
        [Column(Name = "Number of Meanings")]
        public int Number { get; set; }
    }

    [Table(Name = "StatisticByCategorier")]
    public class StatisticByCategories
    {
        [Column(Name = "Category")]
        public string Category { get; set; }
        [Column(Name = "Number of Meanings")]
        public int Number { get; set; }
    }

    [Table(Name = "MeaningsBase")]
    public class Meanings
    {
        [Column(Name = "Id", IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }
        [Column(Name = "Word")]
        public string Word { get; set; }
        [Column(Name = "Meaning")]
        public string Meaning { get; set; }
        [Column(Name = "Category")]
        public string Category { get; set; }
        [Column(Name = "EditDate")]
        public DateTime EditDate { get; set; }
    }

    [Table(Name = "MarkersBase")]
    public class Markers
    {
        [Column(Name = "Id")]
        public int Id { get; set; }
        [Column(Name = "Marker")]
        public string Marker { get; set; }
    }


    class DateBase
    {
        public class StroredProcedures : DataContext
        {
            public StroredProcedures(string connectionString)
                : base(connectionString)
            {

            }

            public Table<Markers> Markers { get { return this.GetTable<Markers>(); } }

            [Function(Name = "sp_InsertMarker")]
            [return: Parameter(DbType = "Int")]
            public int InsertMarker(
                [Parameter(Name = "id", DbType = "int")] int id,
                [Parameter(Name = "marker", DbType = "NVarChar(30)")] string marker)
            {
                //executes prcedure, which add cell to table "MarkersBase", id of word, which contains marker, and this (one)marker in string
                IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), id, marker);
                return 0;
            }

            [Function(Name = "sp_DeleteWord")]
            [return: Parameter(DbType = "Int")]
            public int DeleteWord(
                [Parameter(Name = "word", DbType = "NVarChar(30)")] string word)
            {
                //delete word by it string view
                IExecuteResult result = this.ExecuteMethodCall(this, ((MethodInfo)(MethodInfo.GetCurrentMethod())), word);
                return 0;
            }
        }

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

        SqlConnectionStringBuilder builder;
        DataContext db;
        public static Table<Meanings> MeaningsTable;
        Table<Markers> markersTable;
        Table<StatisticByDays> statisticByDaysTable;
        Table<StatisticByCategories> statisticByCategoriewTable;
        Table<StatisticByMarkers> statisticByMarkersTable;
        public StroredProcedures stroredProcedures;

        public DateBase(string connectString)
        {
            builder = new SqlConnectionStringBuilder(connectString);
            db = new DataContext(builder.ConnectionString);
            //intialize database using ;ocal string connection to datebase server
            MeaningsTable = db.GetTable<Meanings>();
            //reads table with information about each added word in format |id - word - meaning - category - date of creation|
            markersTable = db.GetTable<Markers>();
            //reads table with inforamtion about each added marker in format |word id - marker|
            statisticByDaysTable = db.GetTable<StatisticByDays>();
            statisticByCategoriewTable = db.GetTable<StatisticByCategories>();
            statisticByMarkersTable = db.GetTable<StatisticByMarkers>();
            //read statistic tables, with represents number of words by date, categories or markers 
            stroredProcedures = new StroredProcedures(connectString);
            //intializing stored procedures 
        }

        public IEnumerable<Meanings>  GetAllMeaningsSorted(int SortOption)
        {
            IEnumerable<Meanings> answer = null;
            switch (SortOption)
            {
                //gets IEnumerable of Meanings(see class at the top), sorted by word, by date or by category
                case 1:
                    answer = MeaningsTable.OrderBy(u => u.Word).Take(50);
                    break;
                case 2:
                    answer = MeaningsTable.OrderByDescending(u => u.EditDate).Take(50);
                    break;
                case 3:
                    answer = MeaningsTable.OrderBy(u => u.Category).Take(50);
                    break;
            }
            return answer;
        }

        public IEnumerable<StatisticByMarkers> StatisticByMarkers()
        {
            IEnumerable<StatisticByMarkers> statistic = statisticByMarkersTable.OrderBy(u => u.Marker);
            foreach (var element in statistic)
            {
                Console.WriteLine("{0}\t{1}", element.Number, element.Marker);
            }
            return statistic;
        }

        public IEnumerable<StatisticByCategories> StatisticByCategories()
        {
            IEnumerable<StatisticByCategories> statistic = statisticByCategoriewTable.OrderBy(u => u.Category);
            foreach (var element in statistic)
            {
                Console.WriteLine("{0}\t{1}", element.Number, element.Category);
            }
            return statistic;
        }

        public IEnumerable<StatisticByDays> StatisticByDays()
        {
            IEnumerable<StatisticByDays> statistic = statisticByDaysTable.OrderByDescending(u => u.Date);
            foreach (var cell in statistic)
            {
                Console.WriteLine(String.Format("{0} \t{1}", cell.Date.ToString("dd.MM.yyyy"), cell.Number));
            }
            return statistic;
        }

        public Meanings InsertWord(string word, string meaning, string category)
        {
            //creates a specimen of class Meanings(saving word - category - meaning - creation date), its id setteled by datebase automaticaly
            Meanings wordAdd = new Meanings { Word = word, Category = category, Meaning = meaning, EditDate = DateTime.Now};
            db.GetTable<Meanings>().InsertOnSubmit(wordAdd);
            db.SubmitChanges();
            return wordAdd;
        }

        public int GetIdByWord(string word)
        {
            int id = 0; 
            IEnumerable<Meanings> Meanings = MeaningsTable.Where(u => u.Word == word).Take(1);
            foreach(var element in Meanings)
            {
                id = element.Id;
            }
            return id; 
        }

        public IEnumerable<Meanings> GetMeaningsByDate(DateTime date)
        {
            //gets words, which were added inputed date and order it by category
            IEnumerable<Meanings> Meanings =  MeaningsTable.Where(u => u.EditDate.Date == date.Date).OrderBy(u => u.Category);
            return Meanings;
        }

        public IEnumerable<Meanings> GetMeaningsByCategory(string category)
        {
            IEnumerable<Meanings> Meanings = MeaningsTable.Where(u => u.Category == category).OrderByDescending(u => u.EditDate);
            return Meanings;
        }

        public void Delete()
        {
           Console.WriteLine("The glossary was deleted\nDeleted Meanings: {0}", db.ExecuteCommand("Delete from MeaningsBase where id>0"));
        }

        public int GetNumberOfMeaningsInCategory(string category)
        {
            //return the number of words in category
            return MeaningsTable.Where(u => u.Category == category).Count();
        }

        public IEnumerable<Meanings> SearchByMarker(string marker)
        {//invokes stored procedure, which search words by marker
            List<Meanings> Meanings = new List<Meanings>();
            string sqlExpression = "sp_SearchByMarker1";
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                SqlParameter markerParam = new SqlParameter
                {
                    ParameterName = "@marker",
                    Value = marker
                };
                command.Parameters.Add(markerParam);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string word = reader.GetString(1);
                    string meaning = reader.GetString(2);
                    DateTime date = reader.GetDateTime(3);
                    string category = reader.GetString(4);
                    Meanings wordAdd = new Meanings { Word = word, Category = category, Meaning = meaning, EditDate = date, Id =id };
                    Meanings.Add(wordAdd);
                }
                reader.Close();
                connection.Close();
            }
            IEnumerable<Meanings> Meaningsreturn = Meanings;
            return Meaningsreturn;
        }

        public IEnumerable<Meanings> SearchByMarker(string marker1, string marker2)
        {
            List<Meanings> Meanings = new List<Meanings>();
            string sqlExpression = "sp_SearchByMarker2";

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                SqlParameter markerParam1 = new SqlParameter
                {
                    ParameterName = "@marker1",
                    Value = marker1
                };
                command.Parameters.Add(markerParam1);
                SqlParameter markerParam2 = new SqlParameter
                {
                    ParameterName = "@marker2",
                    Value = marker2
                };
                command.Parameters.Add(markerParam2);
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string word = reader.GetString(1);
                        string meaning = reader.GetString(2);
                        DateTime date = reader.GetDateTime(3);
                        string category = reader.GetString(4);
                        Meanings wordAdd = new Meanings { Word = word, Category = category, Meaning = meaning, EditDate = date, Id = id };
                        Meanings.Add(wordAdd);
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Meanings marked as {0} and {1}  was not found in glossary...", marker1, marker2);
                }
                reader.Close();
            }
            IEnumerable<Meanings> Meaningsreturn = Meanings;
            return Meaningsreturn;
        }

        public IEnumerable<Meanings> SearchByMarker(string marker1, string marker2, string marker3)
        {
            List<Meanings> Meanings = new List<Meanings>();
            string sqlExpression = "sp_SearchByMarker3";

            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                SqlParameter markerParam1 = new SqlParameter
                {
                    ParameterName = "@marker1",
                    Value = marker1
                };
                command.Parameters.Add(markerParam1);
                SqlParameter markerParam2= new SqlParameter
                {
                    ParameterName = "@marker2",
                    Value = marker2
                };
                command.Parameters.Add(markerParam2);
                SqlParameter markerParam3 = new SqlParameter
                {
                    ParameterName = "@marker3",
                    Value = marker3
                };
                command.Parameters.Add(markerParam3);
                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string word = reader.GetString(1);
                        string meaning = reader.GetString(2);
                        DateTime date = reader.GetDateTime(3);
                        string category = reader.GetString(4);
                        Meanings wordAdd = new Meanings { Word = word, Category = category, Meaning = meaning, EditDate = date, Id = id };
                        Meanings.Add(wordAdd);
                    }
                }
                else
                {
                    Console.WriteLine("Meanings marked as {0}, {1} and {2} was not found in glossary...", marker1, marker2, marker3);
                }
                reader.Close();
            }
            IEnumerable<Meanings> Meaningsreturn = Meanings;
            return Meaningsreturn;
        }

    }
}
