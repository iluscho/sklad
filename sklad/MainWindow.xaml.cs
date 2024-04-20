using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using sklad;

namespace Sklad
{
    public partial class MainWindow : Window
    {
        public class Product : INotifyPropertyChanged
        {
            private string name;
            private DateTime date;
            private int quantity;

            public string Name
            {
                get { return name; }
                set
                {
                    name = value;
                    NotifyPropertyChanged();
                }
            }

            public DateTime Date
            {
                get { return date; }
                set
                {
                    date = value;
                    NotifyPropertyChanged();
                }
            }

            public int Quantity
            {
                get { return quantity; }
                set
                {
                    quantity = value;
                    NotifyPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public List<Product> Products { get; set; } = new List<Product>();
        private const string dataFilePath = "data.json";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadData();
            auth auth = new auth();
            auth.Close();
        }

        private void SaveData()
        {
            string jsonData = JsonConvert.SerializeObject(Products);
            File.WriteAllText(dataFilePath, jsonData);
        }
        private void UpdateProductsListBox()
        {
            // Группировка продуктов по имени и суммирование количества
            var groupedProducts = Products
                .GroupBy(p => p.Name)
                .Select(group => new Product
                {
                    Name = group.Key,
                    Quantity = group.Sum(p => p.Quantity)
                });

            // Обновление списка продуктов в ListBox
            productsListBox.ItemsSource = null;
            productsListBox.ItemsSource = groupedProducts;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime currentDate = DateTime.Now;
            int quantity = int.Parse(quantityTextBox.Text);

            Product product = new Product
            {
                Name = productNameTextBox.Text,
                Date = currentDate,
                Quantity = quantity
            };

            Products.Add(product);
            SaveData();
            UpdateProductsListBox(); // Обновление ListBox после добавления продукта
        }

        private void RemoveProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (productsListBox.SelectedItem != null)
            {
                Product selectedProduct = (Product)productsListBox.SelectedItem;
                string productName = selectedProduct.Name;
                MessageBoxResult result = MessageBox.Show($"Удалить все данные о товаре {productName}?", "Удаление", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    int n = 0;

                    // Создаем временную коллекцию для хранения элементов, которые нужно удалить
                    List<Product> productsToRemove = new List<Product>();

                    // Проходим по коллекции и добавляем в список элементы с нужным именем
                    foreach (Product product in Products)
                    {
                        if (product.Name == productName)
                        {
                            productsToRemove.Add(product);
                        }
                    }

                    // Удаляем все найденные элементы из основной коллекции
                    foreach (Product productToRemove in productsToRemove)
                    {
                        Products.Remove(productToRemove);
                        n++;
                    }

                    MessageBox.Show($"Удалено все движения товара {productName} в количестве {n} штук");
                    SaveData();
                    UpdateProductsListBox(); // Обновление ListBox после удаления продуктов
                }         
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления.", "Удаление");
            }
        }

        private void LoadData()
        {
            if (File.Exists(dataFilePath))
            {
                string jsonData = File.ReadAllText(dataFilePath);
                Products = JsonConvert.DeserializeObject<List<Product>>(jsonData);
                UpdateProductsListBox(); // Обновление ListBox после загрузки данных
            }
            else
            {
                SaveData();
            }
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            switch (reportPeriodComboBox.SelectedIndex)
            {
                case 0: // Day
                    startDate = DateTime.Today;
                    break;
                case 1: // Week
                    startDate = DateTime.Today.AddDays(-7);
                    break;
                case 2: // Month
                    startDate = DateTime.Today.AddDays(-30);
                    break;
            }

            List<Product> filteredProducts = Products.Where(p => p.Date >= startDate && p.Date <= endDate).ToList();

            string report = "Отчёт движения товаров:\n\n";
            foreach (Product product in filteredProducts)
            {
                report += $"Товар: {product.Name}, Дата: {product.Date}, Кол-во: {product.Quantity}\n";
            }

            MessageBox.Show(report);
        }

        private void productsListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (productsListBox.SelectedItem != null)
            {
                Product selectedProduct = (Product)productsListBox.SelectedItem;
                productNameTextBox.Text = selectedProduct.Name;
                productNameTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void productNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (productNameTextBox.Text == "Товар")
            {
                productNameTextBox.Text = "";
                productNameTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void productNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (productNameTextBox.Text == "")
            {
                productNameTextBox.Text = "Товар";
                productNameTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void quantityTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (quantityTextBox.Text == "Кол-во")
            {
                quantityTextBox.Text = "";
                quantityTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void quantityTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (quantityTextBox.Text == "")
            {
                quantityTextBox.Text = "Кол-во";
                quantityTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}
