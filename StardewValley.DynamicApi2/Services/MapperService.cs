using System;
using Igorious.StardewValley.DynamicApi2.Events;
using StardewModdingAPI.Events;
using StardewValley;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public class MapperService
    {
        public static MapperService Instance { get; } = new MapperService();

        private bool IsMappingActivated { get; set; }
        private bool IsRun { get; set; }

        public void Run()
        {
            if (IsRun) return;
            IsRun = true;
            PlayerEvents.LoadedGame += (s, e) => ActivateMapping();
            SavingEvents.BeforeSaving += DeactivateMapping;
            SavingEvents.AfterSaving += ActivateMapping;
            LocationEvents.CurrentLocationChanged += OnCurrentLocationChanged;
            LocationEvents.LocationObjectsChanged += OnLocationObjectsChanged;
            InventoryEvents.ActiveObjectChanged += OnObjectChanged;
            InventoryEvents.CraftedObjectChanged += OnObjectChanged;
            PlayerEvents.InventoryChanged += OnInventoryChanged;
        }

        private void OnInventoryChanged(object s, EventArgsInventoryChanged e)
        {
            if (!IsMappingActivated) return;

            var inventory = Game1.player.Items;
            foreach (var addedItemInfo in e.Added)
            {
                var addedItem = addedItemInfo.Item as Object;
                var index = inventory.FindIndex(i => i == addedItem);
                inventory[index] = Wrap(addedItem);
            }
        }

        private void OnObjectChanged(ObjectEventArgs args)
        {
            if (!IsMappingActivated) return;

            args.Object = Wrap(args.Object);
        }

        private void ActivateMapping()
        {
            if (IsMappingActivated) return;
            IsMappingActivated = true;
            Console.WriteLine("Activating mapping...");
            Traverser.Instance.TraverseLocations(l => Traverser.Instance.TraverseLocation(l, Wrap));
            Traverser.Instance.TraverseInventory(Game1.player, Wrap);
            Console.WriteLine("Mapping is activated.");
        }

        private void DeactivateMapping()
        {
            if (!IsMappingActivated) return;
            IsMappingActivated = false;
            Console.WriteLine("Deactivating mapping...");
            Traverser.Instance.TraverseLocations(l => Traverser.Instance.TraverseLocation(l, Unwrap));
            Traverser.Instance.TraverseInventory(Game1.player, Unwrap);
            Console.WriteLine("Mapping is deactivated.");
        }

        private void OnLocationObjectsChanged(object sender, EventArgsLocationObjectsChanged e)
        {
            if (!IsMappingActivated) return;
            Traverser.Instance.TraverseLocation(Game1.currentLocation, Wrap);
        }

        private void OnCurrentLocationChanged(object sender, EventArgsCurrentLocationChanged e)
        {
            if (!IsMappingActivated) return;
            Traverser.Instance.TraverseLocation(Game1.currentLocation, Wrap);
        }

        private static Object Wrap(Object basicObject) => Wrapper.Instance.Wrap(basicObject);

        private static Object Unwrap(Object customObject) => Wrapper.Instance.Unwrap(customObject);
    }
}