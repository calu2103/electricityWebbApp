using DSU23_G5.Models.Dtos;

namespace DSU23_G5.Repositories
{
    public interface IConsumptionRepo
    {
        List<string> CalculateDailyConsumption(List<double> _hourValues);
        List<string> CalculateMonthlyConsumption(List<double> _hourValues, DateTime fromTime);
        Task<List<string>> HandleConsumptionAPIData(string? endpoint, string? selector);
        List<string> HourlyConsumption(List<double> _hourValues);
    }
}
