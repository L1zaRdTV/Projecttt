using GenericStore.AppData;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace GenericStore.Pages
{
    public partial class ProductEditorWindow : Window
    {
        private readonly Catalogs _product;
        private readonly bool _isNew;

        public ProductEditorWindow(Catalogs product = null)
        {
            InitializeComponent();
            _product = product ?? new Catalogs();
            _isNew = product == null;
            TitleText.Text = _isNew ? "Добавление комнаты" : "Редактирование комнаты";
            SaveButton.Content = _isNew ? "Добавить" : "Сохранить";
            CategoryBox.ItemsSource = AppConnect.model0db.Categories.OrderBy(x => x.NameCategory).ToList();
            FillForm();
        }

        private void FillForm()
        {
            ProductNameBox.Text = _product.Product ?? string.Empty;
            CategoryBox.SelectedValue = _product.IdCategory == 0 ? (int?)null : _product.IdCategory;
            PriceBox.Text = _product.Price == 0 ? string.Empty : _product.Price.ToString("0.##", CultureInfo.CurrentCulture);
            DescriptionBox.Text = _product.Descripton ?? string.Empty;
            PhotoPathBox.Text = _product.PhotoPath ?? string.Empty;
        }

        private bool ValidateForm()
        {
            string name = ProductNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) { MessageBox.Show("Введите название комнаты"); return false; }
            bool duplicate = AppConnect.model0db.Catalogs.Any(x => x.Product == name && x.IdCatalog != _product.IdCatalog);
            if (duplicate) { MessageBox.Show("Комната с таким названием уже существует!"); return false; }
            if (CategoryBox.SelectedValue == null) { MessageBox.Show("Выберите стиль оформления"); return false; }
            if (string.IsNullOrWhiteSpace(DescriptionBox.Text)) { MessageBox.Show("Введите описание комнаты"); return false; }
            if (!decimal.TryParse(PriceBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal price) || price <= 0m)
            {
                MessageBox.Show("Стоимость аренды должна быть числом больше 0"); return false;
            }
            if (price > 99999999.99m) { MessageBox.Show("Цена слишком большая"); return false; }
            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm()) return;
                _product.Product = ProductNameBox.Text.Trim();
                _product.IdCategory = (int)CategoryBox.SelectedValue;
                _product.Price = Math.Round(decimal.Parse(PriceBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture), 2);
                _product.Descripton = DescriptionBox.Text.Trim();
                _product.PhotoPath = PhotoPathBox.Text.Trim();
                if (_isNew) AppConnect.model0db.Catalogs.Add(_product);
                AppConnect.model0db.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить комнату: " + ex.GetBaseException().Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void PhotoPathBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*", Title = "Выберите изображение" };
            if (dialog.ShowDialog() == true)
            {
                string imagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Images\\");
                Directory.CreateDirectory(imagesDirectory);
                PhotoPathBox.Text = Path.GetFileName(dialog.FileName);
            }
        }
    }
}
