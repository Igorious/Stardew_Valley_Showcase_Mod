using System;
using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.DynamicApi2.Services
{
    public sealed class Traverser
    {
        public static Traverser Instance { get; } = new Traverser();

        public void TraverseLocations(Action<GameLocation> processLocation)
        {
            foreach (var location in Game1.locations)
            {
                processLocation(location);

                var buildableLocation = location as BuildableGameLocation;
                if (buildableLocation == null) continue;

                foreach (var building in buildableLocation.buildings)
                {
                    var indoorLocation = building.indoors;
                    if (indoorLocation == null) continue;
                    processLocation(indoorLocation);
                }
            }
        }

        public void TraverseLocation(GameLocation location, Func<Object, Object> processObject)
        {
            if (location == null) return;
            var objectInfos = location.Objects.ToList();
            
            foreach (var objectInfo in objectInfos)
            {
                var oldObject = objectInfo.Value;
                var newObject = processObject(oldObject);
                if (newObject.heldObject != null)
                {
                    newObject.heldObject = processObject(newObject.heldObject);
                }
                TraverseChest(newObject as Chest, processObject);
                if (oldObject != newObject)
                {
                    location.Objects[objectInfo.Key] = newObject;
                }
            }

            var decoratableLocation = location as DecoratableLocation;
            if (decoratableLocation == null) return;
            for (var i = 0; i < decoratableLocation.furniture.Count; ++i)
            {
                decoratableLocation.furniture[i] = (Furniture)processObject(decoratableLocation.furniture[i]);
            }

            var farmHouse = decoratableLocation as FarmHouse;
            if (farmHouse == null) return;
            TraverseChest(farmHouse.fridge, processObject);           
        }

        public void TraverseInventory(Farmer farmer, Func<Object, Object> processObject)
        {
            if (farmer == null) return;
            for (var i = 0; i < farmer.Items.Count; ++i)
            {
                var inventoryObject = farmer.Items[i] as Object;
                if (inventoryObject == null) continue;
                farmer.Items[i] = processObject(inventoryObject);
            }
            if (farmer.ActiveObject != null)
            {
                farmer.ActiveObject = processObject(farmer.ActiveObject);
            }
        }

        private void TraverseChest(Chest chest, Func<Object, Object> processObject)
        {
            if (chest == null) return;
            for (var i = 0; i < chest.items.Count; ++i)
            {
                var chestObject = chest.items[i] as Object;
                if (chestObject == null) continue;
                chest.items[i] = processObject(chestObject);
            }
        }
    }
}