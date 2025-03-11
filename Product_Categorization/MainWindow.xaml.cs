using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace Product_Categorization
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString = "Data Source=LAPTOP-CERQCNC0;Initial Catalog=ProductManagementDB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

        public MainWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            lists_of_category.Items.Clear(); // Clear existing items

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
                            lists_of_category.Items.Add(reader["Category_Name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void add_category_Click(object sender, RoutedEventArgs e)
        {
            AddCategoryWindow addCategoryWindow = new AddCategoryWindow();
            addCategoryWindow.ShowDialog(); // Open window and wait for it to close

            if (addCategoryWindow.CategoryAdded)
            {
                LoadCategories(); // Refresh category list
            }
        }

        private void remove_category_Click(object sender, RoutedEventArgs e)
        {
            if (lists_of_category.SelectedItem != null)
            {
                string selectedCategory = lists_of_category.SelectedItem.ToString();

                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM Categories WHERE Category_Name = @CategoryName;";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Category Removed Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadCategories(); // Refresh category list
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing category: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a category to remove.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void add_item_Click(object sender, RoutedEventArgs e)
        {
            AddItemWindow addItemWindow = new AddItemWindow();
            addItemWindow.ShowDialog();

            if (addItemWindow.ItemAdded)
            {
                // Optionally refresh the item list or perform other actions
                LoadItemsForSelectedCategory();
            }
        }

        private void remove_item_Click(object sender, RoutedEventArgs e)
        {
            RemoveItemWindow removeItemWindow = new RemoveItemWindow();
            removeItemWindow.ShowDialog();

            if (removeItemWindow.ItemRemoved)
            {
                // Optionally refresh the item list or perform other actions
                LoadItemsForSelectedCategory();
            }
        }

        private void lists_of_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lists_of_category.SelectedItem != null)
            {
                string selectedCategory = lists_of_category.SelectedItem.ToString().ToUpper(); // Convert to uppercase
                selectedCategoryTextBlock.Text = selectedCategory; // Update TextBlock

                selectedCategoryTextBlock.FontSize = 30; // Change font size dynamically

                LoadItemsForSelectedCategory(); // Load items for the selected category
            }
        }



        private void list_of_items_on_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle item selection change here (if needed)
        }

        private void LoadItemsForSelectedCategory()
        {
            if (lists_of_category.SelectedItem == null)
                return;

            string selectedCategory = lists_of_category.SelectedItem.ToString();
            list_of_items_on_category.Items.Clear();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Item_Name, Description FROM Items WHERE Category_Name = @CategoryName;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CategoryName", selectedCategory);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list_of_items_on_category.Items.Add(new
                                {
                                    Name = reader["Item_Name"].ToString(),
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
    }
}