using System;
using System.Configuration;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net;

namespace WordCheckPro
{
    public partial class LoginForm : Form
    {
        SqlConnection conn = null;
        string emailSender = "marinkinaelena009@gmail.com";
        string passwordSender = "hqbiwwwpquhrvqpp";
        string serverAddress = "smtp.gmail.com";
        int serverPort = 587;
        string username = null;
        string password = null;

        public LoginForm()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            username = textBox1.Text;
            password = textBox2.Text;

            try
            {
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString);
                conn.Open();
                MessageBox.Show("Ok");
                bool isValidUser = await ValidateUserCredentialsAsync(username, password);

                try
                {
                    if(isValidUser)
                    {
                        MessageBox.Show("You entered successfully!");
                        await Task.Run(SendEmailNotification);
                        this.Hide();
                        TranslationForm translationForm = new TranslationForm();
                        translationForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Users WHERE LOWER(Login) = LOWER(@Login) AND Password = @Password";
                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@Login", username);
                    command.Parameters.AddWithValue("@Password", password);

                    int userCount = (int)await command.ExecuteScalarAsync();

                    return userCount > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка проверки логина и пароля: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SendEmailNotification()
        {
            try
            {
                MailAddress fromAddress = new MailAddress(emailSender, "WordCheckPro App");
                MailAddress toAddress = new MailAddress(username);

                MailMessage message = new MailMessage(fromAddress, toAddress);
                message.Subject = "Entrance to your account";
                message.Body = "Somebody has entered to your account on WordCheckPro. If it's you, just miss this message!";
                message.BodyEncoding = Encoding.UTF8;

                SmtpClient client = new SmtpClient(serverAddress, serverPort);
                client.Credentials = new NetworkCredential(emailSender, passwordSender);
                client.EnableSsl = true;
                client.Send(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //private bool ConnectToDatabase()
        //{
        //    try
        //    {
        //        DatabaseConnection.OpenConnection(); // Открыть соединение
        //        MessageBox.Show("Вход выполнен успешно!", "Поздравляем", MessageBoxButtons.OK);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return false;
        //    }
        //}

        private async void button2_Click(object sender, EventArgs e)
        {
            username = textBox1.Text;
            password = textBox2.Text;

            try
            {
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString);
                conn.Open();
                bool isUserInserted = await InsertUserAsync(username, password);

                if (isUserInserted)
                {
                    MessageBox.Show("Пользователь успешно зарегистрирован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при регистрации пользователя", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private async Task<bool> InsertUserAsync(string username, string password)
        {
            try
            {
                string query = "INSERT INTO Users (Login, Password) VALUES (@Login, @Password)";
                using (SqlCommand command = new SqlCommand(query, conn))
                {
                    command.Parameters.AddWithValue("@Login", username);
                    command.Parameters.AddWithValue("@Password", password);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при регистрации пользователя: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}