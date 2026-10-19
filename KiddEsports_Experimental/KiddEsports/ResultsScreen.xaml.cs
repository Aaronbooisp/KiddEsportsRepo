using DataManagement;
using DataManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KiddEsports
{
    /// <summary>
    /// Results Screen to interact with the SQL Database in the background through the WPF screen UI. This screen and its functions are linked to the Results table of the
    /// KiddEsports database.
    /// </summary>
    public partial class ResultsScreen : UserControl
    {
        DataManager dataManager = new DataManager();
        List<ResultDisplayModel> resultsList = new List<ResultDisplayModel>();
        Results currentResult;
        bool isNewEntry = true;
        string sortState = "Unsorted";

        // This will be none by default but will be used to determine how to keep a list in what way.
        // When we need this modified, we will preset its value in the method and immediately use it for comparison.

        public ResultsScreen()
        {
            InitializeComponent();
            RefreshDataGridDisplay();
        }

        private void dgvResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedEntry();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            RefreshDataGridDisplay();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveResultEntry();
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedEntry();
        }

        private void SaveResultEntry()
        {
            if (AreFieldsValid() is false)
            {
                return;
            }

            if (isNewEntry)
            {
                currentResult = new Results();
            }

            currentResult.EventId = Convert.ToInt32(cboEventName.SelectedValue);
            currentResult.GameId = Convert.ToInt32(cboGamePlayed.SelectedValue);
            currentResult.CompetitorId = Convert.ToInt32(cboCompetitor.SelectedValue);

            // Stores the time values entered into the textboxes temporarily to use for the TimeSpan calculation.
            int hours = Convert.ToInt32(txtCompletionHours.Text);
            int minutes = Convert.ToInt32(txtCompletionMinutes.Text);
            int seconds = Convert.ToInt32(txtCompletionSeconds.Text);

            TimeSpan totalTime = new TimeSpan(hours, minutes, seconds);

            currentResult.TotalCompletionTime = totalTime.Ticks;
            if (isNewEntry)
            {
                dataManager.CreateResult(currentResult);
                MessageBox.Show("Result entry saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                dataManager.UpdateResult(currentResult);
                MessageBox.Show("Result entry updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ClearFields();
            RefreshDataGridDisplay();
        }

        /// <summary>
        /// Method to populate the ComboBoxes with the relevant respective data.
        /// </summary>
        private void LoadComboBoxes()
        {
            // Populates the EventName ComboBox
            // Sources all the data in the EventName ComboBox through the dataManager.GetAllEvents method, which returns a list of Event objects.
            cboEventName.ItemsSource = dataManager.GetAllEvents();

            // This only configures the display to what the user sees on the UI screen.
            cboEventName.DisplayMemberPath = "EventName";

            // This is actually hidden but is important to handle logical operations in the background, this acts as a referencer.
            cboEventName.SelectedValuePath = "EventId";

            // Populate GamesPlayed ComboBox
            cboGamePlayed.ItemsSource = dataManager.GetAllGames();
            cboGamePlayed.DisplayMemberPath = "GameName";
            cboGamePlayed.SelectedValuePath = "GameId";

            // Populate Competitors ComboBox
            cboCompetitor.ItemsSource = dataManager.GetAllCompetitors();
            cboCompetitor.DisplayMemberPath = "CompetitorName";
            cboCompetitor.SelectedValuePath = "CompetitorId";
        }
        private void ShowSelectedEntry()
        {
            // Checks that a datagrid selection is a ResultDisplayModel object, and casts it to a locally declared selectedResultDisplay.
            if (dgvResults.SelectedItem is not ResultDisplayModel selectedResultDisplay)
                return;

            // Then set the new entry state to false and disable the datagrid. (Update mode)
            isNewEntry = false;
            dgvResults.IsEnabled = false;

            currentResult = new Results();
            currentResult.ResultId = selectedResultDisplay.ResultId;

            // Converts the Ticks back to a readable TimeSpan format for used to put into the TextBoxes in the next step.
            TimeSpan time = TimeSpan.FromTicks(selectedResultDisplay.TotalCompletionTime);

            // Populate your textboxes.
            txtCompletionHours.Text = ((int)time.TotalHours).ToString();
            txtCompletionMinutes.Text = time.Minutes.ToString();
            txtCompletionSeconds.Text = time.Seconds.ToString();

            // Sets the data in the Textbox and Combobox to whatever is in the selectedGame object.
            cboEventName.SelectedValue = selectedResultDisplay.EventId;
            cboGamePlayed.SelectedValue = selectedResultDisplay.GameId;
            cboCompetitor.SelectedValue = selectedResultDisplay.CompetitorId;
        }
        private void DeleteSelectedEntry()
        {
            // Checks that the selected item in the DataGrid is a ResultDisplayModel object, and maps it to selectedResult. If this check fails, then exit the code early.
            if (dgvResults.SelectedItem is not ResultDisplayModel selectedResult)
            {
                MessageBox.Show("Please select a result to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirm deletion with the user.
            var result = MessageBox.Show($"Are you sure you want to delete the result '{selectedResult.ResultId}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result is MessageBoxResult.Yes)
            {
                dataManager.DeleteResult(selectedResult.ResultId);
                MessageBox.Show("Result entry deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearFields();
                RefreshDataGridDisplay();
            }

        }
        private void ClearFields()
        {
            // Clear the fields in the ResultsScreen.
            cboEventName.SelectedIndex = -1;
            cboGamePlayed.SelectedIndex = -1;
            cboCompetitor.SelectedIndex = -1;
            txtCompletionHours.Text = string.Empty;
            txtCompletionMinutes.Text = string.Empty;
            txtCompletionSeconds.Text = string.Empty;

            isNewEntry = true;
        }
        private void RefreshDataGridDisplay()
        {
            resultsList = dataManager.GetAllResults();

            resultsList = FilterResultsBySearchTerm(resultsList, txtSearch.Text);

            if (sortState is "orderedByGameAndTime")
            {
                resultsList.Sort(CompareByGameAndTime);
            }

            // Then populate the datagrid with whatever branch of resultsList it went through.
            dgvResults.ItemsSource = resultsList;
            LoadComboBoxes();
            dgvResults.IsEnabled = true;
        }

        private bool AreFieldsValid()
        {
            if (cboEventName.SelectedValue is null)
            {
                MessageBox.Show("Event Name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (cboGamePlayed.SelectedValue is null)
            {
                MessageBox.Show("Game Played cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (cboCompetitor.SelectedValue is null)
            {
                MessageBox.Show("Competitor cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (IsValidNumber(txtCompletionHours.Text) is false)
            {
                MessageBox.Show("Please enter a valid number for Hours.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (IsValidNumber(txtCompletionMinutes.Text) is false)
            {
                MessageBox.Show("Please enter a valid non-negative number for Minutes.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (IsValidNumber(txtCompletionSeconds.Text) is false)
            {
                MessageBox.Show("Please enter a valid non-negative number for Seconds.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks for nulls, whitespaces, and negative numbers.
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns>True if all checks pass. Otherwise false if any check fails.</returns>
        private bool IsValidNumber(string inputText)
        {
            // Checks for nulls or whitespace.
            if (string.IsNullOrWhiteSpace(inputText))
            {
                return false;
            }

            // If it fails to parse into a number, it's invalid.
            if (int.TryParse(inputText, out int number) is false)
            {
                return false;
            }

            // If the number is negative, it's invalid.
            if (number < 0)
            {
                return false;
            }

            // Passed all checks!
            return true;
        }

        private void btnOrderByGameAndBestTime_Click(object sender, RoutedEventArgs e)
        {
            // Sets the state to "orderedByGameAndTime" to flag it for future operations. (mostly RefreshDataGridDisplay)
            // We will do this to delegate the work into the RefreshDataGridDisplay method. This makes it easier to manage overall.
            sortState = "orderedByGameAndTime";

            RefreshDataGridDisplay();
        }




        private List<ResultDisplayModel> OrderResultsByGameAndTime(List<ResultDisplayModel> listToSort)
        {
            // Pass the comparison method directly into C#'s built-in Sort
            listToSort.Sort(CompareByGameAndTime);
            return listToSort;
        }

        private int CompareByGameAndTime(ResultDisplayModel baseValue, ResultDisplayModel comparedValue)
        {
            // Compare games alphabetically (case-insensitive).
            // Compares 2 ResulDisplayModel objects in resultsList against each other and returns an int.
            // Assigns an object a number depending on whether they should come earlier in a sorted list or later.
            // -1 = earlier entry, 0 = equal entry, 1 = later entry. It looks at every single letter and compares them alphabetically.
            // For visualisation, 
            // baseValue.AssassinsCreed compared to comparedValue.Battlefield = -1
            // baseValue.Battlefield compared to comparedValue.Battlefield = 0
            // baseValue.Civilisation compared to comparedValue.Bioshock = 1
            int gameComparison = string.Compare(baseValue.GamePlayed, comparedValue.GamePlayed, StringComparison.OrdinalIgnoreCase);

            // If the games being compared is not 0 (-1 or 1, meaning they are different), return that int value immediately.
            if (gameComparison is not 0)
            {
                return gameComparison;
            }

            // The code carries on further here if the games are identical, then compare times using the same process. (fastest to slowest)
            return baseValue.TotalCompletionTime.CompareTo(comparedValue.TotalCompletionTime);
        }





        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            dataManager.WriteDataToFile(resultsList);
            MessageBox.Show("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // EXPERIMENTAL METHOD TO HIDE IRRELEVANT DISPLAYED COLUMN NAMES (Ids, Tick count).
        private void dgvResults_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Automatically cancels column creation for any property ending with "Id"
            if (e.PropertyName.EndsWith("Id"))
            {
                e.Cancel = true;
            }

            if (e.PropertyName.EndsWith("CompletionTime"))
            {
                e.Cancel = true;
            }
        }

        private List<ResultDisplayModel> FilterResultsBySearchTerm(List<ResultDisplayModel> originalList, string filterText)
        {
            // If search text is empty, return the original list as-is
            if (string.IsNullOrWhiteSpace(filterText))
            {
                return originalList;
            }

            // Create a new empty list to hold matching records
            List<ResultDisplayModel> filteredList = new List<ResultDisplayModel>();

            // Clean up the search string once for fair comparison
            string cleanSearchText = filterText.Trim();

            // Loop through every item in the original list
            foreach (ResultDisplayModel item in originalList)
            {
                 bool matchesCompetitor = item.Competitor.Contains(cleanSearchText, StringComparison.OrdinalIgnoreCase);

                 // 2. Check if the GamePlayed matches
                 bool matchesGamePlayed = item.GamePlayed.ToString().Contains(cleanSearchText, StringComparison.OrdinalIgnoreCase);

                // 3. Add item if EITHER field matches
                if (matchesCompetitor || matchesGamePlayed)
                {
                    filteredList.Add(item);
                }
            }

            // 5. Return the new filtered collection
            return filteredList;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshDataGridDisplay();
        }
    }
}
