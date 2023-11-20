using DSU23_G5.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;
using DSU23_G5.Infrastrukture;
using DSU23_G5.ViewModels;
using DSU23_G5.Repositories;
using Microsoft.AspNetCore.Http;
using DSU23_G5.Modelsusing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace DSU23_G5.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConsumptionRepo consumptionRepo;
        private readonly ISpotPriceRepo spotPriceRepo;

        public HomeController(IConsumptionRepo consumptionRepo, ISpotPriceRepo spotPriceRepo)
        {
            this.consumptionRepo = consumptionRepo;
            this.spotPriceRepo = spotPriceRepo;
        }

        /// <summary>
        /// Method for downloading chart as JSON
        /// </summary>
        /// <param name="home"></param>
        /// <returns>.json file (chartdata.json)</returns>
        public IActionResult DownloadChart(string home)
        {
            User user = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("User")!)!;
            if (home == "homeOne")
            {
                var chartData = user.Homes[0].Chart;

                var chartDataJson = JsonConvert.SerializeObject(chartData);

                var chartDataStream = new MemoryStream(Encoding.UTF8.GetBytes(chartDataJson));

                return File(chartDataStream, "application/json", "chartdata.json");

            }
            else
            {
                var chartData = user.Homes[1].Chart;

                var chartDataJson = JsonConvert.SerializeObject(chartData);

                var chartDataStream = new MemoryStream(Encoding.UTF8.GetBytes(chartDataJson));

                return File(chartDataStream, "application/json", "chartdata.json");
            }

        }

        /// <summary>
        /// Updating chartview depending on what parameters selected by user.
        /// </summary>
        /// <param name="home">selected home (1 or 2)</param>
        /// <param name="year">selected year</param>
        /// <param name="month">selected month</param>
        /// <param name="day">selected day</param>
        /// <param name="selector">selected view (day/month/year)</param>
        /// <returns>View for selected home/dates OR error message</returns>
        public async Task<IActionResult> UpdateChart(string home, string? year, string? month, int day, string? selector)
        {
            try
            {
                User user = JsonConvert.DeserializeObject<User>(HttpContext.Session.GetString("User")!)!;
                DateTime todaysDate = new DateTime();
                todaysDate = DateTime.Now;
                DateTime tomorrowsDate = todaysDate.Date.AddDays(1);
                string dayId = AddAZeroAtBeginningOfSingleNumber(day);
                string monthId = AddAZeroAtBeginningOfSingleNumber(ConvertMonthToDate(month));
                string? yearId = year;
                string stringDate = $"{yearId}-{monthId}-{dayId}";
                string dateToShowInInterface = SetDateStringToShow(day, month, year, selector, todaysDate);
                //Creates chart for homeOne.
                if (home == "homeOne")
                {
                    var consumptionEndPointForHomeOne = SetEndPointForConsumptionBySelectedDate(dayId, monthId, yearId, selector, todaysDate, tomorrowsDate, user.Homes?[0].Contract);

                    DisplayChartViewModel chart1 = FillChartWithValues(stringDate, selector, consumptionEndPointForHomeOne, dateToShowInInterface, user.Homes?[0].PriceArea);
                    user.Homes![0].Chart = chart1;

                    var spotPricesToday = spotPriceRepo.SpotPrice(todaysDate, user.Homes[0].PriceArea);
                    chart1.SpotPriceRightNow = spotPriceRepo.SpotPriceRightNow(await spotPricesToday, todaysDate);
                    var lowestPrice = spotPriceRepo.CheapestHour(await spotPricesToday);
                    chart1.CheapestHour = lowestPrice.ToString("HH:mm");
                    
                    HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));
                    SaveSelectedResolution(selector);
                    SaveSelectedHome(home);

                    return View("Index", user.Homes[0].Chart);
                }
                // Creates chart for homeTwo.
                else if (home == "homeTwo")
                {
                    var consumptionEndPointForHomeTwo = SetEndPointForConsumptionBySelectedDate(dayId, monthId, yearId, selector, todaysDate, tomorrowsDate, user.Homes?[1].Contract);

                    DisplayChartViewModel chart1 = FillChartWithValues(stringDate, selector, consumptionEndPointForHomeTwo, dateToShowInInterface, user.Homes?[1].PriceArea);
                    user.Homes![1].Chart = chart1;

                    var spotPricesToday = spotPriceRepo.SpotPrice(todaysDate, user.Homes[1].PriceArea);
                    chart1.SpotPriceRightNow = spotPriceRepo.SpotPriceRightNow(await spotPricesToday, todaysDate);
                    var lowestPrice = spotPriceRepo.CheapestHour(await spotPricesToday);
                    chart1.CheapestHour = lowestPrice.ToString("HH:mm");
                    
                    HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));
                    SaveSelectedResolution(selector);
                    SaveSelectedHome(home);

                    return View("Index", user.Homes[1].Chart);
                }
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", new { errorMessage = "Det finns ingen data för framtida datum. Vänligen ladda om sidan och välj ett annat datum." });
            }

        }

        /// <summary>
        /// Saves what home is selected so that it remains checked when updating chart.
        /// </summary>
        /// <param name="home">selected home</param>
        public void SaveSelectedHome(string? home)
        {
            if (home == "homeOne" || home == null)
            {
                ViewBag.SelectedHome = "homeOne";
            }
            if (home == "homeTwo")
            {
                ViewBag.SelectedHome = "homeTwo";
            }
        }

        /// <summary>
        /// Initial action-method to create all values for model surronded by a "Try, Catch" to handle failed API-calls
        /// </summary>
        /// <param name="year">Drop downs value year</param>
        /// <param name="month">Drop downs value month</param>
        /// <param name="day">Drop downs value day</param>
        /// <param name="selector">Value of which radio-btn that is checked</param>
        /// <returns>View(chart1)</returns>
        public async Task<IActionResult> Index(string? year, string? month, int day, string? selector)

        {
            try
            {
                User user = CreateUser();
                DateTime todaysDate = new DateTime();
                todaysDate = DateTime.Now;
                DateTime tomorrowsDate = todaysDate.Date.AddDays(1);

                string dayId = AddAZeroAtBeginningOfSingleNumber(day);
                string monthId = AddAZeroAtBeginningOfSingleNumber(ConvertMonthToDate(month));
                string? yearId = year;
                string stringDate = $"{yearId}-{monthId}-{dayId}";

                string dateToShowInInterface = SetDateStringToShow(day, month, year, selector, todaysDate);

                var consumptionEndPointForHomeOne = SetEndPointForConsumptionBySelectedDate(dayId, monthId, yearId, selector, todaysDate, tomorrowsDate, user?.Homes?[0].Contract);
                var consumptionEndPointForHomeTwo = SetEndPointForConsumptionBySelectedDate(dayId, monthId, yearId, selector, todaysDate, tomorrowsDate, user?.Homes?[1].Contract);

                DisplayChartViewModel chart1 = FillChartWithValues(stringDate, selector, consumptionEndPointForHomeOne, dateToShowInInterface, user?.Homes?[0].PriceArea);
                user!.Homes![0].Chart = chart1;
                DisplayChartViewModel chart2 = FillChartWithValues(stringDate, selector, consumptionEndPointForHomeTwo, dateToShowInInterface, user.Homes[1].PriceArea);
                user.Homes[1].Chart = chart2;

                var spotPricesToday = spotPriceRepo.SpotPrice(todaysDate, user.Homes[0].PriceArea);
                chart1.SpotPriceRightNow = spotPriceRepo.SpotPriceRightNow(await spotPricesToday, todaysDate);
                var spotPricesTodayHome2 = spotPriceRepo.SpotPrice(todaysDate, user.Homes[1].PriceArea);
                chart2.SpotPriceRightNow = spotPriceRepo.SpotPriceRightNow(await spotPricesTodayHome2, todaysDate);
                var lowestPrice = spotPriceRepo.CheapestHour(await spotPricesToday);
                chart1.CheapestHour = lowestPrice.ToString("HH:mm");
                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));
                SaveSelectedResolution(selector);
                SaveSelectedHome(null); // SaveSelectedHome: Needs to be 'null' on first startup to automatically select a home (null => home 1 selected)
                return View(chart1); // Default showing view for chart 1 (chart for Home 1)
            }
            catch (Exception)
            {
                return RedirectToAction("Error", new { errorMessage="Systemet kunde inte hantera din förfrågan. Vänligen ladda om sidan."});
            }

        }

        /// <summary>
        /// Creates user with connected homes (one and two)
        /// </summary>
        /// <returns>user</returns>
        public User CreateUser()
        {
            User user = new User() { Homes = new List<Home>() };
            Home homeOne = new Home() { Contract = 37962, Name = "Hem 1 (gamla hemmet)", PriceArea = "SE3", };
            Home homeTwo = new Home() { Contract = 15784, Name = "Hem 2 (nya sommarstugan)", PriceArea = "SE1", };
            user.Homes.Add(homeOne);
            user.Homes.Add(homeTwo);
            return user;
        }

        /// <summary>
        /// Create chart based on indataparameters
        /// </summary>
        /// <param name="dateToShowInInterface"></param>
        /// <param name="labels"></param>
        /// <param name="spotPriceData"></param>
        /// <param name="consumption"></param>
        /// <returns>a generated chart (DisplayChartViewModel)</returns>
        private DisplayChartViewModel CreateChart(string? dateToShowInInterface, string[] labels, string[]? spotPriceData, string[] consumption)
        {
            var chart = new Chart() { Labels = labels, SpotPrice = spotPriceData, Consumption = consumption };
            var model = new DisplayChartViewModel();

            model.Chart = chart;
            model.SelectedDate = dateToShowInInterface;
            return model;
        }

        /// <summary>
        /// Fills chart with values according to dates
        /// </summary>
        /// <param name="stringDate">chosen date in string-form</param>
        /// <param name="selector">chosen date range (day/month/year)</param>
        /// <param name="endPoint">consumption API end point</param>
        /// <param name="dateToShowInInterface">what's showing in interface</param>
        /// <returns>model</returns>
        public DisplayChartViewModel FillChartWithValues(string? stringDate, string? selector, string endPoint, string? dateToShowInInterface, string? homePriceArea)

        {
            Task<List<string>> cons = consumptionRepo.HandleConsumptionAPIData(endPoint, selector);

            Task<string[]> spot = spotPriceRepo.HourlySpotPrice(DateTime.Today, homePriceArea);

            string[] spotPriceData = spot.Result;
            string[] consumption = cons.Result.ToArray();
            string[] labels = SetLabel(selector, DateTime.Today);

            DisplayChartViewModel model = new DisplayChartViewModel();

            model = CreateChart(dateToShowInInterface, labels, spotPriceData, consumption);

            if (selector != null)
            {
                DateTime fullDate = stringDate != null ? DateTime.Parse(stringDate) : DateTime.Today;
                var dbData = new MyDbContext();
                cons = consumptionRepo.HandleConsumptionAPIData(endPoint, selector);
                labels = SetLabel(selector, fullDate);
                // If selected date is BEFORE 1 january 2023 (DB)
                if (SelectedDateIsBeforeBreakDate(fullDate))
                {
                    // MonthlyView
                    if (selector == "viewMonth")
                    {
                        var data = dbData.GetMonthlySpotPricesFromDb(fullDate, homePriceArea);

                        double[] spotPrices = data.Select(x => x.Value).ToArray();
                        spotPriceData = spotPrices.Select(x => x.ToString()).ToArray();

                        model = CreateChart(dateToShowInInterface, labels, spotPriceData, consumption);

                    }
                    // DayView
                    else if (selector == "viewDay")
                    {
                        var data = dbData.GetHourlySpotPriceFromDb(fullDate, homePriceArea);
                        double[] spotPrices = data.Select(x => x.Value).ToArray();
                        spotPriceData = spotPrices.Select(x => x.ToString()).ToArray();

                        model = CreateChart(dateToShowInInterface, labels, spotPriceData, consumption);
                    }
                    // YearlyView
                    else if (selector == "viewYear")
                    {
                        var data = dbData.GetYearlySpotPricesFromDb(fullDate, homePriceArea);
                        double[] spotPrices = data.Select(x => x.Value).ToArray();

                        spotPriceData = spotPrices.Select(x => x.ToString()).ToArray();

                        model = CreateChart(dateToShowInInterface, labels, spotPriceData, consumption);
                    }
                }
                // If selected date is from or AFTER 1 january 2023 (API)
                else
                {
                    if (selector == "viewMonth")
                    {
                        // If time allows make code neater 146 & 159 (change spot/dayView to <double>)
                        Task<List<double>> spot2 = spotPriceRepo.PricePerMonth(fullDate, homePriceArea);
                        string[] spot2Array = spot2.Result.Select(d => d.ToString()).ToArray();
                        model = CreateChart(dateToShowInInterface, labels, spot2Array, consumption);
                    }
                    // DayView
                    else if (selector == "viewDay")
                    {
                        spot = spotPriceRepo.HourlySpotPrice(fullDate, homePriceArea);
                        model = CreateChart(dateToShowInInterface, labels, spotPriceData, consumption);
                    }
                    // YearlyView
                    else if (selector == "viewYear")
                    {
                        Task<List<double>> spot2 = spotPriceRepo.AverageMonthlyPricesPerYear(fullDate, homePriceArea);
                        string[] spot2Array = spot2.Result.Select(d => d.ToString()).ToArray();
                        model = CreateChart(dateToShowInInterface, labels, spot2Array, consumption);

                    }
                }
            }
            return model;
        }

        /// <summary>
        /// Formats how date is showed depending on what selector is chosen.
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        /// <param name="year"></param>
        /// <param name="selector"></param>
        /// <param name="todaysDate"></param>
        /// <returns>date as either "day month year" / "month year" / "year" </returns>
        public string SetDateStringToShow(int day, string? month, string? year, string? selector, DateTime todaysDate)
        {
            var months = new Month();
            string monthstring = months.Months[todaysDate.Month - 1];
            string dateShowing = months.GetDateString(todaysDate.Day, monthstring, todaysDate.Year.ToString());

            if (selector != null)
            {
                switch (selector)
                {
                    case "viewMonth":
                        dateShowing = months.GetMonthString(month, year);
                        break;
                    case "viewDay":
                        dateShowing = months.GetDateString(day, month, year);
                        break;
                    case "viewYear":
                        dateShowing = months.GetYearString(year);
                        break;
                }
            }
            return dateShowing;
        }

        /// <summary>
        /// Sets endpoint for consumption API depending ingoing dates and selection
        /// </summary>
        /// <param name="dayId"></param>
        /// <param name="monthId"></param>
        /// <param name="yearId"></param>
        /// <param name="selector"></param>
        /// <param name="todaysDate"></param>
        /// <param name="tomorrowsDate"></param>
        /// <returns>endPoint</returns>
        public string SetEndPointForConsumptionBySelectedDate(string? dayId, string? monthId, string? yearId, string? selector, DateTime todaysDate, DateTime tomorrowsDate, int? homeContractNumber)
        {
            string yearMonthDayDate = $"{yearId}-{monthId}-{dayId}";
            string yearMonthDate = $"{yearId}-{monthId}-01";
            string yearDate = $"{yearId}-01-01";

            var endPoint = $"Consumptions?from={todaysDate.ToShortDateString()}&to={tomorrowsDate.ToShortDateString()}&contract_nr={homeContractNumber}";

            if (selector != null)
            {

                if (selector == "viewMonth")
                {
                    try
                    {
                        DateTime dateToConvert = DateTime.ParseExact(yearMonthDate, "yyyy-MM-dd", null);
                        endPoint = $"Consumptions?from={dateToConvert.ToShortDateString()}&to={dateToConvert.Date.AddMonths(1).ToShortDateString()}&contract_nr={homeContractNumber}";
                    }
                    catch (ArgumentNullException ex) { Console.WriteLine("Error: " + ex.Message); }
                }
                else if (selector == "viewDay")
                {
                    try
                    {

                        DateTime dateToConvert = DateTime.ParseExact(yearMonthDayDate, "yyyy-MM-dd", null);
                        endPoint = $"Consumptions?from={dateToConvert.ToShortDateString()}&to={dateToConvert.Date.AddDays(1).ToShortDateString()}&contract_nr={homeContractNumber}";

                    }
                    catch (ArgumentNullException ex) { Console.WriteLine("Error: " + ex.Message); }
                }
                else
                {
                    try
                    {
                        DateTime dateToConvert = DateTime.ParseExact(yearDate, "yyyy-MM-dd", null);
                        endPoint = $"Consumptions?from={dateToConvert.ToShortDateString()}&to={dateToConvert.Date.AddYears(1).ToShortDateString()}&contract_nr={homeContractNumber}";
                    }
                    catch (ArgumentNullException ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
            }
            return endPoint;
        }

        /// <summary>
        /// Takes ints and converts them to a 2 digit lenght to fit API-endpoint
        /// </summary>
        /// <param name="date">int</param>
        /// <returns>string</returns>
        public string AddAZeroAtBeginningOfSingleNumber(int? date)
        {
            string? newDate;

            if (date?.ToString().Length < 2)
            {
                newDate = date.ToString()!.PadLeft(2, '0');
                return newDate;
            }
            else return date.ToString()!;

        }

        /// <summary>
        /// Bool to check if date selected is before or after 1 january 2023 (=>Db or API-fetch)
        /// </summary>
        /// <param name="date"></param>
        /// <returns>TRUE if date is before 1 jan 2023 or FALSE if after</returns>
        public bool SelectedDateIsBeforeBreakDate(DateTime date)
        {
            DateTime breakDate = new DateTime(2022, 12, 31);

            // If date selected is before 1 january 2023, fetch data from database
            if (date <= breakDate)
            {
                return true;
            }

            // If date selected is after 1 january 2023, fetch data from API
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Generates string array with labels to show in chart
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="date"></param>
        /// <returns>labels</returns>
        public string[] SetLabel(string? selector, DateTime date)
        {
            int x = 0;
            if (selector == "viewMonth")
            {
                int daysInMonth = System.DateTime.DaysInMonth(date.Year, date.Month);
                x = daysInMonth;
            }
            else if (selector == "viewYear")
            {
                x = 12;
            }
            else
            {
                x = 24;

            }

            string[] labels = new string[x];
            for (int i = 1; i <= x; i++)
            {
                labels[i - 1] = i.ToString();

            }

            return labels;
        }

        /// <summary>
        /// Creates error page
        /// </summary>
        /// <param name="errorMessage">What went wrong in code</param>
        /// <returns>View(new ErrorViewModel)</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Error(string errorMessage)
        {

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage = errorMessage, FormerPage = "Home" });
        }

        /// <summary>
        /// Converts month from string to int
        /// </summary>
        /// <param name="month"></param>
        /// <returns>date as int</returns>
        private int? ConvertMonthToDate(string? month)
        {
            Month months = new Month();
            int? date = Array.IndexOf(months.Months, month) + 1;
            return date;
        }

        /// <summary>
        /// Saves selected resolution in "ViewBag.SelectedResolution".
        /// </summary>
        /// <param name="viewSelector"></param>
        /// <param name="dayId"></param>
        /// <param name="monthId"></param>
        /// <param name="yearId"></param>
        public void SaveSelectedResolution(string? viewSelector)
        {
            if (viewSelector == "viewYear")
            {
                ViewBag.SelectedResolution = "viewYear";
            }
            else if (viewSelector == "viewMonth")
            {
                ViewBag.SelectedResolution = "viewMonth";
            }
            else
            {
                ViewBag.SelectedResolution = "viewDay";
            }

        }
    }
}
