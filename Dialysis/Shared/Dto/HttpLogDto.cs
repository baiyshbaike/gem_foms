using System;

namespace Dialysis.Shared.Dto
{
    public class HttpLogDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string HttpMethod { get; set; }
        public string? Path { get; set; }
        public string? QueryString { get; set; }
        public string? RequestHeaders { get; set; }
        public string? RequestBody { get; set; }
        public int StatusCode { get; set; }
        public string? ResponseHeaders { get; set; }
        public string? ResponseBody { get; set; }
        public long DurationMs { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
    }

    public class PagedHttpLogResponse
    {
        public List<HttpLogDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
    }
}
