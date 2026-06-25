using GenericStore.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GenericStore.Pages
{
    public partial class BasketPage : Page
    {
        private List<BasketItemViewModel> _items = new List<BasketItemViewModel>();

        public BasketPage()
        {
            InitializeComponent();
            LoadBasket();
        }

        private void LoadBasket()
        {
            try
            {
                _items = BasketManager.GetCurrentBasketItems();
                BasketGrid.ItemsSource = _items;
                TotalTextBlock.Text = $"Итого: {_items.Sum(x => x.PositionTotal):N2} ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки корзины: " + ex.Message);
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            BasketItemViewModel item = (sender as Button)?.DataContext as BasketItemViewModel;
            if (item == null) return;

            BasketManager.RemoveItem(item.IdBasketCatalog);
            LoadBasket();
        }

        private void ClearBasketButton_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Корзина уже пуста.");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Очистить корзину?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            BasketManager.ClearCurrentBasket();
            LoadBasket();
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_items.Count == 0)
                {
                    MessageBox.Show("Корзина пуста. Добавьте товары перед оформлением заказа.");
                    return;
                }

                BasketManager.EnsureMoneyColumnsPrecision();
                Baskets basket = BasketManager.GetOrCreateCurrentBasket();
                decimal total = BasketManager.NormalizeMoney(_items.Sum(x => x.PositionTotal));

                using (var transaction = AppConnect.model0db.Database.BeginTransaction())
                {
                    Orders order = new Orders
                    {
                        IdUser = CurrentUser.User.IdUser,
                        Data = DateTime.Now,
                        Price = total,
                        IdStatusOrder = GetDefaultStatusId()
                    };

                    AppConnect.model0db.Orders.Add(order);
                    AppConnect.model0db.SaveChanges();

                    foreach (BasketItemViewModel item in _items)
                    {
                        AppConnect.model0db.OrdersCatalogs.Add(new OrdersCatalogs
                        {
                            IdOrder = order.IdOrder,
                            IdCatalog = item.IdCatalog,
                            Quantity = item.Quantity
                        });
                    }

                    basket.IsOrdered = true;
                    basket.TotalPrice = total;

                    AppConnect.model0db.SaveChanges();
                    transaction.Commit();

                    MessageBox.Show($"Заказ №{order.IdOrder} успешно оформлен.", 
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LoadBasket();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось оформить заказ: " + ex.GetBaseException().Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetDefaultStatusId()
        {
            StatusOrders status = AppConnect.model0db.StatusOrders
                .OrderBy(x => x.IdStatusOrder).FirstOrDefault();
            if (status == null)
                throw new InvalidOperationException("Не найдено ни одного статуса заказа в базе данных.");

            return status.IdStatusOrder;
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new PageOutput());
        }
    }
}
