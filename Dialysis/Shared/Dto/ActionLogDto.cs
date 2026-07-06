using System;

namespace Dialysis.Shared.Dto
{
    public class ActionLogDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserIp { get; set; }
        public string ActionType { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Description { get; set; }
    }

    public class PagedActionLogResponse
    {
        public List<ActionLogDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
    }
}
