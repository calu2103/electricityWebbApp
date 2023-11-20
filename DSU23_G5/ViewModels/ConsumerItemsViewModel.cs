using DSU23_G5.Models;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Identity.Client;

namespace DSU23_G5.ViewModels
{
    public class ConsumerItemsViewModel:LayoutViewModel
    {
        public List<ConsumerItems>? Items { get; set; }

        public ConsumerItemsViewModel()
        {
        }

    }

}


