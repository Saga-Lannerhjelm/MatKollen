using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MatKollen.Models
{
    public class MeasurementUnit
    {
        public int Id { get; set; }
        public required string Unit { get; set; }
        public double Multiplier { get; set; }
        public string? Type { get; set; }
    }
}