using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Product_Categorization
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString = "Data Source=LAPTOP-CERQCNC0;Initial Catalog=ProductManagementDB;Integrated Security=True;";

        public ObservableCollection<string> Categories { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this; // Set DataContext for Binding
            LoadCategories();
        }

        private void LoadCategories()
        {
            Categories.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Category_Name FROM Categories ORDER BY Category_Name;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categories.Add(reader["Category_Name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lists_of_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lists_of_category.SelectedItem != null)
            {
                string selectedCategory = lists_of_category.SelectedItem.ToString().ToUpper(); // Convert to uppercase

                // Define max character count per line (approximate)
                int maxCharsPerLine = 14;

                // Split by words for proper wrapping
                string[] words = selectedCategory.Split(' ');
                StringBuilder firstLine = new StringBuilder();
                StringBuilder secondLine = new StringBuilder();
                int charCount = 0;

                foreach (string word in words)
                {
                    if (charCount + word.Length <= maxCharsPerLine || firstLine.Length == 0)
                    {
                        firstLine.Append((firstLine.Length > 0 ? " " : "") + word);
                        charCount += word.Length + 1;
                    }
                    else
                    {
                        secondLine.Append((secondLine.Length > 0 ? " " : "") + word);
                    }
                }

                if (secondLine.Length > 0)
                {
                    selectedCategoryTextBlock.Text = firstLine + "\n" + secondLine;
                    selectedCategoryTextBlock.Margin = new Thickness(366, 60, 0, 0); // Move up a bit
                    selectedCategoryTextBlock.FontSize = 22; // Reduce font size slightly
                }
                else
                {
                    selectedCategoryTextBlock.Text = selectedCategory;
                    selectedCategoryTextBlock.Margin = new Thickness(366, 79, 0, 0); // Default position
                    selectedCategoryTextBlock.FontSize = 26; // Default size
                }

                LoadItemsForSelectedCategory(selectedCategory);
            }
        }




        public void LoadItemsForSelectedCategory(string selectedCategory)
        {
            Items.Clear();
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
                                Items.Add(new Item
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
            catch (Exception ex)
            {
                MessageBox.Show("Error loading items: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void add_category_Click(object sender, RoutedEventArgs e)
        {
            AddCategoryWindow addCategoryWindow = new AddCategoryWindow();
            addCategoryWindow.ShowDialog();
            if (addCategoryWindow.CategoryAdded)
            {
                LoadCategories();
            }
        }

        private void remove_category_Click(object sender, RoutedEventArgs e)
        {
            if (lists_of_category.SelectedItem != null)
            {
                string selectedCategory = lists_of_category.SelectedItem.ToString();

                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete the category '{selectedCategory}'? All associated items will also be removed.",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();

                            string deleteItemsQuery = "DELETE FROM ItemCategories WHERE Category_ID = (SELECT Category_ID FROM Categories WHERE Category_Name = @CategoryName);";
                            using (SqlCommand deleteItemsCmd = new SqlCommand(deleteItemsQuery, conn))
                            {
                                deleteItemsCmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                                deleteItemsCmd.ExecuteNonQuery();
                            }

                            string deleteCategoryQuery = "DELETE FROM Categories WHERE Category_Name = @CategoryName;";
                            using (SqlCommand deleteCategoryCmd = new SqlCommand(deleteCategoryQuery, conn))
                            {
                                deleteCategoryCmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                                deleteCategoryCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Category deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadCategories();
                            Items.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error removing category: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a category to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void add_item_Click(object sender, RoutedEventArgs e)
        {
            if (lists_of_category.SelectedItem != null)
            {
                string selectedCategory = lists_of_category.SelectedItem.ToString();
                AddItemWindow addItemWindow = new AddItemWindow(selectedCategory);
                addItemWindow.ShowDialog();

                if (addItemWindow.ItemAdded)
                {
                    LoadItemsForSelectedCategory(selectedCategory);
                }
            }
            else
            {
                MessageBox.Show("Please select a category to add an item to.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void remove_item_Click(object sender, RoutedEventArgs e)
        {
            if (lists_of_category.SelectedItem != null)
            {
                string selectedCategory = lists_of_category.SelectedItem.ToString();
                RemoveItemWindow removeItemWindow = new RemoveItemWindow(selectedCategory);
                removeItemWindow.ShowDialog();

                if (removeItemWindow.ItemRemoved)
                {
                    LoadItemsForSelectedCategory(selectedCategory);
                }
            }
            else
            {
                MessageBox.Show("Please select a category to remove an item from.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}