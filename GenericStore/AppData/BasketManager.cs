using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericStore.AppData
{
    public class BasketItemViewModel
    {
        public int IdBasketCatalog { get; set; }
        public int IdCatalog { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal PositionTotal => Price * Quantity;
    }

    public static class BasketManager
    {
        public static bool IsUserAuthorized()
        {
            return CurrentUser.User != null;
        }

        public static Baskets GetOrCreateCurrentBasket()
        {
            EnsureMoneyColumnsPrecision();

            if (!IsUserAuthorized())
            {
                throw new InvalidOperationException("Пользователь не авторизован.");
            }

            int userId = CurrentUser.User.IdUser;
            Baskets basket = AppConnect.model0db.Baskets
                .Include(x => x.BasketsCatalogs)
                .FirstOrDefault(x => x.IdUser == userId && !x.IsOrdered);

            if (basket != null)
            {
                return basket;
            }

            basket = new Baskets
            {
                IdUser = userId,
                CreateDate = DateTime.Now,
                IsOrdered = false,
                TotalPrice = 0m
            };

            AppConnect.model0db.Baskets.Add(basket);
            AppConnect.model0db.SaveChanges();
            return basket;
        }

        public static void AddToBasket(Catalogs product, int quantity = 1)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (quantity <= 0)
            {
                quantity = 1;
            }

            Baskets basket = GetOrCreateCurrentBasket();

            BasketsCatalogs basketItem = AppConnect.model0db.BasketsCatalogs
                .FirstOrDefault(x => x.IdBasket == basket.IdBasket && x.IdCatalog == product.IdCatalog);

            if (basketItem == null)
            {
                basketItem = new BasketsCatalogs
                {
                    IdBasket = basket.IdBasket,
                    IdCatalog = product.IdCatalog,
                    Quantity = quantity
                };
                AppConnect.model0db.BasketsCatalogs.Add(basketItem);
            }
            else
            {
                basketItem.Quantity += quantity;
            }

            RecalculateBasketTotal(basket.IdBasket);
            AppConnect.model0db.SaveChanges();
        }

        public static List<BasketItemViewModel> GetCurrentBasketItems()
        {
            Baskets basket = GetOrCreateCurrentBasket();

            List<BasketItemViewModel> items = AppConnect.model0db.BasketsCatalogs
                .Where(x => x.IdBasket == basket.IdBasket)
                .Select(x => new BasketItemViewModel
                {
                    IdBasketCatalog = x.IdBasketCatalog,
                    IdCatalog = x.IdCatalog,
                    ProductName = x.Catalogs.Product,
                    Price = x.Catalogs.Price,
                    Quantity = x.Quantity
                })
                .ToList();

            return items;
        }

        public static decimal GetCurrentBasketTotal()
        {
            Baskets basket = GetOrCreateCurrentBasket();
            RecalculateBasketTotal(basket.IdBasket);
            return basket.TotalPrice;
        }

        public static void RemoveItem(int basketCatalogId)
        {
            BasketsCatalogs item = AppConnect.model0db.BasketsCatalogs.FirstOrDefault(x => x.IdBasketCatalog == basketCatalogId);
            if (item == null)
            {
                return;
            }

            int basketId = item.IdBasket;
            AppConnect.model0db.BasketsCatalogs.Remove(item);
            AppConnect.model0db.SaveChanges();
            RecalculateBasketTotal(basketId);
            AppConnect.model0db.SaveChanges();
        }

        public static void ClearCurrentBasket()
        {
            Baskets basket = GetOrCreateCurrentBasket();
            ClearBasketItems(basket.IdBasket);
        }

        public static void ClearBasketItems(int basketId)
        {
            Baskets basket = AppConnect.model0db.Baskets.FirstOrDefault(x => x.IdBasket == basketId);
            if (basket == null)
            {
                return;
            }

            List<BasketsCatalogs> items = AppConnect.model0db.BasketsCatalogs.Where(x => x.IdBasket == basketId).ToList();
            if (items.Count > 0)
            {
                AppConnect.model0db.BasketsCatalogs.RemoveRange(items);
            }

            basket.TotalPrice = 0m;
            AppConnect.model0db.SaveChanges();
        }

        public static void RecalculateBasketTotal(int basketId)
        {
            Baskets basket = AppConnect.model0db.Baskets.FirstOrDefault(x => x.IdBasket == basketId);
            if (basket == null)
            {
                return;
            }

            decimal total = AppConnect.model0db.BasketsCatalogs
                .Where(x => x.IdBasket == basketId)
                .Select(x => x.Catalogs.Price * x.Quantity)
                .DefaultIfEmpty(0m)
                .Sum();

            basket.TotalPrice = NormalizeMoney(total);
        }

        public static void EnsureMoneyColumnsPrecision()
        {
            try
            {
                AppConnect.model0db.Database.ExecuteSqlCommand(
                    "IF COL_LENGTH('dbo.Orders', 'Price') IS NOT NULL ALTER TABLE dbo.Orders ALTER COLUMN Price decimal(10,2) NOT NULL; " +
                    "IF COL_LENGTH('dbo.Baskets', 'TotalPrice') IS NOT NULL ALTER TABLE dbo.Baskets ALTER COLUMN TotalPrice decimal(10,2) NOT NULL;");
            }
            catch
            {
                // If the application is not connected to SQL Server yet, the normal EF error handling will show the real problem later.
            }
        }

        public static decimal NormalizeMoney(decimal value)
        {
            if (value < 0m)
            {
                return 0m;
            }

            const decimal maxSupportedValue = 99999999.99m;
            if (value > maxSupportedValue)
            {
                return maxSupportedValue;
            }

            return Math.Round(value, 2);
        }
    }
}
