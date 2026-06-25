using GenericStore.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GenericStore.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageOutput.xaml
    /// </summary>
    public partial class PageOutput : Page
    {
        public PageOutput()
        {
            InitializeComponent();
            Fill();
            ListProducts.ItemsSource = CatalogsList();
        }

        public void Fill()
        {
            ComboSort.Items.Add("Без сортировки");
            ComboSort.Items.Add("По возрастанию");
            ComboSort.Items.Add("По убыванию");
            ComboSort.SelectedIndex = 0;

            ComboFilter.DisplayMemberPath = "NameCategory";
            ComboFilter.Items.Add(new Categories { IdCategory = 0, NameCategory = "Все категории" });
            foreach (var item in AppConnect.model0db.Categories.OrderBy(x => x.NameCategory))
            {
                ComboFilter.Items.Add(item);
            }
            ComboFilter.SelectedIndex = 0;
        }

        Catalogs[] CatalogsList()
        {
            try
            {
                List<Catalogs> catalogs = AppConnect.model0db.Catalogs.ToList();
                if (TextSearch != null)
                {
                    catalogs = catalogs.Where(x => x.Product.ToLower().Contains(TextSearch.Text.ToLower())).ToList();
                }

                if (ComboFilter.SelectedItem is Categories selectedCategory && selectedCategory.IdCategory > 0)
                {
                    catalogs = catalogs.Where(x => x.IdCategory == selectedCategory.IdCategory).ToList();
                }
                if (ComboSort.SelectedIndex > 0)
                {
                    switch (ComboSort.SelectedIndex)
                    {
                        case 1:
                            catalogs = catalogs.OrderBy(x => x.Price).ToList();
                            break;
                        case 2:
                            catalogs = catalogs.OrderByDescending(x => x.Price).ToList();
                            break;
                    }
                }
                if (catalogs.Count > 0)
                {
                    tbCounter.Text = "Найдено " + catalogs.Count + " рец.";
                }
                else
                {
                    tbCounter.Text = "Не найдено";
                }
                return catalogs.ToArray();
            }
            catch
            {
                MessageBox.Show("Повтори попытку позже");
                return null;
            }
        }

        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListProducts.ItemsSource = CatalogsList();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListProducts.ItemsSource = CatalogsList();
        }

        private void TextSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ListProducts.ItemsSource = CatalogsList();
        }

        private void AddProductsButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new AdminPage());
        }

        private void AddProductsButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AppData.CurrentUser.IsAdmin)
            {
                AddProductsButton.Visibility = Visibility.Hidden;
                AddProductsButton.IsEnabled = false;
            }
        }

        private void AddToBusketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                Catalogs selectedProduct = button?.DataContext as Catalogs;
                if (selectedProduct == null)
                {
                    MessageBox.Show("Не удалось определить выбранный товар.");
                    return;
                }

                Catalogs dbProduct = AppConnect.model0db.Catalogs.FirstOrDefault(x => x.IdCatalog == selectedProduct.IdCatalog);
                if (dbProduct == null)
                {
                    MessageBox.Show("Товар не найден в базе данных.");
                    return;
                }

                BasketManager.AddToBasket(dbProduct);
                decimal total = BasketManager.GetCurrentBasketTotal();
                MessageBox.Show($"Товар \"{dbProduct.Product}\" добавлен в корзину. Текущая сумма: {total:N2} ₽");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось добавить товар в корзину: " + ex.Message);
            }
        }

        private void GoToBasketButton_Click_1(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new BasketPage());
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new ProfilePage());
        }

        private void ListProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListProducts.SelectedItem is Catalogs selectedProduct)
            {
                AppFrame.framemain.Navigate(new ProductDetailsPage(selectedProduct));
            }
        }
    }
}