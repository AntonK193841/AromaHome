using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace WpfApp6
{
    public partial class EditMaterialWindow : Window
    {
        private readonly int? materialId;

        private readonly CultureInfo russianCulture =
            new CultureInfo("ru-RU");

        public EditMaterialWindow(int? id = null)
        {
            InitializeComponent();

            materialId = id;

            LoadMaterialTypes();

            if (materialId.HasValue)
            {
                Title = "Редактирование материала";
                FormTitle.Text = "Редактирование материала";

                LoadMaterial();
            }
            else
            {
                Title = "Добавление материала";
                FormTitle.Text = "Добавление материала";

                PackageCountTextBox.Text = "1";
                WarehouseCountTextBox.Text = "0";
                MinimumCountTextBox.Text = "0";
                PriceTextBox.Text = "0,00";
            }
        }

        private void LoadMaterialTypes()
        {
            try
            {
                using (AromaHomeEntities context =
                    new AromaHomeEntities())
                {
                    var types = context.Materials
                        .Select(item => item.Type)
                        .Distinct()
                        .OrderBy(type => type)
                        .ToList();

                    TypeComboBox.ItemsSource = types;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Не удалось загрузить типы материалов. "
                    + "Проверьте подключение к базе данных.\n\n"
                    + exception.GetBaseException().Message,
                    "Ошибка загрузки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadMaterial()
        {
            try
            {
                using (AromaHomeEntities context =
                    new AromaHomeEntities())
                {
                    Materials material =
                        context.Materials.Find(materialId.Value);

                    if (material == null)
                    {
                        MessageBox.Show(
                            "Выбранный материал не найден. "
                            + "Обновите список и выберите материал снова.",
                            "Материал не найден",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        SaveButton.IsEnabled = false;
                        return;
                    }

                    TypeComboBox.Text = material.Type;
                    TitleTextBox.Text = material.Title;

                    PriceTextBox.Text =
                        material.Price.ToString("0.00", russianCulture);

                    UnitTextBox.Text = material.UnitOfMeasurement;

                    PackageCountTextBox.Text =
                        material.CountInPackage.ToString(russianCulture);

                    WarehouseCountTextBox.Text =
                        material.CountInWarehouse.ToString(russianCulture);

                    MinimumCountTextBox.Text =
                        material.MinPermissibleValue.ToString(russianCulture);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Не удалось загрузить данные материала.\n\n"
                    + exception.GetBaseException().Message,
                    "Ошибка загрузки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                SaveButton.IsEnabled = false;
            }
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string type = TypeComboBox.Text.Trim();
            string title = TitleTextBox.Text.Trim();
            string unit = UnitTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(type)
                || string.IsNullOrWhiteSpace(title)
                || string.IsNullOrWhiteSpace(unit))
            {
                ShowValidationError(
                    "Заполните тип материала, наименование "
                    + "и единицу измерения.");

                return;
            }

            decimal price;

            if (!decimal.TryParse(
                    PriceTextBox.Text.Trim(),
                    NumberStyles.Number,
                    russianCulture,
                    out price)
                && !decimal.TryParse(
                    PriceTextBox.Text.Trim(),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out price))
            {
                ShowValidationError(
                    "Цена должна быть числом. Например: 450,00.");

                PriceTextBox.Focus();
                return;
            }

            if (price < 0)
            {
                ShowValidationError(
                    "Цена не может быть отрицательной. "
                    + "Укажите ноль или положительное значение.");

                PriceTextBox.Focus();
                return;
            }

            if (decimal.Round(price, 2) != price)
            {
                ShowValidationError(
                    "Цена должна содержать не более "
                    + "двух знаков после запятой.");

                PriceTextBox.Focus();
                return;
            }

            int packageCount;
            int warehouseCount;
            int minimumCount;

            if (!TryReadNonNegativeInteger(
                    PackageCountTextBox.Text,
                    "Количество в упаковке",
                    out packageCount))
            {
                return;
            }

            if (!TryReadNonNegativeInteger(
                    WarehouseCountTextBox.Text,
                    "Количество на складе",
                    out warehouseCount))
            {
                return;
            }

            if (!TryReadNonNegativeInteger(
                    MinimumCountTextBox.Text,
                    "Минимальное количество",
                    out minimumCount))
            {
                return;
            }

            try
            {
                using (AromaHomeEntities context =
                    new AromaHomeEntities())
                {
                    Materials material;

                    if (materialId.HasValue)
                    {
                        material =
                            context.Materials.Find(materialId.Value);

                        if (material == null)
                        {
                            MessageBox.Show(
                                "Материал был удалён или больше "
                                + "не существует. Закройте форму "
                                + "и обновите список.",
                                "Материал не найден",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
                    }
                    else
                    {
                        material = new Materials();
                        context.Materials.Add(material);
                    }

                    material.Type = type;
                    material.Title = title;
                    material.Price = price;
                    material.UnitOfMeasurement = unit;
                    material.CountInPackage = packageCount;
                    material.CountInWarehouse = warehouseCount;
                    material.MinPermissibleValue = minimumCount;

                    context.SaveChanges();
                }

                MessageBox.Show(
                    "Данные материала успешно сохранены.",
                    "Сохранение выполнено",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Не удалось сохранить материал. "
                    + "Проверьте подключение к базе данных "
                    + "и корректность введённых данных.\n\n"
                    + exception.GetBaseException().Message,
                    "Ошибка сохранения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool TryReadNonNegativeInteger(
            string text,
            string fieldName,
            out int value)
        {
            if (!int.TryParse(
                    text.Trim(),
                    NumberStyles.Integer,
                    russianCulture,
                    out value)
                || value < 0)
            {
                ShowValidationError(
                    "Поле «" + fieldName
                    + "» должно содержать целое "
                    + "неотрицательное число.");

                return false;
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            MessageBox.Show(
                message,
                "Проверьте введённые данные",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (MessageBox.Show(
                    "Закрыть форму без сохранения изменений?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}