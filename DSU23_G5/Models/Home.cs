using DSU23_G5.ViewModels;
using System.Reflection.Metadata.Ecma335;

namespace DSU23_G5.Models
{
    public class Home
    {
        public int Contract { get; set; }
        public string? Name { get; set; }
        public DisplayChartViewModel? Chart { get; set; }
        public string? PriceArea { get; set; }
       
    }
}
