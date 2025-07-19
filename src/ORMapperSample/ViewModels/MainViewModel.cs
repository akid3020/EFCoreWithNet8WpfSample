using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ORMapperSample.Data;
using ORMapperSample.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace ORMapperSample.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly AppDbContext _context;

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private string newProductName = string.Empty;

    [ObservableProperty]
    private string newProductDescription = string.Empty;

    [ObservableProperty]
    private decimal newProductPrice;

    [ObservableProperty]
    private int newProductQuantity;

    [ObservableProperty]
    private Category? newProductCategory;

    [ObservableProperty]
    private string databaseProvider = string.Empty;

    public MainViewModel(AppDbContext context)
    {
        _context = context;
        LoadProducts();
        LoadCategories();
        GetDatabaseProvider();
    }

    private void GetDatabaseProvider()
    {
        if (_context.Database.IsSqlite())
        {
            DatabaseProvider = "SQLite";
        }
        else if (_context.Database.IsMySql())
        {
            DatabaseProvider = "MySQL";
        }
        else
        {
            DatabaseProvider = "Unknown";
        }
    }

    [RelayCommand]
    private async Task LoadProducts()
    {
        try
        {
            // 既存のエンティティの変更を破棄して、データベースから再読み込み
            var entries = _context.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                await entry.ReloadAsync();
            }
            
            var productList = await _context.Products.Include(p => p.Category).ToListAsync();
            Products.Clear();
            foreach (var product in productList)
            {
                Products.Add(product);
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
            Categories.Clear();
            foreach (var category in categoryList)
            {
                Categories.Add(category);
            }
            
            // デフォルトカテゴリを設定
            if (Categories.Any())
            {
                NewProductCategory = Categories.First();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"カテゴリの読み込みに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task AddProduct()
    {
        if (string.IsNullOrWhiteSpace(NewProductName))
        {
            MessageBox.Show("商品名を入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NewProductCategory == null)
        {
            MessageBox.Show("カテゴリを選択してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var product = new Product
            {
                Name = NewProductName,
                Description = NewProductDescription,
                Price = NewProductPrice,
                Quantity = NewProductQuantity,
                CategoryId = NewProductCategory.Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // カテゴリ情報を含めて再読み込み
            await LoadProducts();

            // 入力フィールドをクリア
            NewProductName = string.Empty;
            NewProductDescription = string.Empty;
            NewProductPrice = 0;
            NewProductQuantity = 0;

            MessageBox.Show("商品を追加しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"商品の追加に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task UpdateProduct()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("更新する商品を選択してください。", "選択エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _context.Update(SelectedProduct);
            await _context.SaveChangesAsync();
            
            MessageBox.Show("商品を更新しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"商品の更新に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteProduct()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("削除する商品を選択してください。", "選択エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"「{SelectedProduct.Name}」を削除してもよろしいですか？", "削除確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            _context.Products.Remove(SelectedProduct);
            await _context.SaveChangesAsync();

            Products.Remove(SelectedProduct);
            SelectedProduct = null;

            MessageBox.Show("商品を削除しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"商品の削除に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task InitializeDatabase()
    {
        try
        {
            // ChangeTrackerをクリアしてからデータベースを初期化
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