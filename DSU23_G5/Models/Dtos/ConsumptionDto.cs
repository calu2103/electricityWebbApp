namespace DSU23_G5.Models.Dtos
{
    public class ConsumptionDto
    {
            public MetaDto? _Meta { get; set; }
            public int Contract_Nr { get; set; }
            public DateTime Date_From { get; set; }
            public DateTime Date_To { get; set; }
            public string? Resolution { get; set; }
            public string? Unit { get; set; }
            public List<double>? Values { get; set; }

            public class MetaDto
            {
                public int Total_Records { get; set; }
                public int Total_Days { get; set; }
            }
        
    }
}
