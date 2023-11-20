using DSU23_G5.Infrastrukture;
using DSU23_G5.Models;
using DSU23_G5.Models.Dtos;
using DSU23_G5.Modelsusing;
using System;
using System.Linq;

namespace DSU23_G5.Repositories
{
    public class SpotPriceRepo : ISpotPriceRepo
    {

        /// <summary>
        /// Returns list of average spot price per month for a year view
        /// </summary>
        /// <param name="date"></param>
        /// <returns>pricePerMonth</returns>
        public async Task<List<double>> AverageMonthlyPricesPerYear(DateTime date, string? homePriceArea)
        {
            var month = date.Month;
            var year = date.Year;
            int daysInMonth = System.DateTime.DaysInMonth(year, month);
            double pricePerMonth = 0;
            List<double> pricePerMonthList = new List<double>();
            var currentSpotPriceApiClient = new CurrentSpotPriceApiClient();

            for (int m = 1; m < 13; m++)
            {
                if (m <= DateTime.Now.Month && year <= DateTime.Now.Year)
                {
                    for (int i = 1; i <= daysInMonth; i++)
                    {
                        if (i <= DateTime.Now.Day)
                        {
                            var todaysSpotPrice = await currentSpotPriceApiClient.GetAsync<CurrentSpotPriceDto>($"v1/prices/{year}/{AddAZeroAtBeginningOfSingleNumber(m)}-{AddAZeroAtBeginningOfSingleNumber(i)}_{homePriceArea}.json");

                            double dayAverage = AverageDailySpotPrice(todaysSpotPrice);
                            pricePerMonth += dayAverage;
                        }
                    }
                  pricePerMonth /= daysInMonth;
                  pricePerMonthList.Add(pricePerMonth);
                }
            }

            return pricePerMonthList;
        }

        /// <summary>
        /// Monthly view. Calculate average daily price per month
        /// </summary>
        /// <param name="date"></param>
        /// <returns>List<Double> price</returns>
        /// CRASH! Needs to add if-statement to catch when days  are after todays date
        public async Task<List<double>> PricePerMonth (DateTime date, string? homePriceArea)
        {
            var month = date.Month;
            var year = date.Year;
            int daysInMonth = System.DateTime.DaysInMonth(year, month);
            
            List<double> price = new List<double>();
            var currentSpotPriceApiClient = new CurrentSpotPriceApiClient();

            if (month <= DateTime.Now.Month && year <= DateTime.Now.Year)
            {

                for (int i = 1; i <= daysInMonth; i++)
                {

                    if (month == DateTime.Now.Month && i == DateTime.Now.Day + 1)
                    {
                        break;
                    }
                    else
                    {
                        var todaysSpotPrice = await currentSpotPriceApiClient.GetAsync<CurrentSpotPriceDto>($"v1/prices/{year.ToString()}/{AddAZeroAtBeginningOfSingleNumber(month)}-{AddAZeroAtBeginningOfSingleNumber(i)}_{homePriceArea}.json");
               
                        double dayAverage = AverageDailySpotPrice(todaysSpotPrice);
                        price.Add(dayAverage);
                    }
                }
                

            }

            return price;
        }

        /// <summary>
        /// Calculates average cost per day, in öre/kWh
        /// </summary>
        /// <param name="todaysSpotPrice"></param>
        /// <returns>double averagePriceToday</returns>
        public double AverageDailySpotPrice(CurrentSpotPriceDto[] todaysSpotPrice)
        {
            List<double> _dailySpotPrice = new List<double>();
       
            foreach (var item in todaysSpotPrice)
            {
                double hourInDay = item.SEK_per_kWh;
                _dailySpotPrice.Add(hourInDay);
            }
            double averagePriceToday = _dailySpotPrice.Sum() / 24 * 100;
            
            return averagePriceToday;

        }
        //Daily view, gets value for one specifik day

        public async Task<string[]> HourlySpotPrice(DateTime date, string? homePriceArea)
        {
            var year = date.Year;
            var month = date.Month;
            var day = date.Day;
            CurrentSpotPriceDto[] todaysSpotPrice;

            var currentSpotPriceApiClient = new CurrentSpotPriceApiClient();

            try
            {
                todaysSpotPrice = await currentSpotPriceApiClient.GetAsync<CurrentSpotPriceDto>($"v1/prices/{year.ToString()}/{AddAZeroAtBeginningOfSingleNumber(month)}-{AddAZeroAtBeginningOfSingleNumber(day)}_{homePriceArea}.json");
            }
            catch (Exception)
            {
                throw;
            }
            string[] spot = new string[todaysSpotPrice.Length];
            double[] sekValue = new double[todaysSpotPrice.Length];




            for (int i = 0; i < todaysSpotPrice.Count(); i++)
            {
                sekValue[i] = todaysSpotPrice[i].SEK_per_kWh * 100;
                spot[i] = sekValue[i].ToString("F3");
                spot[i] = spot[i].Replace(",", ".");

            }

            return spot;
        }
        public async Task<CurrentSpotPriceDto[]> SpotPrice(DateTime date, string? homePriceArea)
        {
            var year = date.Year;
            var month = date.Month;
            var day = date.Day;
            CurrentSpotPriceDto[] todaysSpotPrice;

            var currentSpotPriceApiClient = new CurrentSpotPriceApiClient();

            try
            {
                todaysSpotPrice = await currentSpotPriceApiClient.GetAsync<CurrentSpotPriceDto>($"v1/prices/{year.ToString()}/{AddAZeroAtBeginningOfSingleNumber(month)}-{AddAZeroAtBeginningOfSingleNumber(day)}_{homePriceArea}.json");
            }
            catch (Exception)
            {
                throw;
            }
            foreach (var item in todaysSpotPrice)
            {
                item.SEK_per_kWh = item.SEK_per_kWh * 100;
            }

            return todaysSpotPrice;
        }
        /// <summary>
        /// Method that returns the spot price for the current hour
        /// </summary>
        /// <param name="todaysSpotPrice"></param>
        /// <param name="currentHour"></param>
        /// <returns>string spot price </returns>
        public string SpotPriceRightNow(CurrentSpotPriceDto[] todaysSpotPrice, DateTime currentHour)
        {
            var result = todaysSpotPrice.Where(g => g.Time_Start.Hour == currentHour.Hour);
            string spot = "pris";
            foreach (var item in result)
            {

                spot = item.SEK_per_kWh.ToString("F3");
                spot = spot.Replace(".", ",");
            }

            return spot;
        }

        /// <summary>
        /// Method for getting cheapest spotprice hour in one day
        /// </summary>
        /// <param name="hourlySpotPrice"></param>
        /// <returns>DateTime representing cheapest hour today</returns>
        public DateTime CheapestHour(CurrentSpotPriceDto[] todaysSpotPrice)
        {
            List<double> spot = new List<double>();
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
        private string AddAZeroAtBeginningOfSingleNumber(int date)
        {
            string newDate;

            if (date.ToString().Length < 2)
            {
                newDate = date.ToString().PadLeft(2, '0');
                return newDate;
            }
            else return date.ToString();

        }

    }
    
}
