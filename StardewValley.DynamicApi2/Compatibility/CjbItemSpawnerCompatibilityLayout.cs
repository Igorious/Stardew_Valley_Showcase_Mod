using System;
using System.Collections.Generic;
using System.Reflection;
using Igorious.StardewValley.DynamicApi2.Extensions;
using Igorious.StardewValley.DynamicApi2.Services;
using Igorious.StardewValley.DynamicApi2.Utils;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.DynamicApi2.Compatibility
{
    internal sealed class CjbItemSpawnerCompatibilityLayout
    {
        private const string CjbItemSpawnerID = "CJBItemSpawner";

        private static readonly Lazy<CjbItemSpawnerCompatibilityLayout> Lazy = new Lazy<CjbItemSpawnerCompatibilityLayout>(() => new CjbItemSpawnerCompatibilityLayout());
        public static CjbItemSpawnerCompatibilityLayout Instance => Lazy.Value;

        private CjbItemSpawnerCompatibilityLayout() { }

        public void Initialize()
        {
            if (!Smapi.GetModRegistry().IsLoaded(CjbItemSpawnerID)) return;
            MenuEvents.MenuChanged += OnMenuChanged;
        }

        private IClickableMenu CurrentMenu { get; set; }

        private void OnMenuChanged(object s, EventArgsClickableMenuChanged e)
        {
            if (e.NewMenu?.GetType().FullName != "CJBItemSpawner.ItemMenu") return;

            Log.Trace("Overriding CJB Item Spawner menu items...");

            CurrentMenu = e.NewMenu;
            var itemList = CurrentMenu.GetType().GetField<List<Item>>("itemList");
            for (var i = 0; i < itemList.Count; ++i)
            {
                var item = itemList[i] as Object;
                if (item == null) continue;
                itemList[i] = Wrapper.Instance.Wrap(item);
            }

            var loadInventory = CurrentMenu.GetType().GetMethod("loadInventory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            loadInventory.Invoke(CurrentMenu, null);

            Log.Trace("Overrided CJB Item Spawner menu items.");

            MenuEvents.MenuClosed += OnMenuClosed;
            GameEvents.UpdateTick += OnMenuUpdateTick;
        }

        private void OnMenuUpdateTick(object sender, EventArgs e)
        {
            var heldItem = CurrentMenu.GetField<Object>("heldItem");
            if (heldItem == null) return;
            var wrapped = Wrapper.Instance.Wrap(heldItem);
            if (heldItem == wrapped) return;
            CurrentMenu.SetField("heldItem", wrapped);
            Log.Trace("Wrapped item under cursor.");
        }

        private void OnMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            GameEvents.UpdateTick -= OnMenuUpdateTick;
            CurrentMenu = null;
        }
    }
}