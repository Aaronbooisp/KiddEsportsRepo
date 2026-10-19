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
    /// Events Screen to interact with the SQL Database in the background through the WPF screen UI. This screen and its functions interacts with the Events table of the
    /// KiddEsports database.
    /// </summary>
    public partial class EventsScreen : UserControl
    {
        DataManager dataManager = new DataManager();
        List<Events> eventsList = new List<Events>();
        Events currentEvent;
        bool isNewEntry = true;

        public EventsScreen()
        {
            InitializeComponent();
            RefreshDataGridDisplay();
        }

        private void dgvEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShowSelectedEntry();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveEventEntry();
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

        private void SaveEventEntry()
        {
            if (AreFieldsValid() is false)
            {
                return;
            }

            if (isNewEntry)
            {
                currentEvent = new Events();
            }

            currentEvent.EventName = txtEventName.Text.Trim();
            currentEvent.EventLocation = txtEventLocation.Text.Trim();

            // Checks for a selected date and uses it. Otherwise, default to DateTime.Now if value is null.
            /* 
             * Assigns date selected from the datepicker to the EventDate field in the currentEvent class-wide object. 
             * If calEventDate.SelectedDate is null, then default to the current date (DateTime.Now.).
             * 
             * Realistically it will never have to default to the current date because the AreFieldsValid method already pre-validates the incoming EventDate data and ensures no nulls.
             * 
             * This is mainly here as a failsafe if somehow calEventDate.SelectedDate becomes null (by external handling of 
             * the data or such eg. outside of the UI, directly from database) and also to get rid of the nullable warning(s).
             */
            currentEvent.EventDate = calEventDate.SelectedDate ?? DateTime.Now;

            if (isNewEntry)
            {
                dataManager.CreateEvent(currentEvent);
                MessageBox.Show("Event created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                dataManager.UpdateEvent(currentEvent);
                MessageBox.Show("Event updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ClearFields();
            RefreshDataGridDisplay();
        }
        private void DeleteSelectedEntry()
        {
            if (dgvEvents.SelectedItem is not Events selectedEvent)
            {
                MessageBox.Show("Please select an event to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            if(MessageBox.Show($"Are you sure you want to delete the event '{selectedEvent.EventName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) 
               is MessageBoxResult.Yes)
            {
                dataManager.DeleteEvent(selectedEvent.EventId);
                MessageBox.Show("Event deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
                RefreshDataGridDisplay();
            }
        }
        private void ClearFields()
        {
            txtEventName.Text = string.Empty;
            calEventDate.SelectedDate = null;
            txtEventLocation.Text = string.Empty;

            isNewEntry = true;
        }
        private void RefreshDataGridDisplay()
        {
            eventsList = dataManager.GetAllEvents();
            dgvEvents.ItemsSource = eventsList;

            dgvEvents.IsEnabled = true;
        }
        private void ShowSelectedEntry()
        {
            if (dgvEvents.SelectedItem is Events selectedEvent)
            {
                isNewEntry = false;
                dgvEvents.IsEnabled = false;

                currentEvent = selectedEvent;

                txtEventName.Text = currentEvent.EventName;
                txtEventLocation.Text = currentEvent.EventLocation;
                calEventDate.SelectedDate = currentEvent.EventDate;
            }
        }

        private bool AreFieldsValid()
        {
            if (string.IsNullOrWhiteSpace(txtEventName.Text))
            {
                MessageBox.Show("Event Name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEventLocation.Text))
            {
                MessageBox.Show("Event Location cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (calEventDate.SelectedDate is null)
            {
                MessageBox.Show("Please select a valid Event Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
