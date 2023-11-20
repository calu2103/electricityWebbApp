using DSU23_G5.ViewModels;

namespace DSU23_G5.Models
{
    public class ErrorViewModel:LayoutViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? ErrorMessage { get; set; }

        public string? FormerPage { get; set; }

    }
}