using System.Collections.Generic;
using Igorious.StardewValley.DynamicApi2.Extensions;
using StardewValley;
using StardewValley.Menus;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public sealed class ShopMenuProxy
    {
        public ShopMenuProxy(ShopMenu menu)
        {
            ItemsForSale = menu.GetField<List<Item>>("forSale");
            ItemsPriceAndStock = menu.GetField<Dictionary<Item, int[]>>("itemPriceAndStock");
        }

        public Dictionary<Item, int[]> ItemsPriceAndStock { get; }
        public List<Item> ItemsForSale { get; }

        public void AddItem(Item item, int stack = int.MaxValue, int? price = null)
        {
            ItemsForSale.Add(item);
            ItemsPriceAndStock.Add(item, new[] { price ?? item.salePrice(), stack });
        }
    }
}