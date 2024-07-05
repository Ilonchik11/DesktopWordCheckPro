using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;

namespace WordCheckPro
{
    public partial class TranslationForm : Form
    {
        SqlConnection conn = null;
        SqlDataReader reader = null;
        List<string> correctAnswers = null;
        List<string> userAnswers = null;

        public TranslationForm()
        {
            InitializeComponent();
        }

        private async void TranslationForm_Load(object sender, EventArgs e)
        {
            try
            {
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString);
                conn.Open();
                bool tablesLoaded = await LoadTableNamesAsync();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Database isn't opened!");
            }
            finally
            {
                conn.Close();
            }
        }

        private async Task<bool> LoadTableNamesAsync()
        {
            try
            {
                string query = "SELECT table_name FROM information_schema.tables WHERE table_type = 'BASE TABLE'";
                SqlCommand command = new SqlCommand(query, conn);
                reader = command.ExecuteReader();

                do 
                {
                    while (await reader.ReadAsync())
                    {
                        string tableName = reader["table_name"].ToString();
                        if (tableName != "Users")
                        {
                            comboBox1.Items.Add(tableName);
                        }
                    }
                } while (await reader.NextResultAsync());
                comboBox1.SelectedIndex = 0;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке информации из базы данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                reader.Close();
                conn.Close();
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString);
                conn.Open();
                // MessageBox.Show("Connection is active");
                string cmdText = $"SELECT * FROM {comboBox1.Text}";

                SqlCommand sqlCommand = new SqlCommand(cmdText, conn);
                reader = sqlCommand.ExecuteReader();
                correctAnswers = new List<string>();

                while (await reader.ReadAsync())
                {
                    if (reader.FieldCount >= 4) 
                    {
                        int id = Convert.ToInt32(reader[0]);

                        string textBox1Data = reader["English"].ToString();
                        string radioButton1Data = reader["German_Correct"].ToString();
                        correctAnswers.Add(radioButton1Data);
                        // listBox1.Items.Add(radioButton1Data);
                        string radioButton2Data = reader["German_InCorrect"].ToString();

                        System.Windows.Forms.TextBox textBox = Controls.Find($"textBox{id}", true).FirstOrDefault() as System.Windows.Forms.TextBox;
                        RadioButton radioButton1 = this.Controls.Find($"radioButton1_{id}", true).FirstOrDefault() as RadioButton;
                        RadioButton radioButton2 = this.Controls.Find($"radioButton2_{id}", true).FirstOrDefault() as RadioButton;

                        if (radioButton1 != null)
                            radioButton1.Checked = false;

                        if (radioButton2 != null)
                            radioButton2.Checked = false;

                        if (textBox != null)
                            textBox.Text = textBox1Data;

                        if (radioButton1 != null)
                            radioButton1.Text = radioButton1Data;

                        if (radioButton2 != null)
                            radioButton2.Text = radioButton2Data;
                    }
                }
            }
            catch (Exception ex )
            {
                MessageBox.Show(ex.Message, "Данные не могут быть считаны");
            }
            finally
            {
                //MessageBox.Show("Connection is closed!");
                conn.Close();
                reader.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            userAnswers = new List<string>();
            foreach (Control control in this.Controls)
            {
                if (control is GroupBox groupBox)
                {
                    RadioButton checkedBut = groupBox.Controls.OfType<RadioButton>()
                                                                  .FirstOrDefault(rb => rb.Checked);

                    if (checkedBut != null)
                    {
                        userAnswers.Add(checkedBut.Text);
                        // listBox1.Items.Add(checkedBut.Text);
                    }
                }
            }
            userAnswers.Reverse();
            //int numOfCorAnswers = correctAnswers.Zip(userAnswers, (x, y) => x == y ? 1 : 0).Sum();
            int numOfCorAnswers = 0;

            if (correctAnswers.Count == userAnswers.Count)
            {
                for (int i = 0; i < correctAnswers.Count; i++)
                {
                    if (correctAnswers[i] == userAnswers[i])
                    {
                        numOfCorAnswers++;
                    }
                }
            }
            MessageBox.Show($"Correct answers: {numOfCorAnswers}/5", "Results", MessageBoxButtons.OK);
            listBox1.Items.Add($"{comboBox1.Text}: {numOfCorAnswers}/5");
        }
    }
}
