using DSU23_G5.Models;

namespace DSU23_G5.ViewModels
{
    public class DisplayChartViewModel:LayoutViewModel
    {
        public Chart? Chart { get; set; }
        
        public string? SelectedDate { get; set; }

    }
}
