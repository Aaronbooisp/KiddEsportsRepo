using DataManagement;
using DataManagement.Models;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.ActiveDirectory;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
    /// Competitors Screen to interact with the SQL Database in the background through the WPF screen UI. This screen and its functions interacts with the Competitors table of the
    /// KiddEsports database.
    /// </summary>
    public partial class CompetitorsScreen : UserControl
    {
        DataManager dataManager = new DataManager();
        List<Competitors> competitorsList = new List<Competitors>();
        Competitors currentCompetitor;
        bool isNewEntry = true;

        public CompetitorsScreen()
        {
            InitializeComponent();
            RefreshDataGridDisplay();
        }

        private void dgvCompetitors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedEntry();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveCompetitorEntry();
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedEntry();
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            RefreshDataGridDisplay();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void SaveCompetitorEntry()
        {
            // Data fields validation check. If the validation check fails, the method will return early and not proceed with saving.
            if (AreFieldsValid() is false)
            {
                return;
            }

            // Otherwise, proceed to either create a new Competitor object or update an existing one based on the isNewEntry state (Validation check passed).
            if (isNewEntry)
            {
                currentCompetitor = new Competitors();
            }

            currentCompetitor.CompetitorName = txtCompetitorName.Text.Trim();
            currentCompetitor.Age = int.Parse(txtAge.Text.Trim());
            currentCompetitor.Phone = txtContactPhone.Text.Trim();
            currentCompetitor.Email = txtContactEmail.Text.Trim();

            if (isNewEntry)
            {
                dataManager.CreateCompetitor(currentCompetitor);
                MessageBox.Show("Competitor entry saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                dataManager.UpdateCompetitor(currentCompetitor);
                MessageBox.Show("Competitor entry updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ClearFields();
            RefreshDataGridDisplay();
        }
        private void DeleteSelectedEntry()
        {
            // Checks for an existing selection in the DataGrid.
            /*
             *  Checks that the selected item in the DataGrid is a valid Competitors object, and binds it to the locally declared selectedCompetitor within the DeleteSelectedEntry method. 
             *  If this check fails, then exit the code early and display a message box to inform the user that they need to select an entry to delete. 
             */ 
            if (dgvCompetitors.SelectedItem is not Competitors selectedCompetitor)
            {
                MessageBox.Show("Please select a competitor entry to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete the competitor '{selectedCompetitor.CompetitorName}?'", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question)
                is MessageBoxResult.Yes)
            {
                currentCompetitor = selectedCompetitor;
                dataManager.DeleteCompetitor(currentCompetitor.CompetitorId);
                MessageBox.Show("Competitor entry deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
                RefreshDataGridDisplay();
            }
        }
        private void ClearFields()
        {
            txtCompetitorName.Text = string.Empty;
            txtAge.Text = string.Empty;
            txtContactPhone.Text = string.Empty;
            txtContactEmail.Text = string.Empty;

            isNewEntry = true;
        }
        private void RefreshDataGridDisplay()
        {
            // Populates DataGrid with data from the database & enables the DataGrid afterwards (Competitors Table).
            /* 
             * Populates the competitorsList with Competitors objects retrieved from the database through the DataManager class, 
             * and then binds this list to the DataGrid's ItemsSource property to display the data and enables the DataGrid afterwards. 
             * 
             * Direct assignment is also possible ( dgvCompetitors.ItemsSource = dataManager.GetAllCompetitors(); ).
             */
            competitorsList = dataManager.GetAllCompetitors();
            dgvCompetitors.ItemsSource = competitorsList;
            
            dgvCompetitors.IsEnabled = true;
        }
        private void ShowSelectedEntry()
        {
            // Checks if there is a selected item.
            /*  
                Checks that the selected item in the DataGrid is a valid Competitors object, and binds it to the locally declared selectedCompetitor within the ShowSelectedEntry method. 
                If this check fails, then exit the code early. Otherwise run the code below, begin by setting the isNewEntry state to false and disable the DataGrid. 
            */
            if (dgvCompetitors.SelectedItem is not Competitors selectedCompetitor)
                return;

            isNewEntry = false;
            dgvCompetitors.IsEnabled = false;

            // Assigns the local variable to the class-wide variable. This allows it to be used for other operations.
            /* 
             * Assigns the locally declared selectedCompetitor variable to the class-wide currentCompetitor variable, 
             * so that it can be used in other methods like SaveCompetitorEntry and DeleteSelectedEntry.
             * 
             * This is important otherwise there will be logical errors in other places it is being used.
             * e.g. In SaveCompetitorEntry, it will treat the state as isNewEntry = false, and skip instantiating a new Competitor object based on the SaveCompetitorEntry method logic.  
             * Without the assignment, the code eventually try to reference a null currentCompetitor object, which will then throw a NullReferenceException error. 
             */
            currentCompetitor = selectedCompetitor;

            txtCompetitorName.Text = selectedCompetitor.CompetitorName;
            txtAge.Text = selectedCompetitor.Age.ToString();
            txtContactPhone.Text = selectedCompetitor.Phone;
            txtContactEmail.Text = selectedCompetitor.Email;
        }

        private bool AreFieldsValid()
        {
            // Validate Name (Empty check).
            if (string.IsNullOrWhiteSpace(txtCompetitorName.Text))
            {
                MessageBox.Show("Competitor Name cannot be empty.", "Missing Field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate Age (Empty check) & (Numeric check). 
            // If either fails, exits the method early and return a false value.
            if (int.TryParse(txtAge.Text, out _) is false)
            {
                MessageBox.Show("Please enter a valid age (numbers only).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate Phone (Empty check) & (Format check) through the use of the IsValidPhoneNumber method.
            if (IsValidPhoneNumber(txtContactPhone.Text) is false)
            {
                MessageBox.Show("Please enter a valid phone number between 8 and 15 characters.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validate Email (Empty check) & (Format check) through the use of the IsValidEmail method.
            if (IsValidEmail(txtContactEmail.Text) is false)
            {
                MessageBox.Show("Please enter a valid email address (e.g., name@domain.com).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // If all validations pass, return a true value,
            // allowing whatever method that is calling ValidateFields to evaluate the result and proceed further.
            return true;
        }
        /// <summary>
        /// Checks for a valid email format.
        /// 
        /// This method checks for whitespace, trims the input, and then uses the System.Net.Mail.MailAddress class to validate the email format.
        /// 
        /// The Email validation requires a seperate method because it uses a more complex logic to ensure that the email format is correct,
        /// which is not as straightforward as just simply checking for empty or numeric values.
        /// </summary>
        /// <param name="email"></param>
        /// <returns>True if the email is a valid format, otherwise false</returns>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string cleanEmail = email.Trim();

            // Checks if the entered email parses out successfully, then evaluate true or false based on the outcome.
            if (System.Net.Mail.MailAddress.TryCreate(cleanEmail, out var parsedEmail) is false)
            {
                return false;
            }

            // Returns true or false depending on the result of this conditional check.
            return parsedEmail.Address == cleanEmail;
        }
        /// <summary>
        /// Checks that the phone number is a valid format.
        /// 
        /// This method checks for whitespace, trims the input, and then uses a regular expression to validate the phone number format (using System.Text.RegularExpressions).
        /// 
        /// The Phone validation requires a seperate method because it uses a more complex logic to ensure that the phone number format is correct,
        /// which is not as straightforward as just simply checking for empty or numeric values.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns>True if validation check passes, otherwise false.</returns>
        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Check if empty or null.
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Trim the phone number to remove any leading or trailing whitespace after the initial check, ensuring that the regex pattern is applied to a clean string.
            string cleanPhoneNumber = phoneNumber.Trim();

            // Pattern check
            // Allows: Optional '+' at start, followed by digits, spaces, hyphens, or parentheses
            // Length check: Between 8 and 15 allowed characters (e.g., Australian 10-digit formats)
            string phonePattern = @"
                                  ^                   # Start of string
                                  \+?                 # Optional leading '+' for international codes
                                  [0-9\s\-\(\)]{8,15} # 8 to 15 digits, spaces, hyphens, or parentheses
                                  $                   # End of string";

            return Regex.IsMatch(cleanPhoneNumber, phonePattern, RegexOptions.IgnorePatternWhitespace);
        }
        
    }
}
