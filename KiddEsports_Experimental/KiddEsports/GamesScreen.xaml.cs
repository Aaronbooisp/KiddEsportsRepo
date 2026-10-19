using DataManagement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataManagement.Models;

namespace KiddEsports
{
    /// <summary>
    /// Games Screen to interact with the SQL Database in the background through the WPF screen UI. This screen and its functions interacts with the Games table of the
    /// KiddEsports database.
    /// </summary>
    public partial class GamesScreen : UserControl
    {
        DataManager dataManager = new DataManager();
        List<Category> categoryList = new List<Category>();
        List<Games> gamesList = new List<Games>();
        Games currentGame;
        bool isNewEntry = true;

        public GamesScreen()
        {
            InitializeComponent();
            SetupCategoryComboBox();
            RefreshDataGridDisplay();
        }

        private void dgvGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedEntry();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveGameEntry();
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedEntry();
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void SaveGameEntry()
        {
            // Validates data fields first. If any validation fails, the method will return early and not proceed with saving.
            if (AreFieldsValid() is false)
            {
                return;
            }

            if (isNewEntry)
            {
                currentGame = new Games();
            }

            currentGame.GameName = txtGameName.Text.Trim();
            currentGame.CategoryId = (int)cboGameCategory.SelectedValue;

            if (isNewEntry)
            {
                dataManager.CreateGame(currentGame);
                MessageBox.Show("Game entry saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                dataManager.UpdateGame(currentGame);
                MessageBox.Show("Game entry updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ClearFields();
            RefreshDataGridDisplay();
        }
        private void DeleteSelectedEntry()
        {
            // Checks that the selected item in the DataGrid is a Games object, and binds it to selectedGame. If this check fails, then exit the code early.
            if (dgvGames.SelectedItem is not Games selectedGame)
            {
                MessageBox.Show("Please select a game to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirm deletion with the user.
            var result = MessageBox.Show($"Are you sure you want to delete the game '{selectedGame.GameName}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result is MessageBoxResult.Yes)
            {
                dataManager.DeleteGame(selectedGame.GameId);
                MessageBox.Show("Game entry deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
                RefreshDataGridDisplay();
            }

        }
        private void ClearFields()
        {
            txtGameName.Text = string.Empty;
            cboGameCategory.SelectedIndex = -1;

            dgvGames.IsEnabled = true;
            isNewEntry = true;
        }

        private void SetupCategoryComboBox()
        {
            //Request all our categories from our data manager and store them in our list
            categoryList = dataManager.GetAllCategories();
            //Pass the list as the item source of the combo box to set its content
            cboGameCategory.ItemsSource = categoryList;
            //Tells the combo box which property form the models need to be displayed in 
            //each combo box option in the dropdown.
            cboGameCategory.DisplayMemberPath = "CategoryName";
            //Tells the combo box which property to set in its hidden identifier field for each entry
            cboGameCategory.SelectedValuePath = "CategoryId";

        }

        private void RefreshDataGridDisplay()
        {
            // Populates the datagrid with data from the GetAllGames method. 
            // Direct assignment is also available ( dgvGames.ItemsSource = dataManager.GetAllGames(); ). This however limits it to use only within the method.
            gamesList = dataManager.GetAllGames();
            dgvGames.ItemsSource = gamesList;
     
            dgvGames.IsEnabled = true;
        }
        private void ShowSelectedEntry()
        {
            // Checks that the selected item in the DataGrid is a Games object, and binds it to selectedGame. If this check fails, then exit the code early.
            if (dgvGames.SelectedItem is not Games selectedGame)
                return;

            // Otherwise, set the new entry state to false and disable the datagrid (Update mode).
            isNewEntry = false;
            dgvGames.IsEnabled = false;

            // Assign local selectedGame to the class-wide currentGame variable, so that it can be used in other methods like SaveGameEntry and DeleteSelectedEntry.
            currentGame = new Games();
            currentGame.GameId = selectedGame.GameId;

            // Sets the data in the Textbox and Combobox to whatever is in the selectedGame object.
            txtGameName.Text = selectedGame.GameName;
            cboGameCategory.SelectedValue = selectedGame.CategoryId;
        }

        private bool AreFieldsValid()
        {
            if (string.IsNullOrWhiteSpace(txtGameName.Text))
            {
                MessageBox.Show("Game Name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (cboGameCategory.SelectedItem is null)
            {
                MessageBox.Show("Please select a Game Category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
