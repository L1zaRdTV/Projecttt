using GenericStore.AppData;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GenericStore.Pages
{
    public partial class ProductDetailsPage : Page
    {
        private readonly Catalogs product;

        public ProductDetailsPage(Catalogs product)
        {
            InitializeComponent();
            this.product = product;
            DataContext = product;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.framemain.CanGoBack)
            {
                AppFrame.framemain.GoBack();
            }
            else
            {
                AppFrame.framemain.Navigate(new PageOutput());
            }
        }

        private void AddToBasketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (product == null)
                {
                    MessageBox.Show("Товар не выбран.");
                    return;
                }

                BasketManager.AddToBasket(product);
                decimal total = BasketManager.GetCurrentBasketTotal();
                MessageBox.Show($"Товар \"{product.Product}\" добавлен в корзину. Текущая сумма: {total:N2} ₽");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось добавить товар в корзину: " + ex.Message);
            }
        }
    }
}
