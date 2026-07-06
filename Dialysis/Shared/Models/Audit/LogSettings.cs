using System;

namespace Dialysis.Shared.Models
{
    public class LogSettings
    {
        public long Id { get; set; }
        public bool LogHttpEnabled { get; set; } = true;
        public string LoggedMethods { get; set; } = "POST,PUT,PATCH,DELETE";
        public bool LogActionEnabled { get; set; } = true;
        public int MaxBodyLength { get; set; } = 10000;
        public int RetentionDays { get; set; } = 30;
    }
}
