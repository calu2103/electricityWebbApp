using DSU23_G5.Models.Dtos;
using DSU23_G5.Modelsusing;

namespace DSU23_G5.Repositories
{
    public interface ISpotPriceRepo
    {
        double AverageDailySpotPrice(CurrentSpotPriceDto[] todaysSpotPrice);
        Task<string[]> HourlySpotPrice(DateTime date, string? priceArea);
        Task<List<double>> PricePerMonth(DateTime date, string? priceArea);
        Task<List<double>> AverageMonthlyPricesPerYear(DateTime date, string? priceArea);
        Task<CurrentSpotPriceDto[]> SpotPrice(DateTime date, string? homePriceArea);
        string SpotPriceRightNow(CurrentSpotPriceDto[] todaysSpotPrice, DateTime currentHour);
        DateTime CheapestHour(CurrentSpotPriceDto[] todaysSpotPrice);
    }
}
