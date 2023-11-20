using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DSU23_G5.Modelsusing
{
    public class SpotPrice : DbContext
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public List<PriceArea>? PriceAreas { get; set; }
        public string? CurrencyText { get; set; }
        public string? Currency { get; set; }
        public string? Resolution { get; set; }

        public override string ToString()
        {
          return $"Id: {Id} Date: {Date.ToUniversalTime()} CurrencyText: {CurrencyText} Currency: {Currency}";
        }
    }
    public class PriceArea
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? Name { get; set; }
        public double Value { get; set; }

        public override string ToString()
        {
            return $"Id: {Id} Name: {Name} Value: {Value}";
        }
    }
    
}
