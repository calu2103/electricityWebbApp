using DSU23_G5.Infrastrukture;
using DSU23_G5.Models;
using DSU23_G5.Models.Dtos;
using DSU23_G5.Modelsusing;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics.Metrics;

using System.Net.WebSockets;


namespace DSU23_G5.Repositories
{
    public class ConsumerItemsRepo : IConsumerItems
    {
        public static List<ConsumerItems> _consumerItems = new List<ConsumerItems>
        {
          new ConsumerItems("Elbil",  22 , 8),
          new ConsumerItems("Diskmaskin", 0.54, 2),
          new ConsumerItems( "Tvättmaskin", 0.6 , 3)
        };
    
        /// <summary>
        /// Sets optimal time interval to each list object and returns the list with consumer item objects
        /// </summary>
        /// <param name="spotPrice"></param>
        /// <param name="selector"></param>
        /// <returns>List with consumer items objects </returns>
        public List<ConsumerItems> GetItems(string[] spotPrice, string? selector)        
        {
            foreach (var item in _consumerItems)
            {
                item.OptimalStartTime = OptimalConsumtionInterval(spotPrice, item.OperatingTime, selector);
            }

            return _consumerItems;
        }
        /// <summary>
        /// Adds new consumer item
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Consumer item</returns>
        public ConsumerItems AddItem(ConsumerItems item )
        {
            _consumerItems.Add( item );
            return item;
        }

