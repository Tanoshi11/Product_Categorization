using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Product_Categorization
{
    public partial class AddItemWindow : Window
    {
        private readonly string connectionString = "Server=LAPTOP-CERQCNC0;Database=ProductManagementDB;Integrated Security=True;";
        public bool ItemAdded { get; private set; } = false;

        public AddItemWindow()
        {
            InitializeComponent();
            LoadCategories();

            // Initialize placeholder text for the search box
            ItemTextBox_LostFocus(null, null);
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Category_Name FROM Categories ORDER BY Category_Name;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CategoryComboBox.Items.Add(reader["Category_Name"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading categories: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedItem = SearchResultsListBox.SelectedItem?.ToString();
            string selectedCategory = CategoryComboBox.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(selectedItem) || string.IsNullOrWhiteSpace(selectedCategory))
            {
                MessageBox.Show("Please select an item and a category.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Items (Item_Name, Category_Name) VALUES (@ItemName, @CategoryName);";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemName", selectedItem);
                        cmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Item added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ItemAdded = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding item: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Placeholder text logic for the search box
        private void ItemTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ItemTextBox.Text == "Type to search...")
            {
                ItemTextBox.Text = "";
                ItemTextBox.Foreground = SystemColors.ControlTextBrush;
            }
        }

        private void ItemTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemTextBox.Text))
            {
                ItemTextBox.Text = "Type to search...";
                ItemTextBox.Foreground = SystemColors.GrayTextBrush;
            }
        }
    }
}