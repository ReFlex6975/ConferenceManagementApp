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

namespace ConferenceManagementApp
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Data Source=REFLEXLAPTOP;Initial Catalog=PR1_1;Integrated Security=True";

        public MainWindow()
        {
            InitializeComponent();
            PopulateAnalysisData();
        }

        private void SaveConferenceButton_Click(object sender, RoutedEventArgs e)
        {
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
            int nextEmployeeId = GetNextEmployeeId();

            if (nextEmployeeId < 1000 || nextEmployeeId > 30000)
            {
                MessageBox.Show("Cannot add new participant. Employee ID out of range (1000 - 30000).");
                return;
            }

            string selectedDegree = degreeComboBox.SelectedItem.ToString(); // Получить выбранную степень из ComboBox

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
                        nextEmployeeId = Math.Max(nextEmployeeId, maxId + 1);
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

        private void PopulateAnalysisData()
        {
            string analysisQuery = "SELECT full_name, COUNT(*) AS number_of_presentations " +
                                   "FROM Researchers " +
                                   "INNER JOIN Participation ON Researchers.id = Participation.researcher_id " +
                                   "GROUP BY full_name";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(analysisQuery, connection);

                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    analysisResultsDataGrid.ItemsSource = dataTable.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
    }
}
