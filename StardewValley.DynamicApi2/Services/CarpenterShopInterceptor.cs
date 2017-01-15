using System.Collections.Generic;
using Igorious.StardewValley.DynamicApi2.Extensions;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public sealed class CarpenterShopInterceptor
    {
        public bool IsRun { get; set; }
        public static CarpenterShopInterceptor Instance { get; } = new CarpenterShopInterceptor();

        private readonly IList<int> _addedFurniture = new List<int>();

        public void Run()
        {
            if (IsRun) return;
            IsRun = true;
            LocationEvents.CurrentLocationChanged += OnCurrentLocationChanged;
        }

        public void AddFurniture(int id)
        {
            _addedFurniture.Add(id);
        }

        private void OnCurrentLocationChanged(object sender, EventArgsCurrentLocationChanged args)
        {
            if (args.NewLocation.Name == "ScienceHouse")
            {
                MenuEvents.MenuChanged += OnMenuChanged;
            }
            else
            {
                MenuEvents.MenuChanged -= OnMenuChanged;
            }
        }

        private void OnMenuChanged(object sender, EventArgsClickableMenuChanged args)
        {
            if (!(args.NewMenu is ShopMenu)) return;

            var shopMenu = (ShopMenu)args.NewMenu;
            var itemsForSale = shopMenu.GetField<List<Item>>("forSale");
            var itemsPriceAndStock = shopMenu.GetField<Dictionary<Item, int[]>>("itemPriceAndStock");

            foreach (var newItem in _addedFurniture)
            {
                var item = new Furniture(newItem, Vector2.Zero);
                itemsForSale.Add(item);
                itemsPriceAndStock.Add(item, new[] { item.salePrice(), int.MaxValue });
            }
        }
    }
}