using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace CSCD371_FileSystemWatcherApp
{
   
    public partial class QueryForm : Window
    {
        /* https:// stackoverflow.com/questions/21563940/how-to-connect-to-localdb-in-visual-studio-server-explorer */

        private const string Create_Command = "CREATE TABLE events (FileName VARCHAR(150) not null, AbsolutePath VARCHAR(300) not null, EventOccurred VARCHAR(40) not null, DateTime VARCHAR(40), PRIMARY KEY (EventOccurred, DateTime))";
        private SQLiteConnection SQL_QueryForm { get; set; }
        private DataTable DataTable_QueryForm { get; set; }
        private List<EventModel> eventsOccurred_QueryForm { get; set; }

        private DataTable Database_QueryForm{ get; set; }
        public QueryForm()
        {
            InitializeComponent();
            MainWindow mainWin = (MainWindow)System.Windows.Application.Current.MainWindow;
            if (!(File.Exists("local_database.sqlite")))
            {
                SQLiteConnection.CreateFile("local_database.sqlite");
                SQL_QueryForm = new SQLiteConnection("Data Source = local_database.sqlite; Version=3;");
                SQL_QueryForm.Open();
                SQLiteCommand SQLCommand = new SQLiteCommand(Create_Command, SQL_QueryForm);
                SQLCommand.ExecuteNonQuery();

            }
            else
            {
                SQL_QueryForm = new SQLiteConnection("Data Source=local_database.sqlite;Version=3;");
                SQL_QueryForm.Open();
                try
                {
                    SQLiteCommand SQLCommand = new SQLiteCommand("SELECT * FROM events", SQL_QueryForm);
                    SQLCommand.ExecuteReader();

                }
                #pragma warning disable CS0168 
                catch (Exception e)
                #pragma warning restore CS0168 
                {
                    SQLiteCommand SQLCommand = new SQLiteCommand(Create_Command, SQL_QueryForm);
                    SQLCommand.ExecuteNonQuery();

                }
            }
            UpdateCurrentListing();
        }

        public void Update()
        {
            UpdateCurrentListing();
        }
        private void UpdateCurrentListing()
        {
            MainWindow mainWin = (MainWindow)System.Windows.Application.Current.MainWindow;
            this.eventsOccurred_QueryForm = mainWin.GetEventsForWindow2();
            this.DataTable_QueryForm = new DataTable();
            this.SetUpDataTable("QueryForm_data_table");
            this.PopulateCurrentDatatable();
            this.PopSQLDatatable();
        }

        private void SetUpDataTable(string name)
        {
            DataTable_QueryForm.TableName = name;
            DataTable_QueryForm.Columns.Add(new DataColumn("File Name"));
            DataTable_QueryForm.Columns.Add(new DataColumn("PATH"));
            DataTable_QueryForm.Columns.Add(new DataColumn("EVENT"));
            DataColumn dataCol = new DataColumn("Date/Time");
            dataCol.DateTimeMode = DataSetDateTime.UnspecifiedLocal;
            DataTable_QueryForm.Columns.Add(dataCol);
        }

        private void SetUpDataTable_Database(string name)
        {
            Database_QueryForm.TableName = name;
            Database_QueryForm.Columns.Add(new DataColumn("File Name"));
            Database_QueryForm.Columns.Add(new DataColumn("PATH"));
            Database_QueryForm.Columns.Add(new DataColumn("EVENT"));
            DataColumn dataCol = new DataColumn("Date/Time");
            dataCol.DateTimeMode = DataSetDateTime.UnspecifiedLocal;
            Database_QueryForm.Columns.Add(dataCol);
        }

        private void ClearDB_Click(object sender, RoutedEventArgs e)
        {

            SQLiteCommand cmd = new SQLiteCommand("DELETE FROM events", SQL_QueryForm);
            cmd.ExecuteNonQuery();
            this.Update();
        }

        private void BackQuery_Click(object sender, RoutedEventArgs e)
        {
            this.SQL_QueryForm.Close();
            this.Close();
        }
        private void Submit_Ext(object sender, RoutedEventArgs e)
        {
            this.GridCurrent.ItemsSource = null;
            this.DataTable_QueryForm = new DataTable();
            this.SetUpDataTable("QueryForm_data_table");
            string extension = this.ExtensionTextBox.Text;
            foreach (EventModel eventModel in this.eventsOccurred_QueryForm)
            {
                if (eventModel.AbsolutePath.EndsWith(extension))
                {
                    DataRow dataRow = this.DataTable_QueryForm.NewRow();
                    dataRow["File Name"] = eventModel.FileName;
                    dataRow["PATH"] = eventModel.AbsolutePath;
                    dataRow["EVENT"] = eventModel.EventOccurred;
                    dataRow["Date/Time"] = eventModel.DateTime;
                    this.DataTable_QueryForm.Rows.Add(dataRow);
                }
            }
            GridCurrent.ItemsSource = this.DataTable_QueryForm.DefaultView;
        }

        private void ExtSubmitDB_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWin = (MainWindow)System.Windows.Application.Current.MainWindow;
            this.GridDatabase.ItemsSource = null;
            this.Database_QueryForm = new DataTable();
            this.SetUpDataTable_Database("QueryForm_datatable_database");
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM events", SQL_QueryForm);
            SQLiteDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            #pragma warning disable CS0168 
            catch (Exception ex)
            #pragma warning restore CS0168 
            {
                SQLiteCommand another_cmd = new SQLiteCommand(Create_Command, SQL_QueryForm);
                another_cmd.ExecuteNonQuery();
                reader = cmd.ExecuteReader();
            }
            List<EventModel> list = new List<EventModel>();
            string suffix = this.ExtensionTextBox.Text;
            while (reader.Read())
            {
                EventModel eventModel = new EventModel();
                eventModel.FileName = reader["FileName"].ToString();
                eventModel.AbsolutePath = reader["AbsolutePath"].ToString();
                eventModel.EventOccurred = reader["EventOccurred"].ToString();
                eventModel.DateTime = DateTime.Parse(reader["DateTime"].ToString());
                if (eventModel.AbsolutePath.EndsWith(suffix))
                {
                    list.Add(eventModel);
                }
            }
            mainWin.UpdateDataGridRows(list, this.Database_QueryForm, this.GridDatabase);
            reader.Close();
        }

        private void ExtSubmit_Click(object sender, RoutedEventArgs e)
        {
            InsertIntoSQLDatatable();
        }
        public SQLiteCommand CreateCommand(EventModel eventModel)
        {
            SQLiteCommand InsertIntoSQL = new SQLiteCommand($"INSERT INTO events (FileName, AbsolutePath, EventOccurred, DateTime) VALUES ('{eventModel.FileName}','{eventModel.AbsolutePath}','{eventModel.EventOccurred}','{eventModel.DateTime}')", SQL_QueryForm);
            return InsertIntoSQL;
        }
        private void InsertIntoSQLDatatable()
        {
            foreach (EventModel eventModel in eventsOccurred_QueryForm)
            {
                SQLiteCommand InsertIntoSQL = CreateCommand(eventModel);
                try
                {
                    InsertIntoSQL.ExecuteNonQuery();
                }
                #pragma warning disable CS0168 
                catch (System.Data.SQLite.SQLiteException sle)
                #pragma warning restore CS0168
                {
                    Console.WriteLine("Cant have similiar entries in database.");
                }
            }
            Update();
        }

        private void PopulateCurrentDatatable()
        {
            MainWindow mainWin = (MainWindow)System.Windows.Application.Current.MainWindow;
            mainWin.UpdateDataGridRows(this.eventsOccurred_QueryForm, this.DataTable_QueryForm, this.GridCurrent);
        }
        private void PopSQLDatatable()
        {
            MainWindow mainWin = (MainWindow)System.Windows.Application.Current.MainWindow;

            this.Database_QueryForm = new DataTable();
            SetUpDataTable_Database("QueryForm_datatable_database");

            SQLiteCommand SQLCommand = new SQLiteCommand("SELECT * FROM events", SQL_QueryForm);
            SQLiteDataReader reader = null;
            try
            {
                reader = SQLCommand.ExecuteReader();
            }
            #pragma warning disable CS0168 
            catch (Exception e)
            #pragma warning restore CS0168 
            {
                SQLiteCommand other_cmd = new SQLiteCommand(Create_Command, SQL_QueryForm);
                other_cmd.ExecuteNonQuery();
                reader = SQLCommand.ExecuteReader();
            }
            List<EventModel> list = new List<EventModel>();
            while (reader.Read())
            {
                EventModel eventModel = new EventModel();
                eventModel.FileName = reader["FileName"].ToString();
                eventModel.AbsolutePath = reader["AbsolutePath"].ToString();
                eventModel.EventOccurred = reader["EventOccurred"].ToString();
                eventModel.DateTime = DateTime.Parse(reader["DateTime"].ToString());
                list.Add(eventModel);
            }
            mainWin.UpdateDataGridRows(list, this.Database_QueryForm, this.GridDatabase);
            reader.Close();
        }

        private void ExtensionTextBox_Window2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
