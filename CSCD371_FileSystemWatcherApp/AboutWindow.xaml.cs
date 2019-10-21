using System.Windows;

namespace CSCD371_FileSystemWatcherApp
{
    /// <summary>
    /// Interaction logic for About_Menu_Window.xaml
    /// </summary>
    public partial class About_Menu_Window : Window
    {
        public About_Menu_Window()
        {
            InitializeComponent();
        }

        private void CloseAbout_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
