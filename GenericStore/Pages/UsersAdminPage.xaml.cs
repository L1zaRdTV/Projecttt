using GenericStore.AppData;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GenericStore.Pages
{
    public partial class UsersAdminPage : Page
    {
        public UsersAdminPage()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            UsersGrid.ItemsSource = AppConnect.model0db.Users.OrderBy(x => x.NameUser).ToList();
            UsersGrid.SelectedItem = null;
        }

        private void UsersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UsersGrid.SelectedItem is Users user) OpenEditor(user);
        }

        private void OpenEditor(Users user)
        {
            var window = new UserEditorWindow(user) { Owner = Window.GetWindow(this) };
            if (window.ShowDialog() == true) RefreshData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(UsersGrid.SelectedItem is Users selectedUser))
            {
                MessageBox.Show("Выберите пользователя для удаления");
                return;
            }

            var result = MessageBox.Show("Удалить выбранного пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                AppConnect.model0db.Users.Remove(selectedUser);
                AppConnect.model0db.SaveChanges();
                RefreshData();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show("Не удалось удалить пользователя. Возможно, с ним связаны корзины или заказы. Подробности: " + ex.GetBaseException().Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить пользователя: " + ex.GetBaseException().Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
