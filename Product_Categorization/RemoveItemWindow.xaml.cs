using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Data;

namespace Product_Categorization
{
    public partial class RemoveItemWindow : Window
    {
        private readonly string connectionString = "Server=LAPTOP-CERQCNC0;Database=ProductManagementDB;Integrated Security=True;";
        public bool ItemRemoved { get; private set; } = false;

        public RemoveItemWindow()
        {
            InitializeComponent();
            LoadCategories();
            LoadItems();
            ItemTextBox_LostFocus(null, null); // Set initial placeholder text
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

        private void LoadItems()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Item_Name, Description FROM Items ORDER BY Item_Name;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        SearchResultsDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading items: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsDataGrid.SelectedItem is DataRowView row)
            {
                itemDescriptionTextBlock.Text = row["Description"].ToString();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchResultsDataGrid.SelectedItem is DataRowView row)
            {
                string selectedItem = row["Item_Name"].ToString();
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
                        string query = "DELETE FROM Items WHERE Item_Name = @ItemName AND Category_Name = @CategoryName;";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ItemName", selectedItem);
                            cmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Item removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                ItemRemoved = true;
                                LoadItems(); // Refresh list after deletion
                            }
                            else
                            {
                                MessageBox.Show("Item not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing item: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an item to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

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
