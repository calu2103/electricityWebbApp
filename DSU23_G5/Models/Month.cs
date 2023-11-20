namespace DSU23_G5.Models
{
    public class Month
    {
        public string[] Months = { "januari", "februari", "mars", "april", "maj", "juni", "juli", "augusti", "september", "oktober", "november", "december" };


        public string GetDateString(int? day, string? month, string? year)
        {
            return $"{day} {month} {year}";
        }

        public string GetMonthString(string? month, string? year)
        {
            return $"{month} {year}";
        }

        public string GetYearString(string? year)
        {
            return $"{year}";
        }
    }
}
