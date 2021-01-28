using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EFLibrary.Models;
using System.Linq;

namespace EFLibrary
{
    public class CleanedCollections
    {
        public DateTime? Date { get; set; }
        public double? Level { get; set; }
        public string Info { get; set; }

    }

    public class SQLService
    {
        public static int FileReader(string filePath)
        {
            int rowsAffected = 0;
            try
            {
                using (var context = new EFContext())
                {
                    string[] lines = File.ReadAllLines(filePath); 

                    foreach (var filePosts in lines)
                    {
                        rowsAffected++;
                        string[] fields = filePosts.Split(' ', ','); //Seperates date and timespan + each post.
                        var databaseInput = new Wheather();
                        databaseInput.Date = DateTime.Parse(fields[0]); //Get read in just as short date to DB.
                        databaseInput.Time = TimeSpan.Parse(fields[1]); //Gets read in just as timespan to DB.
                        databaseInput.Place = fields[2];
                        databaseInput.Temp = double.Parse(fields[3], CultureInfo.InvariantCulture); 
                        databaseInput.Humidity = int.Parse(fields[4]);
                        context.Add(databaseInput);
                    }

                        context.SaveChanges(); 
                }
            }
            catch
            {
              return rowsAffected;
            }

            return rowsAffected;
        }
        
        public static double? midtempDate(string calenderParam, bool? location)
        {
            string enviroment = "";
            if(location == true)
            {
                enviroment = "Inne";
            }
            else if(location == false)
            {
                enviroment = "Ute";
            }
            double? adder = 0.0;
            double? midTemp = 0.0;
            using (var context = new EFContext())
            {
                var entites = context.wheathers.
                    Where(p => p.Date.ToString() == calenderParam && p.Place == enviroment).
                    Select(p => p.Temp);

                foreach (var temp in entites)
                {
                    adder = adder + temp;
                }
                midTemp = adder / entites.Count();
            }

            return Math.Round((double)midTemp, 2);
        }
        
        public static IEnumerable <CleanedCollections> TopColdestHottest(bool? location, bool tempOrHumidity)
        {
            List<CleanedCollections> days = new List<CleanedCollections>();
            string enviroment = "";
            
            if (location == true)
            {
                enviroment = "Inne";
            }
            else if (location == false)
            {
                enviroment = "Ute";
            }
            using (var context = new EFContext())
            {
                var entites = context.wheathers.
                    Where(p => p.Place == enviroment).
                    AsEnumerable().                //To specify sorting should be done client-side.
                    GroupBy(p => p.Date);
                 
                foreach(var eachDay in entites)    //Gives a IGroup of each element in each group Key from Groupby
                {
                    int itterations = 0;
                    double? adder = 0;
                    double? midTemp = 0;

                    CleanedCollections day = new CleanedCollections();
                    day.Date = eachDay.Key;

                    foreach(var measures in eachDay) 
                    {
                        if(tempOrHumidity == false)
                        {
                           adder = adder + measures.Temp;
                        }
                        else if(tempOrHumidity == true)
                        {
                           adder = adder + measures.Humidity;
                        }
                        itterations++;
                    }
                    midTemp = adder / itterations;
                    day.Level = Math.Round((double)midTemp, 0);
                    days.Add(day);
                }    
            }

            var hottest = days.
              OrderByDescending(p => p.Level).
              Take(3);
            var coldest = days.
              OrderByDescending(p => p.Level).
              TakeLast(3);

            var topThree = hottest.Concat(coldest);
            
            return topThree;
        }
        
