using DSU23_G5.Infrastrukture;
using DSU23_G5.Models.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace DSU23_G5.Repositories
{
    public class ConsumptionRepo : IConsumptionRepo
    {
        /// <summary>
        /// Makes API call to consumption API and sends the value and date response into methods for day/month/year views
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="selector"></param>
        /// <returns>returns string list with consumption values for day/month/year view</returns>
        public async Task<List<string>> HandleConsumptionAPIData(string? endpoint, string? selector)
        {
            var apiClient = new ConsumptionApiClient();
            var result = await apiClient.GetAsync<ConsumptionDto>(endpoint);

            List<string> totalConsumption = new List<string>();


            if (selector == "viewYear")
            {

                return totalConsumption = CalculateMonthlyConsumption(result.Values, result.Date_From);

            }

            if (selector == "viewMonth")
            {
                return totalConsumption = CalculateDailyConsumption(result.Values);
            }

            else
            {
                return totalConsumption = HourlyConsumption(result.Values);

            }


        }

        /// <summary>
        /// Turns chosen day's hourly values into string and replaces decimal seperator 
        /// </summary>
        /// <returns>String list of the hourly values for a chosen day</returns>

        public List<string> HourlyConsumption(List<double>? _hourValues)
        {

            List<string> _hoursRounded = new List<string>();

            for (int i = 0; i < _hourValues?.Count; i++)
            {
                string hour = _hourValues[i].ToString();
                hour = hour.Replace(",", ".");
                _hoursRounded.Add(hour);
            }
            return _hoursRounded;
        }
        /// <summary>
        /// Calculates daily consumption for a month
        /// </summary>
        /// <returns>String list of daily consumption values</returns>

        public List<string> CalculateDailyConsumption(List<double>? _hourValues)
        {
            List<string> dailyConsumptionPerMonth = new List<string>();

            for (int i = 0; i < _hourValues?.Count; i += 24)
            {
                string twentyFourHours = _hourValues.Skip(i).Take(24).Sum().ToString();
                twentyFourHours = twentyFourHours.Replace(",", ".");
                dailyConsumptionPerMonth.Add(twentyFourHours);
            }

            return dailyConsumptionPerMonth;

        }
        /// <summary>
        /// Calculates total consumption per month for a year
        /// </summary>
        /// <returns>List string of monthly consumption values</returns>
        public List<string> CalculateMonthlyConsumption(List<double>? _hourValues, DateTime fromTime)
        {


            List<string> _monthlyConsumptionPerYear = new List<string> { };


            for (int i = 0; i < _hourValues?.Count();)
            {

                if (i != 2160 || i != 3624 || i != 5832 || i != 7296 || i != 744)
                {
                    string eachMonthInYear = _hourValues.Skip(i).Take(744).Sum().ToString();
                    eachMonthInYear = eachMonthInYear.Replace(",", ".");
                    i += 744;
                    _monthlyConsumptionPerYear.Add(eachMonthInYear);

                }

                if (i == 2160 || i == 3624 || i == 5832 || i == 7296)
                {
                    string eachMonthInYear = _hourValues.Skip(i).Take(720).Sum().ToString();
                    eachMonthInYear = eachMonthInYear.Replace(",", "."); ;
                    i += 720;
                    _monthlyConsumptionPerYear.Add(eachMonthInYear);


                }

                if (i == 744)
                {
                    if (DateTime.IsLeapYear(fromTime.Year))
                    {
                        string eachMonthInYear = _hourValues.Skip(i).Take(696).Sum().ToString();
                        eachMonthInYear = eachMonthInYear.Replace(",", ".");
                        i += 696;
                        _monthlyConsumptionPerYear.Add(eachMonthInYear);

                    }
                    else
                    {
                        string eachMonthInYear = _hourValues.Skip(i).Take(672).Sum().ToString();
                        eachMonthInYear = eachMonthInYear.Replace(",", ".");
                        i += 672;
                        _monthlyConsumptionPerYear.Add(eachMonthInYear);
                    }


                }



            }


            return _monthlyConsumptionPerYear;

        }

    }
}
