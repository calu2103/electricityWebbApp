using DSU23_G5.Models;
using DSU23_G5.Models.Dtos;

namespace DSU23_G5.Repositories
{
    public interface IConsumerItems
    {
        
        string[] ConvertPunctuation(string[] hourlySpotPrice);       
        List<ConsumerItems> GetItems(string[] spotPrice, string? selector);
        string OptimalConsumtionInterval(string[] hourlySpotPrice, int operationalTime, string? selector);
        ConsumerItems AddItem(ConsumerItems item);
        void RemoveItem(string itemName);
        double? ConsumptionCost(ConsumerItems item, string[] spotPrice, string? selector);
        void ChangeOperatingTime(string name, int operatingTime);
    }
}
