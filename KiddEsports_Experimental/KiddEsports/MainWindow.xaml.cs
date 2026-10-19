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

namespace KiddEsports
{
    /// <summary>
    /// Main underlying window that will be used to swap between the 4 other screens.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            mainControl.Content = new CompetitorsScreen();
            btnCompetitors.IsEnabled = false;
        }

        private void btnCompetitors_Click(object sender, RoutedEventArgs e)
        {
            mainControl.Content = new CompetitorsScreen();

            btnCompetitors.IsEnabled = false;
            btnEvents.IsEnabled = true;
            btnGames.IsEnabled = true;
            btnResults.IsEnabled = true;
        }
        private void btnEvents_Click(object sender, RoutedEventArgs e)
        {
            mainControl.Content = new EventsScreen();

            btnCompetitors.IsEnabled = true;
            btnEvents.IsEnabled = false;
            btnGames.IsEnabled = true;
            btnResults.IsEnabled = true;
        }
        private void btnGames_Click(object sender, RoutedEventArgs e)
        {
            mainControl.Content = new GamesScreen();

            btnCompetitors.IsEnabled = true;
            btnEvents.IsEnabled = true;
            btnGames.IsEnabled = false;
            btnResults.IsEnabled = true;
        }
        private void btnResults_Click(object sender, RoutedEventArgs e)
        {
            mainControl.Content = new ResultsScreen();

            btnCompetitors.IsEnabled = true;
            btnEvents.IsEnabled = true;
            btnGames.IsEnabled = true;
            btnResults.IsEnabled = false;
        }

    }
}