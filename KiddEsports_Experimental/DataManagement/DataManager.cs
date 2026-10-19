using Dapper;
using DataManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DataManagement
{
    public class DataManager
    {
        #region Competitors
        public List<Competitors> GetAllCompetitors()
        {
            string query = "SELECT * FROM Competitors";

            using var connection = ConnectionBuilder.GetSqlConnection();

            // Be careful not to do additional operations after the connection.Query because the SQL connection is technically still open. But in this case the method ends immediately after and the connection closes right after.
            return connection.Query<Competitors>(query).ToList();
        }
        public void CreateCompetitor(Competitors competitor)
        {
            string query = "INSERT INTO Competitors (CompetitorName, Age, Phone, Email) VALUES (@CompetitorName, @Age, @Phone, @Email)";

            using var connection = ConnectionBuilder.GetSqlConnection();
            connection.Execute(query, competitor);
        }
        public void UpdateCompetitor(Competitors competitor)
        {
            string query = "UPDATE Competitors SET CompetitorName = @CompetitorName, Age = @Age, Phone = @Phone, Email = @Email WHERE CompetitorID = @CompetitorID";

            using var connection = ConnectionBuilder.GetSqlConnection();
            connection.Execute(query, competitor);
        }
        public void DeleteCompetitor(int competitorId)
        {
            string query = "DELETE FROM Competitors WHERE CompetitorID = @CompetitorID";

            using var connection = ConnectionBuilder.GetSqlConnection();
            var param = new { CompetitorID = competitorId };
            connection.Execute(query, param);
        }
        #endregion

        #region Events
        public List<Events> GetAllEvents()
        {
            string query = "SELECT * FROM Events";

            using var connection = ConnectionBuilder.GetSqlConnection();
            return connection.Query<Events>(query).ToList();
        }
        public void CreateEvent(Events eventEntry)
        {
            string query = "INSERT INTO Events (EventName, EventDate, EventLocation) VALUES (@EventName, @EventDate, @EventLocation)";

            using var connection = ConnectionBuilder.GetSqlConnection();
            connection.Execute(query, eventEntry);
        }
        public void UpdateEvent(Events eventEntry)
        {
            string query = "UPDATE Events SET EventName = @EventName, EventDate = @EventDate, EventLocation = @EventLocation WHERE EventID = @EventID";

            using var connection = ConnectionBuilder.GetSqlConnection();
            connection.Execute(query, eventEntry);
        }
        public void DeleteEvent(int eventId)
        {
            string query = "DELETE FROM Events WHERE EventID = @EventID";

            using var connection = ConnectionBuilder.GetSqlConnection();
            var param = new { EventID = eventId };
            connection.Execute(query, param);
        }
        #endregion

        #region Games
        public List<Games> GetAllGames()
        {
            using var connection = ConnectionBuilder.GetSqlConnection();

            string query = "SELECT GameID, Games.CategoryID, GameName, Categories.CategoryName FROM Games INNER JOIN Categories ON Games.CategoryID = Categories.CategoryID";
            return connection.Query<Games>(query).ToList();
        }

        public Games GetGameById(int id)
        {
            //The query to find our required item using its ID
            string query = $"SELECT * FROM Categories Where CategoryID = {id}";
            //Manages the connecion to our database and then closes and disposes of it once we are done.
            using (var connection = ConnectionBuilder.GetSqlConnection())
            {
                //Runs Dapper's query single operation and then maps the results into our Item models
                return connection.QuerySingle<Games>(query);
            }
        }

        public void CreateGame(Games games)
        {
            string query = "INSERT INTO Games (GameName, CategoryId) VALUES (@GameName, @CategoryId)";

            using var connection = ConnectionBuilder.GetSqlConnection();
            connection.Execute(query, games);
        }
        public void UpdateGame(Games games)
        {
            string query = "UPDATE Games SET GameName = @GameName, CategoryId = @CategoryId WHERE GameId = @GameId";

            using var connection = ConnectionBuilder.GetSqlConnection();
            connection.Execute(query, games);
        }
        public void DeleteGame(int gameId)
        {
            string query = "DELETE FROM Games WHERE GameId = @GameId";

            using var connection = ConnectionBuilder.GetSqlConnection();
            var param = new { GameId = gameId };
            connection.Execute(query, param);
        }

        #endregion

        #region Results
        public List<ResultDisplayModel> GetAllResults()
        {
            string query = "SELECT Results.ResultId, Results.EventId, Results.GameId, Results.CompetitorId, Events.EventName, Games.GameName AS GamePlayed, Competitors.CompetitorName AS Competitor, Results.TotalCompletionTime " +
                            "FROM Results " +
                            "INNER JOIN Events ON Results.EventId = Events.EventId " +
                            "INNER JOIN Games ON Results.GameId = Games.GameId " +
                            "INNER JOIN Competitors ON Results.CompetitorId = Competitors.CompetitorId";

            using var connection = ConnectionBuilder.GetSqlConnection();

            // Returns the data retrieved from the SQL query and puts them into a list of ResultDisplayModel objects.
            return connection.Query<ResultDisplayModel>(query).ToList();
        }

        public void CreateResult(Results result)
        {
            string query = "INSERT INTO Results (EventId, GameId, CompetitorId, TotalCompletionTime) VALUES (@EventId, @GameId, @CompetitorId, @TotalCompletionTime)";

            using var connection = ConnectionBuilder.GetSqlConnection();
            connection.Execute(query, result);
        }

        public void UpdateResult(Results result)
        {
            // This is where we set our SQL query to query the database.
            /* 
             * Update the results table, SET the EventId in the SQL table to their respective Ids from within the result object. Data from here is taken from the user inputs in the UI and passed through the Results object acting as an intermediary.
             * EventId = @EventId is basically SET EventId in the SQL table, to the @EventId in the result object. EventId = SQLColumn. @EventId = result object. (parameterized).
             * UI User data inputs -> UI logical function and passes it -> Results -> result 
             */
            string query = "UPDATE Results SET EventId = @EventId, GameId = @GameId, CompetitorId = @CompetitorId, TotalCompletionTime = @TotalCompletionTime WHERE ResultId = @ResultId";

            using var connection = ConnectionBuilder.GetSqlConnection();
            connection.Execute(query, result);
        }

        public void DeleteResult(int resultId)
        {
            string query = "DELETE FROM Results WHERE ResultID = @ResultID";

            using var connection = ConnectionBuilder.GetSqlConnection();
            var param = new { ResultID = resultId };
            connection.Execute(query, param);
        }

        string fileName = "OrderedResults.csv";
        public void WriteDataToFile(List<ResultDisplayModel> results)
        {
            //Create a new streamwriter within our using statement. This will allow us to use
            //the writer and ensure the file connection is closed properly once we are done.
            using (var writer = new StreamWriter(fileName))
            {
                //Use a try/catch to manage if there is an issue with interacting with the file
                try
                {
                    foreach (var result in results)
                    {
                        //Write the string in comma-delimyted format using string interpolation
                        //E.g Karlie Cross, Cyberpunk 2077, Speed_Con_2026
                        // Note that leaving it as is will display the CompletedTime in huge numbers such as 11000000. This is because it is stored in raw Ticks.
                        // We would have to explicitly convert the raw Ticks back into a readable Timespan format before we save it to the file.
                        // Note on this, since it is saved to the csv file as a TimeSpan format, when reading back from file it HAS to be converted back into a long value.
                        writer.WriteLine($"{result.Competitor}, {result.GamePlayed}, {result.EventName}, {result.CompletedTime}");
                    } 
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occured when writing: " + ex.Message);
                }
            }
        }

        // We will not focus on the reading part for now because it is quite complex considering all the mapping required and making a csv file human readable.
        // Such as deciding if we should have the Ids and Ticks saved in the csv file, how will it interact with the result screen, how will it affect the links to the other 3 tables, etc.
        // Currently we just save the Competitor data fields out into a csv file without the Ids, which means that reading it back will cause complications because when it gets read back,
        // the needed Ids it uses for the referential links are not present anymore.
        public List<ResultDisplayModel> ReadDataFromFile()
        {
            //Create a new streamreader within our using statement. This will allow us to use
            //the reader and ensure the file connection is closed properly once we are done.
            using (var reader = new StreamReader(fileName))
            {
                //Create a new list of ResultDisplayModel Objects to store each entry as they are read from the file
                List<ResultDisplayModel> resultEntries = new List<ResultDisplayModel>();
                try
                {
                    string line;
                    //Read the next line for the file and process our loop if the line is not null
                    while ((line = reader.ReadLine()) is not null)
                    {
                        //Split the line where all the comas are and put all the searate pieces between
                        //them in separate indexes of a string array
                        string[] temp = line.Split(", ");
                        //Create a new ResultDisplayModel to hold the data from our current line
                        ResultDisplayModel newResult = new ResultDisplayModel();
                        //Copy each index value from the temp[] to the ResultDisplayModel object.
                        //Make sure they are done in the same order as they were written to file.
                        newResult.Competitor = temp[0];
                        newResult.GamePlayed = temp[1];
                        newResult.EventName = temp[2];

                        // Using long here works fine, but it will be displayed in an unreadable Tick format. we need to explicitly convert Tick back into a TimeSpan format.
                        newResult.TotalCompletionTime = long.Parse(temp[3]);
                        //Add the completed result to the list
                        resultEntries.Add(newResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occured when reading: " + ex.Message);
                }
                //Return the list back to where the method was called from.
                return resultEntries;
            }
        }

        #endregion

        #region Categories

        public List<Category> GetAllCategories()
        {
            //The query we wil execute to get all our records from the DB.
            string query = "SELECT * FROM Categories";

            //Manages the conecion to our databse and then closes and disposes of it once we are done.
            using (var connection = ConnectionBuilder.GetSqlConnection())
            {
                //Runs Dapper's query operation and then maps the results into our Category models 
                //before returning them as a list.
                return connection.Query<Category>(query).ToList();
            }
        }

        #endregion
    }
}
