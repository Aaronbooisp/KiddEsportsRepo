using System;
using System.Collections.Generic;
using System.Text;

namespace DataManagement.Models
{
    public class Games
    {
        public int GameId { get; set; }
        public int CategoryId { get; set; }
        public string GameName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;
    }
}
