using Microsoft.EntityFrameworkCore;
using ORMapperSample.Data;
using ORMapperSample.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ORMapperSample
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _context;
        
        private ObservableCollection<Product> _products = new();
        private ObservableCollection<Category> _categories = new();
        private Product? _selectedProduct;
        private string _databaseProvider = string.Empty;

        public MainWindow(AppDbContext context)
        {
            _context = context;
            InitializeComponent();
            InitializeUI();
        }

        private async void InitializeUI()
        {
            await LoadProducts();
            await LoadCategories();
            GetDatabaseProvider();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            ProductsDataGrid.ItemsSource = _products;
            CategoriesComboBox.ItemsSource = _categories;
            DatabaseProviderTextBlock.Text = _databaseProvider;

            AddProductButton.Click += async (s, e) => await AddProduct();
            LoadProductsButton.Click += async (s, e) => await LoadProducts();
            UpdateProductButton.Click += async (s, e) => await UpdateProduct();
            DeleteProductButton.Click += async (s, e) => await DeleteProduct();
            InitializeDatabaseButton.Click += async (s, e) => await InitializeDatabase();
            
            ProductsDataGrid.SelectionChanged += (s, e) =>
            {
                _selectedProduct = ProductsDataGrid.SelectedItem as Product;
            };
        }

        private void GetDatabaseProvider()
        {
            if (_context.Database.IsSqlite())
            {
                _databaseProvider = "SQLite";
            }
            else if (_context.Database.IsMySql())
            {
                _databaseProvider = "MySQL";
            }
            else
            {
                _databaseProvider = "Unknown";
            }
            
            DatabaseProviderTextBlock.Text = _databaseProvider;
        }

        private async Task LoadProducts()
        {
            try
            {
                var entries = _context.ChangeTracker.Entries().ToList();
                foreach (var entry in entries)
                {
                    await entry.ReloadAsync();
                }
                
                var productList = await _context.Products.Include(p => p.Category).ToListAsync();
                _products.Clear();
                foreach (var product in productList)
                {
                    _products.Add(product);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"データの読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadCategories()
        {
            try
            {
                var categoryList = await _context.Categories.ToListAsync();
                _categories.Clear();
                foreach (var category in categoryList)
                {
                    _categories.Add(category);
                }
                
                if (_categories.Any())
                {
                    CategoriesComboBox.SelectedItem = _categories.First();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"カテゴリの読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddProduct()
        {
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                MessageBox.Show("商品名を入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoriesComboBox.SelectedItem is not Category selectedCategory)
            {
                MessageBox.Show("カテゴリを選択してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!decimal.TryParse(ProductPriceTextBox.Text, out decimal price))
                {
                    price = 0;
                }

                if (!int.TryParse(ProductQuantityTextBox.Text, out int quantity))
                {
                    quantity = 0;
                }

                var product = new Product
                {
                    Name = ProductNameTextBox.Text,
                    Description = ProductDescriptionTextBox.Text,
                    Price = price,
                    Quantity = quantity,
                    CategoryId = selectedCategory.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                await LoadProducts();

                ProductNameTextBox.Text = string.Empty;
                ProductDescriptionTextBox.Text = string.Empty;
                ProductPriceTextBox.Text = string.Empty;
                ProductQuantityTextBox.Text = string.Empty;

                MessageBox.Show("商品を追加しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"商品の追加に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateProduct()
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("更新する商品を選択してください。", "選択エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _context.Update(_selectedProduct);
                await _context.SaveChangesAsync();
                
                MessageBox.Show("商品を更新しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"商品の更新に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteProduct()
        {
            if (_selectedProduct == null)
            {
                MessageBox.Show("削除する商品を選択してください。", "選択エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"「{_selectedProduct.Name}」を削除してもよろしいですか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _context.Products.Remove(_selectedProduct);
                await _context.SaveChangesAsync();

                _products.Remove(_selectedProduct);
                _selectedProduct = null;

                MessageBox.Show("商品を削除しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"商品の削除に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task InitializeDatabase()
        {
            try
            {
                _context.ChangeTracker.Clear();
                
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.MigrateAsync();
                await LoadCategories();
                await LoadProducts();
                MessageBox.Show("データベースを初期化しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"データベースの初期化に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}