using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Product_Categorization
{
    public partial class AddItemWindow : Window
    {
        private readonly string connectionString = "Server=LAPTOP-CERQCNC0;Database=ProductManagementDB;Integrated Security=True;";
        public bool ItemAdded { get; private set; } = false;

        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public AddItemWindow()
        {
            InitializeComponent();
            LoadCategories();
            LoadPlaceholderData();

            // Ensure placeholder text in search box
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

        private void LoadPlaceholderData()
        {
            Items.Add(new Item { ItemName = "Sample Item 1", Description = "This is the description for Sample Item 1." });
            Items.Add(new Item { ItemName = "Sample Item 2", Description = "This is the description for Sample Item 2." });
            Items.Add(new Item { ItemName = "Sample Item 3", Description = "This is the description for Sample Item 3." });

            SearchResultsDataGrid.ItemsSource = Items;
        }

        private void SearchResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsDataGrid.SelectedItem is Item selectedItem)
            {
                // Update the description TextBox with the selected item's description
                itemDescriptionTextBlock.Text = string.IsNullOrWhiteSpace(selectedItem.Description)
                    ? "No description available."
                    : selectedItem.Description;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchResultsDataGrid.SelectedItem is Item selectedItem && CategoryComboBox.SelectedItem is string selectedCategory)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "INSERT INTO Items (Item_Name, Category_Name) VALUES (@ItemName, @CategoryName);";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@ItemName", selectedItem.ItemName);
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
            else
            {
                MessageBox.Show("Please select an item and a category.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

    public class Item
    {
        public string ItemName { get; set; }
        public string Description { get; set; }
    }
}