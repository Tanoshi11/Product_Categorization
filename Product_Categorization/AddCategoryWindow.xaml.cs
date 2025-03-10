using System;
using System.Data.SqlClient;
using System.Windows;

namespace Product_Categorization
{
    public partial class AddCategoryWindow : Window
    {
        private readonly string connectionString = "Server=LAPTOP-CERQCNC0;Database=ProductManagementDB;Integrated Security=True;";
        public bool CategoryAdded { get; private set; } = false;

        public AddCategoryWindow()
        {
            InitializeComponent();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string newCategory = CategoryTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(newCategory))
            {
                MessageBox.Show("Category name cannot be empty.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Categories (Category_Name) VALUES (@CategoryName);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", newCategory);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Category Added Successfully!");
                    CategoryAdded = true; // Ensure the category gets refreshed in MainWindow
                    this.Close();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Unique constraint violation
                    {
                        MessageBox.Show("This category already exists!");
                    }
                    else
                    {
                        MessageBox.Show("Error adding category: " + ex.Message);
                    }
                }
            }
        }
    }
}
