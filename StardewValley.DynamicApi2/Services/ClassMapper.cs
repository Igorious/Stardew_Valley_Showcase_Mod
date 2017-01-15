using System;
using System.Collections.Generic;
using Igorious.StardewValley.DynamicApi2.Events;
using Igorious.StardewValley.DynamicApi2.Utils;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public class ClassMapper
    {
        private static readonly Lazy<ClassMapper> Lazy = new Lazy<ClassMapper>(() => new ClassMapper());
        public static ClassMapper Instance => Lazy.Value;

        private IDictionary<int, Type> FurnitureMapping { get; } = new Dictionary<int, Type>();
        private bool IsMappingActivated { get; set; }

        private ClassMapper()
        {
            PlayerEvents.LoadedGame += (s, e) => ActivateMapping();
            SavingEvents.BeforeSaving += DeactivateMapping;
            SavingEvents.AfterSaving += ActivateMapping;
            LocationEvents.CurrentLocationChanged += OnCurrentLocationChanged;
            LocationEvents.LocationObjectsChanged += OnLocationObjectsChanged;
            InventoryEvents.ActiveObjectChanged += OnObjectChanged;
            InventoryEvents.CraftedObjectChanged += OnObjectChanged;
            PlayerEvents.InventoryChanged += OnInventoryChanged;
        }

        public ClassMapper MapFurniture<T>(int id) where T : Furniture
        {
            FurnitureMapping.Add(id, typeof(T));
            return this;
        }

        private void ActivateMapping()
        {
            IsMappingActivated = true;
            Console.WriteLine("Activating mapping...");
            Traverser.Instance.TraverseLocations(l => Traverser.Instance.TraverseLocation(l, Wrap));
            Traverser.Instance.TraverseInventory(Game1.player, Wrap);
            Console.WriteLine("Mapping is activated.");
        }

        private void DeactivateMapping()
        {
            IsMappingActivated = false;
            Console.WriteLine("Deactivating mapping...");
            Traverser.Instance.TraverseLocations(l => Traverser.Instance.TraverseLocation(l, Unwrap));
            Traverser.Instance.TraverseInventory(Game1.player, Unwrap);
            Console.WriteLine("Mapping is deactivated.");
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

        private Object Wrap(Object basicObject)
        {
            if (basicObject == null) return null;

            var customType = TryGetType(basicObject);
            if (customType == null || basicObject.GetType() == customType) return basicObject;

            var customObject = new Constructor<int, Object>(customType).Invoke(basicObject.ParentSheetIndex);
            Cloner.Instance.CopyData(basicObject, customObject);
            return customObject;
        }

        private Object Unwrap(Object customObject)
        {
            if (customObject == null) return null;

            var customType = TryGetType(customObject);
            if (customObject.GetType() != customType) return customObject;

            Object basicObject;
            switch (customObject)
            {
                case Furniture furniture:
                    basicObject = new Constructor<Furniture>().Invoke();
                    break;
                default:
                    basicObject = new Constructor<Object>().Invoke();
                    break;
            }

            Cloner.Instance.CopyData(customObject, basicObject);
            basicObject.heldObject = Unwrap(basicObject.heldObject);
            return basicObject;
        }

        private Type TryGetType(Object o)
        {
            switch (o)
            {
                case Furniture furniture:
                    return FurnitureMapping.TryGetValue(o.ParentSheetIndex, out Type type) ? type : null;
                default:
                    return null;
            }
        }
    }
}