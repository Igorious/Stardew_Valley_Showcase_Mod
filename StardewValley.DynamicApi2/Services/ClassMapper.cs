﻿using System;
using Igorious.StardewValley.DynamicApi2.Compatibility;
using Igorious.StardewValley.DynamicApi2.Events;
using Igorious.StardewValley.DynamicApi2.Extensions;
using Igorious.StardewValley.DynamicApi2.Utils;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public class ClassMapper
    {
        private static readonly Lazy<ClassMapper> Lazy = new Lazy<ClassMapper>(() => new ClassMapper());
        public static ClassMapper Instance => Lazy.Value;

        private bool IsMappingActivated { get; set; }
        private ShopMenu CurrentShopMenu { get; set; }

        private ClassMapper()
        {
            SaveEvents.AfterLoad += (s, e) => ActivateMapping();
            SaveEvents.BeforeSave += (s, e) => DeactivateMapping();
            SaveEvents.AfterSave += (s, e) => ActivateMapping();
            LocationEvents.CurrentLocationChanged += OnCurrentLocationChanged;
            LocationEvents.LocationObjectsChanged += OnLocationObjectsChanged;
            InventoryEvents.ActiveObjectChanged += OnObjectChanged;
            InventoryEvents.CraftedObjectChanged += OnObjectChanged;
            PlayerEvents.InventoryChanged += OnInventoryChanged;
            MenuEvents.MenuChanged += MenuEventsOnMenuChanged;
            MenuEvents.MenuClosed += MenuEventsOnMenuClosed;
            ShopService.Instance.MenuItemsAdded += OnMenuItemsAdded;
            CjbItemSpawnerCompatibilityLayout.Instance.Initialize();
        }

        public ClassMap Map => ClassMap.Instance;

        private void ActivateMapping()
        {
            IsMappingActivated = true;
            Log.Trace("Activating mapping...");
            Traverser.Instance.TraverseLocations(l => Traverser.Instance.TraverseLocation(l, Wrap));
            Traverser.Instance.TraverseInventory(Game1.player, Wrap);
            Log.Trace("Mapping is activated.");
        }

        private void DeactivateMapping()
        {
            IsMappingActivated = false;
            Log.Trace("Deactivating mapping...");
            Traverser.Instance.TraverseLocations(l => Traverser.Instance.TraverseLocation(l, Unwrap));
            Traverser.Instance.TraverseInventory(Game1.player, Unwrap);
            Log.Trace("Mapping is deactivated.");
        }

        private void MenuEventsOnMenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (!(e.NewMenu is ShopMenu shopMenu)) return;

            CurrentShopMenu = shopMenu;
            WrapMenuItems();

            GameEvents.UpdateTick += OnMenuUpdateTick;
        }

        private void OnMenuItemsAdded(object sender, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu != CurrentShopMenu) return;
            WrapMenuItems();
        }

        private void WrapMenuItems()
        {
            var menuProxy = new ShopMenuProxy(CurrentShopMenu);
            for (var i = 0; i < menuProxy.ItemsForSale.Count; ++i)
            {
                var item = menuProxy.ItemsForSale[i] as Object;
                if (item == null) continue;

                var wrappedItem = Wrap(item);
                if (wrappedItem == item) continue;

                menuProxy.ItemsForSale[i] = wrappedItem;
                if (menuProxy.ItemsPriceAndStock.TryGetValue(item, out var value))
                {
                    menuProxy.ItemsPriceAndStock.Remove(item);
                    menuProxy.ItemsPriceAndStock.Add(wrappedItem, value);
                }
                Log.Trace("Wrapped item in menu.");
            }
        }

        private void OnMenuUpdateTick(object sender, EventArgs e)
        {
            var menuHeldItem = CurrentShopMenu.GetField<Object>("heldItem");
            if (menuHeldItem == null) return;

            var wrappedItem = Wrap(menuHeldItem);
            if (wrappedItem == menuHeldItem) return;

            CurrentShopMenu.SetField("heldItem", wrappedItem);
            Log.Trace("Wrapped item under cursor.");
        }

        private void MenuEventsOnMenuClosed(object sender, EventArgsClickableMenuClosed eventArgsClickableMenuClosed)
        {
            GameEvents.UpdateTick -= OnMenuUpdateTick;
        }

        private void OnInventoryChanged(object s, EventArgsInventoryChanged e)
        {
            if (!IsMappingActivated) return;
            var inventory = Game1.player.Items;
            foreach (var addedItemInfo in e.Added)
            {
                var addedItem = addedItemInfo.Item as Object;
                if (addedItem == null) return;
                var index = inventory.FindIndex(i => i == addedItem);
                var wrappedItem = Wrap(addedItem);
                if (addedItem == wrappedItem) continue;
                Log.Trace("Inventory object mapped.");
                inventory[index] = wrappedItem;
            }
        }

        private void OnObjectChanged(ObjectEventArgs args)
        {
            if (!IsMappingActivated) return;
            var wrappedItem = Wrap(args.Object);
            if (args.Object == wrappedItem) return;
            Log.Trace("Active object mapped.");
            args.Object = wrappedItem;
        }

        private void OnLocationObjectsChanged(object sender, EventArgsLocationObjectsChanged e)
        {
            if (!IsMappingActivated) return;
            Log.Trace("Location objects changed...");
            Traverser.Instance.TraverseLocation(Game1.currentLocation, Wrap);
        }

        private void OnCurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (!IsMappingActivated) return;
            Log.Trace("Location changed...");
            Traverser.Instance.TraverseLocation(Game1.currentLocation, Wrap);
        }

        private static Object Wrap(Object basicObject) => Wrapper.Instance.Wrap(basicObject);
        private static Object Unwrap(Object customObject) => Wrapper.Instance.Unwrap(customObject);
    }
}