using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

// We need 2 models here because 1 is to handle strictly logical functions and the other can be used for logical function or display purposes.
namespace DataManagement.Models
{
    // The model we will use for logical functions. It contains all the relevant values, such as the main primary key and the relevant foreign keys. This is a reflection of the database data.
    public class Results
    {
        public int ResultId { get; set; }
        public int EventId { get; set; }
        public int GameId { get; set; }
        public int CompetitorId { get; set; }

        // The reason a long is used here is because I swapped over to using bigint in the SQL database.
        // I swapped to bigint because initially I used time and it capped out at 24 hours and would give errors for any entries above 24 hours.
        // Moved over to using raw Ticks and storing the raw total Ticks into the bigint in the database to allow for entries above 24 hours. This fixes the issue,
        // and when we need it to be readable we just explicitly convert the Ticks back to a TimeSpan format.
        //
        // You could use a string/varchar combination, but this becomes a problem later on when you want to do mathematical functions on it, such as ordering.
        public long TotalCompletionTime { get; set; }
    }

    // The model we will use for display purposes. It still contains the ResultId in there because it is relevant for data retrieval but other than that the other fields are used mostly for data display and a little interaction.
    public class ResultDisplayModel
    {
        public int ResultId { get; set; }
        public int EventId { get; set; }
        public int GameId { get; set; }
        public int CompetitorId { get; set; }

        // Relevant fields that will be displayed linked to its associated Ids.
        public string Competitor { get; set; } = string.Empty;
        public string GamePlayed { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;

        public long TotalCompletionTime { get; set; }

        // Important to convert the returned data into a readable TimeSpan format. Otherwise we would just be reading raw Ticks.
        public string CompletedTime
        {
            get
            {
                TimeSpan tsFormat = TimeSpan.FromTicks(TotalCompletionTime);

                // Omit the hours if the result is under 1 hour. Makes the entries easier to read.
                if (tsFormat.TotalHours < 1)
                {
                    return string.Format("{0:D2}:{1:D2}", tsFormat.Minutes, tsFormat.Seconds);
                }

                // This part is needed to prevent the hours from spilling over into days the moment it hits 24. ( (int)tsFormat.TotalHours )
                // It allows the hours to keep going past 24. We need this here because we aren't actively tracking or displaying days anywhere.
                return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)tsFormat.TotalHours, tsFormat.Minutes, tsFormat.Seconds);
            }
        }
    }
}