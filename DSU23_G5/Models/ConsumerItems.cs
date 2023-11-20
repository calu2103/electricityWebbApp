namespace DSU23_G5.Models
{
    public class ConsumerItems
    {
        public string? Name { get; set; }
        public int OperatingTime { get; set; }
        public string? OptimalStartTime { get; set; }
        public double? KWh { get; set; }
        public double? Cost { get; set; }
        public ConsumerItems(string name, double? kWh, int operatingTime)

        {
            Name = name;
            KWh = kWh;
            OperatingTime = operatingTime;
        }

    }
}
