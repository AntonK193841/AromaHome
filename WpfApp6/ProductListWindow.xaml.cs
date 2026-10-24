using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace WpfApp6
{
    public partial class ProductListWindow : Window
    {
        private readonly int materialId;

        private readonly CultureInfo russianCulture =
            new CultureInfo("ru-RU");

        public ProductListWindow(int selectedMaterialId)
        {
            InitializeComponent();

            materialId = selectedMaterialId;

            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                using (AromaHomeEntities context =
                    new AromaHomeEntities())
                {
                    Materials material =
                        context.Materials.Find(materialId);

                    if (material == null)
                    {
                        MessageBox.Show(
                            "Выбранный материал не найден. "
                            + "Вернитесь к списку материалов "
                            + "и выберите существующий материал.",
                            "Материал не найден",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        ProductsListView.ItemsSource =
                            new List<ProductMaterialItem>();

                        StatusText.Text = "Материал не найден.";
                        return;
                    }

                    MaterialTitleText.Text = material.Title;

                    var products = context.ListsOfMaterials
                        .Where(recipe => recipe.MaterialId == materialId)
                        .Select(recipe => new ProductMaterialItem
                        {
                            ProductName = recipe.Products.Title,
                            Quantity = recipe.Count
                        })
                        .OrderBy(item => item.ProductName)
                        .ToList();

                    foreach (ProductMaterialItem product in products)
                    {
                        product.QuantityText =
                            Convert.ToDecimal(product.Quantity)
                                .ToString("N2", russianCulture)
                            + " "
                            + (material.UnitOfMeasurement ?? "");
                    }

                    ProductsListView.ItemsSource = products;

                    StatusText.Text = products.Count == 0
                        ? "Для этого материала продукция не найдена."
                        : "Найдено видов продукции: " + products.Count;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Не удалось загрузить продукцию, "
                    + "использующую выбранный материал.\n\n"
                    + exception.GetBaseException().Message
                    + "\n\nПроверьте подключение к базе данных.",
                    "Ошибка загрузки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                StatusText.Text = "Ошибка загрузки данных.";
            }
        }

        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }
    }

    public class ProductMaterialItem
    {
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public string QuantityText { get; set; }
    }
}