        public static IEnumerable <CleanedCollections> MoleRisk(bool? location)
        {
            List<CleanedCollections> days = new List<CleanedCollections>();
            string enviroment = "";

            if (location == true)
            {
                enviroment = "Inne";
            }
            else if (location == false)
            {
                enviroment = "Ute";
            }
            using (var context = new EFContext())
            {
                var entites = context.wheathers.
                    Where(p => p.Place == enviroment).
                    AsEnumerable().                //To specify sorting should be done client-side.
                    GroupBy(p => p.Date);

                foreach (var eachDay in entites)    //Gives a IGroup of each element in each group Key from Groupby
                {
                    int itterations = 0;
                    double? moleRisk = 0;
                    double? adderTemp = 0;
                    double? adderHumid = 0;
                    double? midTemp = 0;
                    double? midHumid = 0;

                    CleanedCollections day = new CleanedCollections();
                    day.Date = eachDay.Key;

                    foreach (var measures in eachDay)
                    {
                        adderTemp = adderTemp + measures.Temp;
                        adderHumid = adderHumid + measures.Humidity;
                        itterations++;
                    }
                    midTemp = adderTemp / itterations;
                    midHumid = adderHumid / itterations; 
      
                    if(midHumid < 78 && midTemp < 0)
                    {
                        moleRisk = 0;
                    }
                    else
                    {
                        moleRisk = ((midHumid - 78) * (midTemp / 15)) / 0.22;
                    }
                    day.Level = Math.Round((double)moleRisk, 0);
                    days.Add(day);
                }
            }

            var mostHumidDays = days.
              OrderByDescending(p => p.Level).
              Take(3);
            var leastHumidDays = days.
              OrderByDescending(p => p.Level).
              TakeLast(3);

            var topThree = mostHumidDays.Concat(leastHumidDays);

            return topThree;
        }
        
        public static IEnumerable<CleanedCollections> MeteorologicalSeason ()
        {
            List<CleanedCollections> days = new List<CleanedCollections>();
            string enviroment = "Ute";

            using (var context = new EFContext())
            {
                var entites = context.wheathers.
                    Where(p => p.Place == enviroment).
                    AsEnumerable().                   //To specify sorting should be done client-side.
                    GroupBy(p => p.Date);

                foreach (var eachDay in entites)    //Gives a IGroup of each element in each group Key from Groupby.
                {
                    int itterations = 0;
                    double? adder = 0;
                    double? midTemp = 0;

                    CleanedCollections day = new CleanedCollections();
                    day.Date = eachDay.Key;

                    foreach (var measures in eachDay)
                    {
                        adder = adder + measures.Temp;
                        itterations++;
                    }
                    midTemp = adder / itterations;
                    day.Level = Math.Round((double)midTemp, 2);
                    days.Add(day);
                }
            }

            var allTempatures = days.
              OrderBy(p => p.Date).ToList();

            CleanedCollections cleaned = new CleanedCollections();
            List<CleanedCollections> seasons = new List<CleanedCollections>();
            
            for(int i=0; i<days.Count; i++)
            {
                try
                {
                    if (allTempatures[i].Level >= 0 && allTempatures[i].Level <= 10 && allTempatures[i + 1].Level >= 0 && allTempatures[i + 1].Level <= 10 && allTempatures[i + 2].Level >= 0 && allTempatures[i + 2].Level <= 10 && allTempatures[i + 3].Level >= 0 && allTempatures[i + 3].Level <= 10 && allTempatures[i + 4].Level >= 0 && allTempatures[i + 4].Level <= 10)
                    {
                        cleaned.Date = allTempatures[i].Date;
                        cleaned.Level = allTempatures[i].Level;
                        cleaned.Info = "Höst";
                        seasons.Add(cleaned);
                        break;
                    }
                }
                catch
                {
                    continue;
                }
                try
                {
                    if (allTempatures[i].Level <= 0 && allTempatures[i + 1].Level <= 0 && allTempatures[i + 2].Level <= 0 && allTempatures[i + 3].Level <= 0 && allTempatures[i + 4].Level <= 0)
                    {
                        cleaned.Date = allTempatures[i].Date;
                        cleaned.Level = allTempatures[i].Level;
                        cleaned.Info = "Vinter";
                        seasons.Add(cleaned);
                        break;
                    }
                }
                catch
                {
                    continue;
                }
            }

            return seasons;
        }
    }
}
