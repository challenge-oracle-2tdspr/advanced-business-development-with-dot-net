using System;

namespace AgroTech.Application.DTOs
{
    public class SensorSearchDTO
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public DateTime? StartTimestamp { get; set; }
        public DateTime? EndTimestamp { get; set; }

        public string OrderBy { get; set; } = "timestamp";
        public string Direction { get; set; } = "desc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}