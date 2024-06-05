using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ConferenceManagementApp
{
    public partial class AdminWindow : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ReFlexConnectionString"].ConnectionString;

        public List<Researcher> Researchers { get; set; }
        public List<Conference> Conferences { get; set; }
        public List<AnalysisResult> AnalysisResults { get; set; }

        public AdminWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void AddInputValidationHandlers()
        {
            fullNameTextBox.PreviewTextInput += InputValidationHelper.TextBox_PreviewTextInput;
            fullNameTextBox.PreviewKeyDown += InputValidationHelper.TextBox_PreviewKeyDown;

            countryTextBox.PreviewTextInput += InputValidationHelper.TextBox_PreviewTextInput;
            countryTextBox.PreviewKeyDown += InputValidationHelper.TextBox_PreviewKeyDown;

            fullNameSearchTextBox.PreviewTextInput += InputValidationHelper.TextBox_PreviewTextInput;
            fullNameSearchTextBox.PreviewKeyDown += InputValidationHelper.TextBox_PreviewKeyDown;

            countryTextBox.PreviewTextInput += InputValidationHelper.TextBox_PreviewTextInput;
            countryTextBox.PreviewKeyDown += InputValidationHelper.TextBox_PreviewKeyDown;
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            InputValidationHelper.TextBox_PreviewTextInput(sender, e);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            InputValidationHelper.TextBox_PreviewKeyDown(sender, e);
        }
        public class AnalysisResult
        {
            public string FullName { get; set; }
            public int NumberOfPresentations { get; set; }
        }

        private void LoadData()
        {
            AddInputValidationHandlers();
            Researchers = new List<Researcher>();
            Conferences = new List<Conference>();
            LoadResearchers();
            LoadConferences();
            LoadMembers();
            LoadD();
        }

        public void LoadD()
        {
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
        }

        private void LoadMembers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT id, full_name, country, academic_degree FROM Researchers";
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

        private void MembersDataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (membersDataGrid.SelectedItem is DataRowView rowView)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    int id = Convert.ToInt32(rowView["id"]);
                    string fullName = rowView["full_name"].ToString();
                    string country = rowView["country"].ToString();
                    string academicDegree = rowView["academic_degree"].ToString();

                    // Отладочное сообщение
                    MessageBox.Show($"Updating researcher ID {id} with FullName={fullName}, Country={country}, AcademicDegree={academicDegree}");

                    UpdateResearcher(id, fullName, country, academicDegree);
                }), DispatcherPriority.Background);
            }
        }

        private void UpdateResearcher(int id, string fullName, string country, string academicDegree)
        {
            string updateQuery = "UPDATE Researchers SET full_name = @full_name, country = @country, academic_degree = @academic_degree WHERE id = @id";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@full_name", fullName);
                command.Parameters.AddWithValue("@country", country);
                command.Parameters.AddWithValue("@academic_degree", academicDegree);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    // Отладочное сообщение
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Researcher updated successfully!");
                    }
                    else
                    {
                        MessageBox.Show("No rows affected. Update failed.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating researcher data: " + ex.Message);
                }
            }
        }

        // Остальные методы не изменены
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
                    LoadData();
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
                SqlCommand command = new SqlCommand(query);

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

            string query = "SELECT ISNULL(MAX(id), 999) + 1 FROM Researchers";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        nextEmployeeId = Convert.ToInt32(result);
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
}