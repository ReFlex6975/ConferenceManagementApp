using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ReFlexConnectionString"].ConnectionString;

        public Authorization()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Password;

            // Выполните запрос к базе данных, чтобы получить роль пользователя
            string role = GetRoleFromDatabase(username, password);

            if (role != null)
            {
                OpenMainWindow(role);
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль");
            }
        }

        private string GetRoleFromDatabase(string username, string password)
        {
            string role = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT role FROM Users WHERE username = @username AND password = @password";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        role = result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при выполнении запроса к базе данных: " + ex.Message);
                }
            }

            return role;
        }

        private void OpenMainWindow(string role)
        {
            Window mainWindow;

            switch (role)
            {
                case "1":
                    mainWindow = new AdminWindow();
                    break;
                case "2":
                    mainWindow = new ResearcherWindow();
                    break;
                case "3":
                    mainWindow = new GuestWindow();
                    break;
                case "4":
                    mainWindow = new MainWindow();
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя.");
                    return;
            }

            if (mainWindow != null)
            {
                mainWindow.Show();
                this.Close();
            }
        }

        private void GuestLoginButton_Click(object sender, RoutedEventArgs e)
        {
            GuestWindow guest = new GuestWindow();
            this.Close();
            guest.Show();
        }
    }
}
