namespace DSU23_G5.Models.Dtos
{
    public class CurrentSpotPriceDto
    {
        public double SEK_per_kWh { get; set; }
        public double EUR_per_kWh { get; set; }
        public double EXR { get; set; }
        public DateTime Time_Start { get; set; }
        public DateTime Time_End { get; set; }

        public override string ToString()
        {
            return $"Sek_per_kwh: {SEK_per_kWh} EUR_per_kwh: {EUR_per_kWh} EXR: {EXR} time_start: {Time_Start} time_end: {Time_End}";
        }
    }
    
}
