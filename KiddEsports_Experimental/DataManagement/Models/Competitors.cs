using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement.Models
{
    public class Competitors
    {
        public int CompetitorId { get; set; }
        public string CompetitorName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
