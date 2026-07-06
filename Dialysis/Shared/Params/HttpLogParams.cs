namespace Dialysis.Shared.Params
{
    public class HttpLogParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortAsc { get; set; } = false;
        public string? SearchString { get; set; }
        public string? HttpMethod { get; set; }
        public int? StatusCode { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public long? UserId { get; set; }
    }
}