        /// <summary>
        /// Removes chosen consumer item
        /// </summary>
        /// <param name="itemName"></param>
        public void RemoveItem(string itemName)
        {
            for (int i = 0; i < _consumerItems.Count; i++)
            {
                if (_consumerItems[i].Name == itemName)
                {
                    _consumerItems.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Method for getting cheapest spotprice hour in one day
        /// </summary>
        /// <param name="hourlySpotPrice"></param>
        /// <returns>DateTime representing cheapest hour today</returns>
        public DateTime OptimalConsumptionTime(CurrentSpotPriceDto[] todaysSpotPrice)
        {
            List <double> spot = new List<double>();
            foreach (var item in todaysSpotPrice)
            {
                if (item.Time_Start.Hour >= DateTime.Now.Hour)
                {
                    spot.Add(item.SEK_per_kWh);
                }
            }
            var lowestSpot = todaysSpotPrice.Where(g => g.SEK_per_kWh == spot.Min());
            DateTime cheapestHour = new DateTime();
            foreach (var item in lowestSpot)
            {
                cheapestHour = item.Time_Start;
            }
            return cheapestHour;

        }

        /// <summary>
        /// Method for finding the operational time interval with the lowest average price
        /// It will show from the hour after the current one
        /// The lenght of the timespan it can check will change depending on how many hours the item's operational time covers
        /// </summary>
        /// <param name="hourlySpotPrice"></param>
        /// <param name="operationalTime"></param>
        /// <returns>string representing the hour of a day where the following interval has the lowest average price</returns>
        public string OptimalConsumtionInterval(string[] hourlySpotPrice, int operationalTime, string? selector )
        {           
            string[] convertedHourlySpotPrice = ConvertPunctuation(hourlySpotPrice);
            double[] spotPriceDouble = Array.ConvertAll(hourlySpotPrice, s => double.Parse(s));

            int shortenLenght = operationalTime - 1;
            int counter = 0;
            List<double> averagePriceForIntervall = new List<double>();
            int? suitableOperationalTime;
            

            //Today view will check intervals from the following hour until the end of the array minus operational time
            if (selector == "viewToday" || selector == null)
            {
                int currentHour = DateTime.Now.Hour;
                int hoursToSkip = DateTime.Now.Hour + 1;

               for (int i = 0; i < spotPriceDouble.Length - shortenLenght; i++)
               {                   
                 if (i > currentHour)
                 {
                    double interval = spotPriceDouble.Skip(hoursToSkip + counter).Take(operationalTime).Sum() / operationalTime;
                    averagePriceForIntervall.Add(interval);
                    counter++;          

                 }
               }

                suitableOperationalTime = HandleListContentForTodayView(averagePriceForIntervall, currentHour);              
  
            }

            //This will check the cheapest interval in the full 24 hours of the following day when spot prices for that day exists (after 1pm CET day before)
            //Button for this is disabled in the ConsumerItems view with javascript before 1 pm
            else
            {
                for (int i = 0; i < spotPriceDouble.Length - shortenLenght; i++)
                {
                        double intervall = spotPriceDouble.Skip(counter).Take(operationalTime).Sum() / operationalTime;
                        averagePriceForIntervall.Add(intervall);
                        counter++;                   
                }
                var lowestIntervall = averagePriceForIntervall.Min();

                suitableOperationalTime = averagePriceForIntervall.FindIndex(s => s == lowestIntervall);
            }
    
            string convertedOperationalTime = SetOptimalTimeText(suitableOperationalTime, selector);

            return convertedOperationalTime;

        }

        /// <summary>
        /// Handles list content for today view, if the list is empty due to lack of values in array lenght
        /// the start time is set to null so that it can later be set to message "check the tomorrow view"
        /// Otherwise the lowest value interval is returned in int format
        /// </summary>
        /// <param name="averagePriceForIntervall"></param>
        /// <param name="currentHour"></param>
        /// <returns>int representing start time</returns>
        public int? HandleListContentForTodayView (List<double> averagePriceForIntervall, int currentHour)
        {
            int? suitableOperationalTime;

            if (!averagePriceForIntervall.IsNullOrEmpty())
            {
                var lowestIntervall = averagePriceForIntervall.Min();

                suitableOperationalTime = averagePriceForIntervall.FindIndex(s => s == lowestIntervall) + currentHour + 1;
            }

            else
            {
                
                suitableOperationalTime = null;

            }

            return suitableOperationalTime;
        }
        /// <summary>
        /// Sets the optimal start time text to either a message if an optimal time cannot be found 
        /// within the hours of the current day or to a DateTime to string text with hours and minutes if time can be found
        /// </summary>
        /// <param name="suitableOperationalTime"></param>
        /// <param name="selector"></param>
        /// <returns>string representing the optimal time or a message to check the view for tomorrow</returns>
        public string SetOptimalTimeText(int? suitableOperationalTime, string? selector)
        {
            string suitableOperationalTimeToString;

            if (suitableOperationalTime == null)
            {
                suitableOperationalTimeToString = "Se morgondagens prognos";
            }

            else
            {
                DateTime convertedTime = ConvertStartTime(suitableOperationalTime, selector);

                suitableOperationalTimeToString = convertedTime.ToString("HH:mm");
            }

            return suitableOperationalTimeToString;
        }

        /// <summary>
        /// Converts the int for suitable time into a DateTime format
        /// </summary>
        /// <param name="suitableOperationalTime"></param>
        /// <param name="selector"></param>
        /// <returns>suitable time in DateTime format</returns>
        public DateTime ConvertStartTime (int? suitableOperationalTime, string? selector)
        {
            DateTime convertedOperationalTime;

            if (selector != null)
            {
              if (selector == "viewToday")
              {
               convertedOperationalTime = DateTime.Today.AddHours((double)suitableOperationalTime!);
              }

               else
               {
                 convertedOperationalTime = DateTime.Today.AddHours((double)suitableOperationalTime!);
                 convertedOperationalTime = convertedOperationalTime.AddDays(1);
                   
               }

            }

            else
            {
                convertedOperationalTime = DateTime.Today.AddHours((double)suitableOperationalTime!);
            }


            return convertedOperationalTime;
        }
        /// <summary>
        /// Changes punctuation from , to . to get the right format for optimal time method
        /// </summary>
        /// <param name="hourlySpotPrice"></param>
        /// <returns>string array with hourly spotprices</returns>
        public string[] ConvertPunctuation(string[] hourlySpotPrice)
        {
            for (int i = 0; i < hourlySpotPrice.Count(); i++)
            {
                hourlySpotPrice[i] = hourlySpotPrice[i].Replace(".", ",");
            }

            return hourlySpotPrice;
        }

        /// <summary>
        /// Calculates the cost of consumption based on kWh, operatingTime and spotprice during the operatingTime
        /// </summary>
        /// <param name="item"></param>
        /// <param name="spotPrice"></param>
        /// <param name="selector"></param>
        /// <returns>cost as a double with one decimal in kr</returns>
        public double? ConsumptionCost(ConsumerItems item, string[] spotPrice, string? selector)
        {
            double? cost = 0;
            string startTime = OptimalConsumtionInterval(spotPrice, item.OperatingTime, selector);
            if (startTime.Length < 6)
            {
                int hour = Int32.Parse(startTime.Substring(0, 2));
                for (int i = 0; i < item.OperatingTime; i++)
                {
                    cost = cost + Convert.ToDouble(spotPrice[hour]) * item.KWh;
                }
                decimal costKr = (decimal)cost!;
                costKr = costKr / 100;
                costKr = Math.Round(costKr, 1);
                cost = decimal.ToDouble(costKr);
                return cost;
            }
            return cost;
        }

        /// <summary>
        /// Looks through the list of items and changes operating time on the item matching the name of the name-parameter
        /// This could be improved by passing the item directly as a param but could not be done due to time constraints
        /// </summary>
        /// <param name="name"></param>
        /// <param name="operatingTime"></param>
        public void ChangeOperatingTime(string name, int operatingTime)
        {
            foreach (var item in _consumerItems)
            {
                if (name == item.Name)
                {
                    item.OperatingTime = operatingTime;
                }
            }

        }

    }
}