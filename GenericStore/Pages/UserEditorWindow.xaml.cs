using GenericStore.AppData;
using System;
using System.Linq;
using System.Windows;

namespace GenericStore.Pages
{
    public partial class UserEditorWindow : Window
    {
        private readonly Users _user;

        public UserEditorWindow(Users user)
        {
            InitializeComponent();
            _user = user ?? throw new ArgumentNullException(nameof(user));
            TitleText.Text = "Редактирование пользователя";
            RoleBox.ItemsSource = AppConnect.model0db.Roles.OrderBy(x => x.NameRole).ToList();
            CityBox.ItemsSource = AppConnect.model0db.Cities.OrderBy(x => x.NameCity).ToList();
            FillForm();
        }

        private void FillForm()
        {
            UserNameBox.Text = _user.NameUser ?? string.Empty;
            EmailBox.Text = _user.Email ?? string.Empty;
            PasswordBox.Text = _user.Password ?? string.Empty;
            RoleBox.SelectedValue = _user.IdRole;
            CityBox.SelectedValue = _user.IdCity;
        }

        private bool ValidateForm()
        {
            string userName = UserNameBox.Text.Trim();
            string email = EmailBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userName)) { MessageBox.Show("Введите имя пользователя"); return false; }
            if (string.IsNullOrWhiteSpace(email) || !ValidationPhoneAndEmail.IsValidEmail(email)) { MessageBox.Show("Введите корректный email"); return false; }
            if (string.IsNullOrWhiteSpace(PasswordBox.Text) || PasswordBox.Text.Length < 6) { MessageBox.Show("Пароль должен быть не менее 6 символов"); return false; }
            if (RoleBox.SelectedValue == null) { MessageBox.Show("Выберите роль"); return false; }
            if (CityBox.SelectedValue == null) { MessageBox.Show("Выберите город"); return false; }
            if (AppConnect.model0db.Users.Any(x => x.IdUser != _user.IdUser && x.NameUser == userName)) { MessageBox.Show("Пользователь с таким логином уже есть."); return false; }
            if (AppConnect.model0db.Users.Any(x => x.IdUser != _user.IdUser && x.Email == email)) { MessageBox.Show("Эта почта уже занята."); return false; }
            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm()) return;
                _user.NameUser = UserNameBox.Text.Trim();
                _user.Email = EmailBox.Text.Trim();
                _user.Password = PasswordBox.Text;
                _user.IdRole = (int)RoleBox.SelectedValue;
                _user.IdCity = (int)CityBox.SelectedValue;
                AppConnect.model0db.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить пользователя: " + ex.GetBaseException().Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
