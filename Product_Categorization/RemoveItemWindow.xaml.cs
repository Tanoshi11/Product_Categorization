using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Product_Categorization
{
    public partial class RemoveItemWindow : Window
    {
        private readonly string connectionString = "Data Source=LAPTOP-CERQCNC0;Initial Catalog=ProductManagementDB;Integrated Security=True;";

        public bool ItemRemoved { get; private set; } = false;
        public ObservableCollection<Item> AvailableItems { get; set; } = new ObservableCollection<Item>();
        private readonly string selectedCategory;

        // Use a CollectionView to enable filtering in the UI
        private ICollectionView AvailableItemsView;

        public RemoveItemWindow(string category)
        {
            InitializeComponent();
            selectedCategory = category;
            this.Title = $"Remove Item from {selectedCategory}";
            Loaded += RemoveItemWindow_Loaded;
            DataContext = this;
        }

        private void RemoveItemWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAvailableItems();

            // Initialize the CollectionView
            AvailableItemsView = CollectionViewSource.GetDefaultView(AvailableItems);
            SearchResultsDataGrid.ItemsSource = AvailableItemsView;  // Bind to the view

            // Set initial focus
            SearchResultsDataGrid.Focus();

            // Format the title for display
            FormatTitle("REMOVE FROM", selectedCategory);
        }

        private void FormatTitle(string prefix, string category)
        {
            // Define max character count per line (approximate)
            int maxCharsPerLine = 20;

            // Combine prefix and category name
            string fullText = $"{prefix} {category.ToUpper()}";

            // Check if the full text exceeds the max character count
            if (fullText.Length > maxCharsPerLine)
            {
                // Split into two lines: prefix on top, category name on bottom
                RemoveItemTitleText.Text = $"{prefix}\n{category.ToUpper()}";
                RemoveItemTitleText.FontSize = 22; // Reduce font size slightly
                RemoveItemTitleText.Margin = new Thickness(0, 10, 0, 0); // Adjust margin
            }
            else
            {
                // If the full text fits in one line
                RemoveItemTitleText.Text = fullText;
                RemoveItemTitleText.FontSize = 26; // Default size
                RemoveItemTitleText.Margin = new Thickness(0, 20, 0, 0); // Default margin
            }

            // Center the title
            RemoveItemTitleText.HorizontalAlignment = HorizontalAlignment.Center;
            RemoveItemTitleText.TextAlignment = TextAlignment.Center;
        }

        private void LoadAvailableItems()
        {
            AvailableItems.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT i.Item_ID, i.Item_Name, i.Description
                        FROM Items i
                        INNER JOIN ItemCategories ic ON i.Item_ID = ic.Item_ID
                        INNER JOIN Categories c ON ic.Category_ID = c.Category_ID
                        WHERE c.Category_Name = @CategoryName;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AvailableItems.Add(new Item
                                {
                                    ItemID = Convert.ToInt32(reader["Item_ID"]),
                                    ItemName = reader["Item_Name"].ToString(),
                                    Description = reader["Description"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Database error while loading available items: " + ex.Message, "SQL Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading available items: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchResultsDataGrid.SelectedItem is Item selectedItem)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // Check if the item exists in the category
                        if (!ItemExistsInCategory(conn, selectedItem.ItemID, selectedCategory))
                        {
                            MessageBox.Show("This item is not in the selected category.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        // Remove the item from the category
                        if (RemoveItemFromCategory(conn, selectedItem.ItemID, selectedCategory))
                        {
                            MessageBox.Show("Item removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            ItemRemoved = true;
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Failed to remove item.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Database error while removing item: " + ex.Message, "SQL Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private bool ItemExistsInCategory(SqlConnection conn, int itemId, string categoryName)
        {
            string checkQuery = @"
                SELECT COUNT(*)
                FROM ItemCategories ic
                INNER JOIN Categories c ON ic.Category_ID = c.Category_ID
                WHERE ic.Item_ID = @ItemID AND c.Category_Name = @CategoryName;";

            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@ItemID", itemId);
                checkCmd.Parameters.AddWithValue("@CategoryName", categoryName);
                int count = (int)checkCmd.ExecuteScalar();
                return count > 0;
            }
        }

        private bool RemoveItemFromCategory(SqlConnection conn, int itemId, string categoryName)
        {
            string deleteQuery = @"
                DELETE FROM ItemCategories
                WHERE Item_ID = @ItemID
                AND Category_ID = (SELECT Category_ID FROM Categories WHERE Category_Name = @CategoryName);";

            using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
            {
                deleteCmd.Parameters.AddWithValue("@ItemID", itemId);
                deleteCmd.Parameters.AddWithValue("@CategoryName", categoryName);
                int rowsAffected = deleteCmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ItemTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AvailableItemsView != null)
            {
                string searchText = ItemTextBox.Text.Trim().ToLower();

                // Ignore filtering if the placeholder text is active
                if (searchText == "type to search...") return;

                AvailableItemsView.Filter = item =>
                {
                    if (string.IsNullOrEmpty(searchText))
                        return true;

                    if (item is Item itemToFilter)
                        return itemToFilter.ItemName.ToLower().Contains(searchText);

                    return false;
                };

                AvailableItemsView.Refresh();
                SearchResultsDataGrid.UpdateLayout();

                // Optionally, re-select the first item if nothing is selected.
                if (SearchResultsDataGrid.SelectedItem == null && SearchResultsDataGrid.Items.Count > 0)
                {
                    SearchResultsDataGrid.SelectedIndex = 0;
                }
            }
        }

        private void SearchResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsDataGrid.SelectedItem is Item selectedItem)
            {
                itemDescriptionTextBlock.Text = string.IsNullOrWhiteSpace(selectedItem.Description)
                    ? "No description available."
                    : selectedItem.Description;
            }
        }

        private void ItemTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ItemTextBox.Text == "Type to search...")
            {
                ItemTextBox.Text = "";
                ItemTextBox.Foreground = Brushes.Black;
            }
        }

        private void ItemTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemTextBox.Text))
            {
                ItemTextBox.Text = "Type to search...";
                ItemTextBox.Foreground = Brushes.Gray;
            }
        }
    }
}