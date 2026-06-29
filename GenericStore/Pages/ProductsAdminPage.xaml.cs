using GenericStore.AppData;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GenericStore.Pages
{
    public partial class ProductsAdminPage : Page
    {
        public ProductsAdminPage(Catalogs catalog)
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            ProductsGrid.ItemsSource = AppConnect.model0db.Catalogs.OrderBy(x => x.Product).ToList();
            ProductsGrid.SelectedItem = null;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) => OpenEditor(null);

        private void ProductsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProductsGrid.SelectedItem is Catalogs product) OpenEditor(product);
        }

        private void OpenEditor(Catalogs product)
        {
            var window = new ProductEditorWindow(product) { Owner = Window.GetWindow(this) };
            if (window.ShowDialog() == true) RefreshData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(ProductsGrid.SelectedItem is Catalogs selectedProduct))
            {
                MessageBox.Show("Выберите комнату для удаления");
                return;
            }

            var result = MessageBox.Show("Удалить выбранную комнату?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                AppConnect.model0db.Catalogs.Remove(selectedProduct);
                AppConnect.model0db.SaveChanges();
                RefreshData();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show("Не удалось удалить комнату. Возможно, она используется в бронированиях. Подробности: " + ex.GetBaseException().Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить комнату: " + ex.GetBaseException().Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
