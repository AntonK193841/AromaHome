
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp6
{
    public partial class MainWindow : Window
    {
        private readonly AromaHomeEntities _db = new AromaHomeEntities();

        public MainWindow()
        {
            InitializeComponent();
            LoadMaterials();
        }

        public class MaterialCard
        {
            public int Id { get; set; }

            public string Title { get; set; }

            public string Type { get; set; }

            public string PriceText { get; set; }

            public string MinimumText { get; set; }

            public string WarehouseText { get; set; }

            public string PackageText { get; set; }

            public string RequiredText { get; set; }
        }

        private void LoadMaterials()
        {
            try
            {
                List<MaterialCard> cards = _db.Materials
                    .ToList()
                    .Select(material =>
                    {
                        string unit = material.UnitOfMeasurement ?? "";

                        string packageText =
                            "Количество в упаковке: " +
                            material.CountInPackage.ToString("N2") +
                            (string.IsNullOrWhiteSpace(unit) ? "" : " " + unit);

                        string requiredText;

                        try
                        {
                            var recipes = _db.ListsOfMaterials
                                .Where(recipe => recipe.MaterialId == material.Id)
                                .ToList();

                            if (recipes.Count == 0)
                            {
                                requiredText = "Используется в рецептурах: нет данных";
                            }
                            else
                            {
                                requiredText = "Используется в рецептурах: " +
                                    recipes.Count + " записей";
                            }
                        }
                        catch
                        {
                            requiredText = "";
                        }

                        return new MaterialCard
                        {
                            Id = material.Id,
                            Title = material.Title ?? "",
                            Type = material.Type ?? "",
                            PriceText = Convert.ToDecimal(material.Price)
                                .ToString("N2") + " ₽",
                            MinimumText = Convert.ToDecimal(
                                material.MinPermissibleValue).ToString("N2"),
                            WarehouseText = Convert.ToDecimal(
                                material.CountInWarehouse).ToString("N2"),
                            PackageText = packageText,
                            RequiredText = requiredText
                        };
                    })
                    .OrderBy(card => card.Title)
                    .ToList();

                MaterialsItemsControl.ItemsSource = cards;

                StatusText.Text = "Материалов загружено: " + cards.Count;
            }
            catch (Exception ex)
            {
                StatusText.Text = "Не удалось загрузить материалы.";

                MessageBox.Show(
                    "Ошибка при загрузке материалов:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMaterials();
        }

        private void AddMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EditMaterialWindow window = new EditMaterialWindow
                {
                    Owner = this
                };

                bool? result = window.ShowDialog();

                if (result == true)
                {
                    LoadMaterials();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось открыть форму добавления материала:\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EditMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null || button.Tag == null)
                return;

            if (!int.TryParse(button.Tag.ToString(), out int materialId))
                return;

            try
            {
                EditMaterialWindow window =
                    new EditMaterialWindow(materialId)
                    {
                        Owner = this
                    };

                bool? result = window.ShowDialog();

                if (result == true)
                {
                    LoadMaterials();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось открыть форму редактирования:\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ShowProductsButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null || button.Tag == null)
                return;

            if (!int.TryParse(button.Tag.ToString(), out int materialId))
                return;

            try
            {
                ProductListWindow window = new ProductListWindow(materialId)
                {
                    Owner = this
                };

                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось открыть список продукции:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _db.Dispose();
            base.OnClosed(e);
        }
    }
}
