using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConferenceManagementApp
{
    /// <summary>
    /// Логика взаимодействия для GuestWindow.xaml
    /// </summary>
    public partial class GuestWindow : Window
    {
        private string connectionString = "Data Source=REFLEXLAPTOP;Initial Catalog=PR1_1;Integrated Security=True"; //ноут
        //private string connectionString = "Data Source=ReFlex;Initial Catalog=PR1_1;Integrated Security=True"; //пк

        public List<Researcher> Researchers { get; set; }
        public List<Conference> Conferences { get; set; }
        public List<AnalysisResult> AnalysisResults { get; set; }

        public GuestWindow()
        {
            InitializeComponent();
            LoadData();
        }

        public class AnalysisResult
        {
            public string FullName { get; set; }
            public int NumberOfPresentations { get; set; }
        }

        private void LoadData()
        {
            Researchers = new List<Researcher>();
            Conferences = new List<Conference>();
            LoadResearchers();
            LoadConferences();
            analysisResultsDataGrid.ItemsSource = AnalysisResults;

            AnalysisResults = new List<AnalysisResult>();
            string query = "SELECT full_name, COUNT(*) AS number_of_presentations " +
                           "FROM Researchers " +
                           "INNER JOIN Participation ON Researchers.id = Participation.researcher_id " +
                           "GROUP BY full_name";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AnalysisResults.Add(new AnalysisResult
                        {
                            FullName = reader.GetString(0),
                            NumberOfPresentations = reader.GetInt32(1)
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading analysis data: " + ex.Message);
                }
            }

            analysisResultsDataGrid.ItemsSource = AnalysisResults;
        }

        private void LoadResearchers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Researchers";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Researchers.Add(new Researcher
                        {
                            Id = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            Country = reader.GetString(2),
                            AcademicDegree = reader.GetString(3)
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading researchers: " + ex.Message);
                }
            }
        }

        private void LoadConferences()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Conferences";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Conferences.Add(new Conference
                        {
                            ConferenceCode = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Date = reader.GetDateTime(2),
                            Location = reader.GetString(3)
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading conferences: " + ex.Message);
                }
            }
        }
            
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            searchErrorLabel.Content = "";
            if (string.IsNullOrEmpty(fullNameSearchTextBox.Text))
            {
                searchErrorLabel.Content = "Вы не заполнили все поля";
                return;
            }
            string searchQuery = "SELECT Conferences.name, Conferences.date, Conferences.location, Participation.topic " +
                                 "FROM Researchers " +
                                 "INNER JOIN Participation ON Researchers.id = Participation.researcher_id " +
                                 "INNER JOIN Conferences ON Participation.conference_code = Conferences.conference_code " +
                                 "WHERE Researchers.full_name = @full_name " +
                                 "ORDER BY Conferences.date DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(searchQuery, connection);
                command.Parameters.AddWithValue("@full_name", fullNameSearchTextBox.Text);

                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    searchResultsDataGrid.ItemsSource = dataTable.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void ChangeAccountButton_Click(object sender, RoutedEventArgs e)
        {
            Authorization auth = new Authorization();
            this.Close();
            auth.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            CustomMessageBox customMessageBox = new CustomMessageBox();
            customMessageBox.ShowDialog();
        }
    }
}