using DSU23_G5.Models;
using DSU23_G5.Repositories;
using DSU23_G5.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace DSU23_G5.Controllers
{
    public class ConsumerItemsController : Controller
    {
        private readonly ISpotPriceRepo spotPriceRepo;
        private readonly IConsumerItems consumerItemsRepo;

        public ConsumerItemsController(ISpotPriceRepo spotPriceRepo, IConsumerItems consumerItemsRepo)
        {
            this.spotPriceRepo = spotPriceRepo;
            this.consumerItemsRepo = consumerItemsRepo;
        }

        public async Task<IActionResult> Index(string? selector)
        {
            try
            {
                DateTime date = GetDateForOptimalTime(selector);

                DateTime todaysDate = DateTime.Now;
                var spotPrice = await spotPriceRepo.HourlySpotPrice(date, "SE3");
                var consumerItems = consumerItemsRepo.GetItems(spotPrice, selector);

                var model = new ConsumerItemsViewModel();
                model.Items = consumerItems;

                var spotPricesToday = spotPriceRepo.SpotPrice(date, "SE3");
                var lowestPrice = spotPriceRepo.CheapestHour(await spotPricesToday);
                model.SpotPriceRightNow = spotPriceRepo.SpotPriceRightNow(await spotPricesToday, todaysDate);
                model.CheapestHour = lowestPrice.ToString("HH:mm");
                foreach (var item in model.Items)
                {
                    item.Cost = consumerItemsRepo.ConsumptionCost(item, spotPrice, selector);
                }

                return View(model);
            }

            catch(Exception)
            {
                return RedirectToAction("Error", new {errorMessage="Systemet kunde inte hantera din förfrågan. Det kan bero på att spotpriserna för morgondagen ännu inte är tillgängliga. Vänligen uppdatera sidan och försök igen senare."});
            }

        }

        /// <summary>
        /// Gets the date for API-call that gets hourly spotprices
        /// </summary>
        /// <param name="selector"></param>
        /// <returns>Datetime date, either todays date or tomorrows date</returns>
        public DateTime GetDateForOptimalTime(string? selector)
        {
            DateTime date;

            if (selector != null)
            {
                if (selector == "viewToday")
                {
                    return date = DateTime.Now;
                }

                else
                {
                    return date = DateTime.Today.AddDays(1);
                }

            }
            else
            {
                return date = DateTime.Now;
            }
        }

        /// <summary>
        /// Creates new consumer item object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="kWh"></param>
        /// <param name="operatingTime"></param>
        /// <returns>Index page</returns>
        public IActionResult CreateItem(string name, int kWh, int operatingTime)
        {
            ConsumerItems item = new ConsumerItems(name, kWh, operatingTime);
            consumerItemsRepo.AddItem(item);

            return RedirectToAction(nameof(Index));
        }


        public ActionResult Error(string errorMessage)

        {
            return View(new ErrorViewModel{ RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, ErrorMessage=errorMessage, FormerPage="ConsumerItems" });
        }

        /// <summary>
        /// Removes consumer item
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Index page</returns>
        public IActionResult RemoveItem(string name)
        {
            consumerItemsRepo.RemoveItem(name);
            return RedirectToAction(nameof(Index));

        }

        /// <summary>
        /// Edits a consumer item's operating time to a chosen value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="operatingTime"></param>
        /// <returns>Index page</returns>
        public IActionResult EditItem(string name, int operatingTime)
        {
            consumerItemsRepo.ChangeOperatingTime(name, operatingTime);
            return RedirectToAction(nameof(Index));
        }
        
    }
}
