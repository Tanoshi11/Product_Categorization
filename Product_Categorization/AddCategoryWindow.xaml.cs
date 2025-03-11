using System;
using System.Data.SqlClient;
using System.Windows;

namespace Product_Categorization
{
    public partial class AddCategoryWindow : Window
    {
        private readonly string connectionString = "Data Source=LAPTOP-CERQCNC0;Initial Catalog=ProductManagementDB;Integrated Security=True;";
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
                MessageBox.Show("Category name cannot be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if category already exists
                    string checkQuery = "SELECT COUNT(*) FROM Categories WHERE LOWER(Category_Name) = LOWER(@CategoryName);";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@CategoryName", newCategory);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("This category already exists!", "Duplicate Category", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Insert the new category
                    string insertQuery = "INSERT INTO Categories (Category_Name) VALUES (@CategoryName);";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@CategoryName", newCategory);
                        int rowsAffected = insertCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Category Added Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            CategoryAdded = true; // Ensures the main window updates the categories
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add category.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Error adding category: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
