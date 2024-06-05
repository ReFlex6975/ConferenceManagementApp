using System;
using System.Configuration;
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
        private string connectionString = ConfigurationManager.ConnectionStrings["ReFlexConnectionString"].ConnectionString;
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
        private void AddInputValidationHandlers()
        {
            fullNameSearchTextBox.PreviewTextInput += InputValidationHelper.TextBox_PreviewTextInput;
            fullNameSearchTextBox.PreviewKeyDown += InputValidationHelper.TextBox_PreviewKeyDown;
        }

        private void LoadData()
        {
            AddInputValidationHandlers();
            Researchers = new List<Researcher>();
            Conferences = new List<Conference>();
            LoadResearchers();
            LoadConferences();
            LoadMembers();
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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputValidationHelper.TextBox_PreviewTextInput(sender, e);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputValidationHelper.TextBox_PreviewKeyDown(sender, e);
        }
        private void LoadMembers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT full_name, country, academic_degree FROM Researchers";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    membersDataGrid.ItemsSource = dataTable.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading members data: " + ex.Message);
                }
            }
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