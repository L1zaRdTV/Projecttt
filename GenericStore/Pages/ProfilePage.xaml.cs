using GenericStore.AppData;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GenericStore.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
        }

        private void LoadProfile()
        {
            CityBox.ItemsSource = AppConnect.model0db.Cities.OrderBy(x => x.NameCity).ToList();
            NameBox.Text = CurrentUser.User?.NameUser;
            EmailBox.Text = CurrentUser.User?.Email;
            CityBox.SelectedValue = CurrentUser.User?.IdCity;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new PageOutput());
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser.User == null)
                {
                    MessageBox.Show("Пользователь не авторизован.");
                    return;
                }

                if (String.IsNullOrWhiteSpace(NameBox.Text) || String.IsNullOrWhiteSpace(EmailBox.Text) || CityBox.SelectedValue == null)
                {
                    MessageBox.Show("Заполните логин, email и город.");
                    return;
                }

                if (!ValidationPhoneAndEmail.IsValidEmail(EmailBox.Text))
                {
                    MessageBox.Show("Некорректный email.");
                    return;
                }

                int userId = CurrentUser.User.IdUser;
                if (AppConnect.model0db.Users.Any(x => x.IdUser != userId && x.NameUser == NameBox.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже есть.");
                    return;
                }

                if (AppConnect.model0db.Users.Any(x => x.IdUser != userId && x.Email == EmailBox.Text))
                {
                    MessageBox.Show("Эта почта уже занята.");
                    return;
                }

                Users user = AppConnect.model0db.Users.First(x => x.IdUser == userId);
                user.NameUser = NameBox.Text.Trim();
                user.Email = EmailBox.Text.Trim();
                user.IdCity = (int)CityBox.SelectedValue;

                if (!String.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    if (PasswordBox.Password.Length < 6)
                    {
                        MessageBox.Show("Пароль должен быть не менее 6 символов.");
                        return;
                    }

                    user.Password = PasswordBox.Password;
                }

                AppConnect.model0db.SaveChanges();
                CurrentUser.User = user;
                MessageBox.Show("Профиль сохранен.");
                AppFrame.framemain.Navigate(new PageOutput());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить профиль: " + ex.Message);
            }
        }
    }
}
