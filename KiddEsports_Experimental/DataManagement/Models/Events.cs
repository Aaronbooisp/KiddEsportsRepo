using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement.Models
{
    public class Events
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventLocation { get; set; } = string.Empty;
    }
}
