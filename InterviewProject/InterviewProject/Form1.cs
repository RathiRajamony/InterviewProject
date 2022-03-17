using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace InterviewProject
{
    public partial class Form1 : Form
    {
        string connectionString = ConfigurationManager.ConnectionStrings["interviewConnectionString"].ConnectionString;

        /// <summary>
        /// Initializes the Form fields and the data
        /// </summary>
        public Form1()
        {
            InitializeComponent();
            View();
        }

        /// <summary>
        /// Initializes the Form with the applicant details
        /// </summary>
        private void View()
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                string cmd = "Select * from tbl_interview";
                SqlDataAdapter adapter = new SqlDataAdapter(cmd, connectionString);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dgView.DataSource = dataTable;
            }

            //If the Interview Date has passed, then it will display "Past due"
            foreach(DataGridViewRow row in dgView.Rows)
            {
                if(row.Cells["InterviewDate"].Value?.ToString().CompareTo(DateTime.Today.ToString("dd MMMM yyyy")) < 0)
                {
                    row.Cells["InterviewStatus"].Value = "Past Due";
                }
            }
            dgView.Columns[6].Width = 125;
        }

        /// <summary>
        /// Clears the textbox and combobox fields 
        /// </summary>
        private void ClearData()
        {
            txtName.Text = txtPhone.Text = txtEmail.Text = dateInterview.Text = "";
            cboStatus.Enabled = true;
            cboGender.SelectedItem = cboStatus.SelectedItem = cboGender.Text = cboStatus.Text = null;
            cboStatus.SelectedText = cboGender.SelectedText = "Select";
        }

        /// <summary>
        /// Fills the textbox and combobox fields with the corresponding row selected.
        /// </summary>
        private void FillDetails()
        {
            txtName.Text =  dgView.CurrentRow.Cells["ApplicantName"].Value.ToString();
            cboGender.Text = dgView.CurrentRow.Cells["Gender"].Value.ToString();
            txtPhone.Text = dgView.CurrentRow.Cells["Phone"].Value.ToString();
            txtEmail.Text = dgView.CurrentRow.Cells["Email"].Value.ToString();
            cboStatus.Text = dgView.CurrentRow.Cells["InterviewStatus"].Value.ToString();
            dateInterview.MinDate = (dgView.CurrentRow.Cells["InterviewDate"].Value?.ToString().CompareTo(DateTime.Today.ToString("dd MMMM yyyy")) < 0)
                ? Convert.ToDateTime(dgView.CurrentRow.Cells["InterviewDate"].Value) 
                : System.DateTime.Today;
            dateInterview.Text = dgView.CurrentRow.Cells["InterviewDate"].Value.ToString();
            cboStatus.Enabled = !dgView.CurrentRow.Cells["InterviewStatus"].Value.ToString().Contains("Completed");
        }
        
        /// <summary>
        /// Validates the fields before saving or updating the applicant details
        /// </summary>
        /// <returns></returns>
        private bool ValidateFields()
        {
            if (txtName.Text == "" || cboGender.SelectedItem == null || txtPhone.Text == "" || txtEmail.Text == "")
            {
                MessageBox.Show("Required Details missing! Please fill all the details");
                return false;
            }

            if (txtPhone.Text.Length != 10)
            {
                MessageBox.Show("Enter a valid 10 digit phone number");
                return false;
            }

            if (txtEmail.Text.Length > 20)
            {
                MessageBox.Show("Email id is too long, Please enter a valid email");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Button Add event handler to add a new applicant details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateFields())
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string cmd = "Insert into tbl_interview (ApplicantName, Gender, Phone, Email, InterviewDate, InterviewStatus) values (@Name, @Gender, @Phone, @Email, @Date, @Status)";

                    connection.Open();

                    SqlCommand command = new SqlCommand(cmd, connection);
                    command.Parameters.AddWithValue("@Name", txtName.Text);
                    command.Parameters.AddWithValue("@Gender", cboGender.SelectedItem?.ToString());
                    command.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    command.Parameters.AddWithValue("@Email", txtEmail.Text);
                    command.Parameters.AddWithValue("@Date", dateInterview.Text);

                    if (cboStatus.SelectedItem?.ToString() == "Completed")
                    {
                        DialogResult result = MessageBox.Show("Do you wish to change the status to Completed?", "Confirmation", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            command.Parameters.AddWithValue("@Status", cboStatus.SelectedItem.ToString() + "-" + DateTime.Today.ToString("dd/MM/yyyy"));
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Status", dgView.CurrentRow.Cells["InterviewStatus"].Value.ToString());
                            MessageBox.Show("Operation Cancelled! Status has been set to Scheduled, Please update it to Completed after the process is over.");
                        }
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Status", cboStatus.SelectedItem.ToString());
                    }

                    command.ExecuteNonQuery();

                    connection.Close();

                    MessageBox.Show("Applicant Details added successfully");
                    View();
                    ClearData();
                }
            }
        }

        /// <summary>
        /// Button Update event handler to update the corresponding applicant details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (ValidateFields())
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string cmd = "Update tbl_interview Set ApplicantName=@Name, Gender=@Gender, Phone=@Phone, Email=@Email, InterviewDate=@Date, InterviewStatus=@Status where ApplicantId=@ApplicantId";

                    connection.Open();

                    SqlCommand command = new SqlCommand(cmd, connection);
                    command.Parameters.AddWithValue("@Name", txtName.Text);
                    command.Parameters.AddWithValue("@Gender", cboGender.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@Phone", txtPhone.Text);
                    command.Parameters.AddWithValue("@Email", txtEmail.Text);
                    command.Parameters.AddWithValue("@Date", dateInterview.Text);

                    if (cboStatus.SelectedItem?.ToString() == "Completed")
                    {
                        DialogResult result = MessageBox.Show("Do you wish to change the status to Completed?", "Confirmation", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            command.Parameters.AddWithValue("@Status", cboStatus.SelectedItem.ToString() + "-" + DateTime.Today.ToString("dd/MM/yyyy"));
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Status", dgView.CurrentRow.Cells["InterviewStatus"].Value.ToString());
                            MessageBox.Show("Operation Cancelled! Status is not changed to Completed");
                        }
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@Status", cboStatus.SelectedItem.ToString());
                    }

                    command.Parameters.AddWithValue("@ApplicantId", dgView.CurrentRow.Cells["ApplicantId"].Value?.ToString());
                    command.ExecuteNonQuery();

                    connection.Close();

                    View();
                    ClearData();
                }
            }
        }

        /// <summary>
        /// Delete button event handler to delete the corresponding applicant details
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string cmd = "Delete from tbl_interview where ApplicantId=@ApplicantId";

                connection.Open();

                SqlCommand command = new SqlCommand(cmd, connection);
                command.Parameters.AddWithValue("@ApplicantId", dgView.CurrentRow.Cells["ApplicantId"].Value?.ToString());
                command.ExecuteNonQuery();

                connection.Close();

                DialogResult result = MessageBox.Show("Are you sure you want to delete the Applicant details?", "Confirmation", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    MessageBox.Show("Applicant Details deleted successfully");
                }
                View();
                ClearData();
            }
        }

        /// <summary>
        /// Data grid view cell click event handler to fill the textbox and combo box when the respective cell is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgView.CurrentCell == null || dgView.CurrentCell.Value == null || e.RowIndex == -1)
            {
                return;
            }

            FillDetails();
        }

        /// <summary>
        /// Clear button click event handler to clear the data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnclear_Click(object sender, EventArgs e)
        {
            ClearData();
        }

        /// <summary>
        /// Name text box key press event handler to restrict numbers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(!char.IsControl(e.KeyChar) && char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Phone text box key press event handler to allow only numbers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
