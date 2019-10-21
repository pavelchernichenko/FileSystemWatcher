
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;

namespace CSCD371_FileSystemWatcherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private List<EventModel> AllSessionEvents = new List<EventModel>();
        private List<EventModel> eventsOccurredSaveForWindow2 = new List<EventModel>();
        private List<EventModel> eventsOccurred = new List<EventModel>();
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private DataTable MainWindowDataTable;
        public MainWindow()
        {
            InitializeComponent();
            
            this.MainWindowDataTable = MainWindow.SetUpDataTable("main_window_data_table");
            this.SetupWatcher();
            DataGrid.ItemsSource = this.MainWindowDataTable.DefaultView;
            DataGrid.DataContext = this.MainWindowDataTable;
        }
        public List<EventModel> GetEventsForWindow2()
        {
            return this.eventsOccurredSaveForWindow2;
        }
        private void Button_Click_Folder(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    this.PathTextBox.Text = fbd.SelectedPath;
                }
            }
        }
        private void SetupWatcher()
        {
            this.watcher.IncludeSubdirectories = true;
            this.watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            this.watcher.Changed += OnChanged;
            this.watcher.Created += OnChanged;
            this.watcher.Deleted += OnChanged;
            this.watcher.Renamed += OnRenamed;
        }
        private void StartSubRoutine()
        {
           
            this.StartMenu.IsEnabled = false;
            this.Start_Button.IsEnabled = false;

           
            this.StopMenu.IsEnabled = true;
            this.Stop_Button.IsEnabled = true;
        }
        private void StopSubRoutine()
        {
           
            this.StartMenu.IsEnabled = true;
            this.Start_Button.IsEnabled = true;

           
            this.StopMenu.IsEnabled = false;
            this.Stop_Button.IsEnabled = false;
        }


        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(PathTextBox.Text))
            {
                this.Error_Label.Content = "No Current Errors.";
                watcher.Path = (PathTextBox.Text.Length == 0) ? "C:/Users/" : PathTextBox.Text;
                watcher.Filter = (ExtTextBox.Text.Length == 0) ? "" : "*" + ExtTextBox.Text;
                this.StartSubRoutine();
                this.watcher.EnableRaisingEvents = true;


            }
            else
            {
                this.Error_Label.Content = "ERROR: Bad Selection of Directory or File!";
            }
            
        }
        private void UpdateDataGrid()
        {
            UpdateDataGridRows(this.eventsOccurred, this.MainWindowDataTable, this.DataGrid);
            this.eventsOccurredSaveForWindow2.AddRange(this.eventsOccurred);
            this.eventsOccurred.Clear();
        }
        public void UpdateDataGridRows(List<EventModel> EventModels, DataTable dt, System.Windows.Controls.DataGrid dg)
        {
            foreach (EventModel em in EventModels)
            {
                DataRow dr = dt.NewRow();
                dr["File Name"] = em.FileName;
                dr["PATH"] = em.AbsolutePath;
                dr["EVENT"] = em.EventOccurred;
                dr["Date/Time"] = em.DateTime;
                dt.Rows.Add(dr);
            }
          dg.ItemsSource = dt.DefaultView;
            dg.DataContext = this.MainWindowDataTable;
        } 
        public static DataTable SetUpDataTable(string name)
        {
            DataTable dt;
            dt = new DataTable() { TableName = name };
            dt.Clear();
            dt.Columns.Add(new DataColumn("File Name"));
            dt.Columns.Add(new DataColumn("PATH"));
            dt.Columns.Add(new DataColumn("EVENT"));
            DataColumn dc = new DataColumn("Date/Time", typeof(DateTime));
            dt.Columns.Add(dc);
            return dt;
        }
        private void ChangeExtensionFilter(String filter)
        {
            watcher.Filter = filter;
        }


        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
            {
                EventModel em = new EventModel();
                em.FileName = e.Name;
                em.AbsolutePath = e.FullPath;
                em.EventOccurred = e.ChangeType.ToString();
                em.DateTime = DateTime.Now; 
                this.eventsOccurred.Add(em);
                this.AllSessionEvents.Add(em);
                this.UpdateDataGrid();            
            });
        }
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate ()
              {
                  EventModel em = new EventModel();
                  em.FileName = e.Name;
                  em.AbsolutePath = e.FullPath;
                  em.EventOccurred = e.ChangeType.ToString();
                  em.DateTime = DateTime.Now;
                  this.eventsOccurred.Add(em);
                  this.AllSessionEvents.Add(em);
                  this.UpdateDataGrid();
              });
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            this.watcher.EnableRaisingEvents = false;
            this.StopSubRoutine();
        }

        private void Database_Click(object sender, RoutedEventArgs e)
        {
            QueryForm queryWindow = new QueryForm();
            queryWindow.Show();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About_Menu_Window aboutWindow = new About_Menu_Window();
            aboutWindow.Show();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void QueryForm_Click(object sender, RoutedEventArgs e)
        {
            QueryForm queryWindow = new QueryForm();
            queryWindow.Show();
        }
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            this.DataGrid.ItemsSource = null;
            this.MainWindowDataTable = new DataTable();
            this.MainWindowDataTable = MainWindow.SetUpDataTable("main_window_data_table");
            string extension = this.ExtTextBox.Text;
            foreach (EventModel em in this.AllSessionEvents)
            {
                if (em.AbsolutePath.EndsWith(extension))
                {
                    DataRow dr = this.MainWindowDataTable.NewRow();
                    dr["File Name"] = em.FileName;
                    dr["PATH"] = em.AbsolutePath;
                    dr["EVENT"] = em.EventOccurred;
                    dr["Date/Time"] = em.DateTime;
                    this.MainWindowDataTable.Rows.Add(dr);
                }
            }
            DataGrid.ItemsSource = this.MainWindowDataTable.DefaultView;
            DataGrid.DataContext = this.MainWindowDataTable;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            this.DataGrid.ItemsSource = null;
            this.eventsOccurred.Clear();
            this.eventsOccurredSaveForWindow2.Clear();
            this.AllSessionEvents.Clear();
            this.MainWindowDataTable = MainWindow.SetUpDataTable("main_window_data_table");
            DataGrid.ItemsSource = this.MainWindowDataTable.DefaultView;
            DataGrid.DataContext = this.MainWindowDataTable;
        }

        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ExtensionTextBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
