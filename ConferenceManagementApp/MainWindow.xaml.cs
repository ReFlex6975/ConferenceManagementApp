using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace ConferenceManagementApp
{
    public partial class MainWindow : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ReFlexConnectionString"].ConnectionString;

        public List<Researcher> Researchers { get; set; }
        public List<Conference> Conferences { get; set; }
        public List<AnalysisResult> AnalysisResults { get; set; }

        public MainWindow()
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
            researchersListBox.ItemsSource = Researchers;
            conferencesListBox.ItemsSource = Conferences;
            analysisResultsDataGrid.ItemsSource = AnalysisResults;
            // Создайте список AnalysisResults и заполните его  
            AnalysisResults = new List<AnalysisResult>();

            // Загрузите данные из базы данных
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

            // Установите AnalysisResults в качестве источника данных для DataGrid
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

        private void AddParticipationButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedResearchers = researchersListBox.SelectedItems;
            var selectedConference = (Conference)conferencesListBox.SelectedItem;
            string topic = topicTextBox.Text;

            if (selectedResearchers.Count == 0 || selectedConference == null)
            {
                MessageBox.Show("Please select at least one researcher and one conference.");
                return;
            }

            foreach (Researcher researcher in selectedResearchers)
            {
                AddParticipation(researcher.Id, selectedConference.ConferenceCode, topic);
            }
        }

        private void AddParticipation(int researcherId, int conferenceCode, string topic)
        {
            // Добавление записи о участии ученого в конференции
            string insertQuery = "INSERT INTO Participation (researcher_id, conference_code, topic) VALUES (@researcher_id, @conference_code, @topic)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@researcher_id", researcherId);
                command.Parameters.AddWithValue("@conference_code", conferenceCode);
                command.Parameters.AddWithValue("@topic", topic);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Participation information saved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding participation: " + ex.Message);
                }
            }
        }

        private void SaveConferenceButton_Click(object sender, RoutedEventArgs e)
        {
            conferenceErrorLabel.Content = "";
            if (string.IsNullOrEmpty(conferenceNameTextBox.Text) || string.IsNullOrEmpty(conferenceLocationTextBox.Text))
            {
                conferenceErrorLabel.Content = "Вы не заполнили все поля";
                return;
            }
            int nextConferenceCode = GetNextConferenceCode();

            if (nextConferenceCode < 1 || nextConferenceCode > 100000)
            {
                MessageBox.Show("Cannot add new conference. Conference code out of range (1 - 100000).");
                return;
            }

            string insertQuery = "INSERT INTO Conferences (conference_code, name, date, location) VALUES (@conference_code, @name, @date, @location)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@conference_code", nextConferenceCode);
                command.Parameters.AddWithValue("@name", conferenceNameTextBox.Text);
                command.Parameters.AddWithValue("@date", conferenceDatePicker.SelectedDate);
                command.Parameters.AddWithValue("@location", conferenceLocationTextBox.Text);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Conference information saved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private int GetNextConferenceCode()
        {
            int nextConferenceCode = 1; // Минимальный код конференции

            string query = "SELECT MAX(conference_code) FROM Conferences";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        int maxConferenceCode = Convert.ToInt32(result);
                        nextConferenceCode = Math.Min(100000, maxConferenceCode + 1);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

            return nextConferenceCode;
        }

        private void SaveParticipantButton_Click(object sender, RoutedEventArgs e)
        {
            errorLabel.Content = "";
            int nextEmployeeId = GetNextEmployeeId();
            if (string.IsNullOrEmpty(countryTextBox.Text) || string.IsNullOrEmpty(fullNameTextBox.Text))
            {
                errorLabel.Content = "Вы не заполнили все поля";
                return;
            }

            if (nextEmployeeId < 1000 || nextEmployeeId > 30000)
            {
                MessageBox.Show("Cannot add new participant. Employee ID out of range (1000 - 30000).");
                return;
            }

            string selectedDegree = (degreeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();


            string insertQuery = "INSERT INTO Researchers (id, full_name, country, academic_degree) VALUES (@id, @full_name, @country, @academic_degree)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@id", nextEmployeeId);
                command.Parameters.AddWithValue("@full_name", fullNameTextBox.Text);
                command.Parameters.AddWithValue("@country", countryTextBox.Text);
                command.Parameters.AddWithValue("@academic_degree", selectedDegree); // Использовать выбранную степень

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    MessageBox.Show("Participant information saved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private int GetNextEmployeeId()
        {
            int nextEmployeeId = 1000; // Минимальный табельный номер

            string query = "SELECT MAX(id) FROM Researchers";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        int maxId = Convert.ToInt32(result);
                        nextEmployeeId = Math.Max(1000, maxId + 1); // Обновлено для корректного увеличения ID
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

            return nextEmployeeId;
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

    public class Researcher
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string AcademicDegree { get; set; }
    }

    public class Conference
    {
        public int ConferenceCode { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
    }
}
