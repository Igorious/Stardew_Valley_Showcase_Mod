using System.Collections.Generic;
using Igorious.StardewValley.DynamicApi2.Extensions;
using StardewValley;
using StardewValley.Menus;

namespace Igorious.StardewValley.ShowcaseMod.Core
{
    internal sealed class ShowcaseContainer : StorageContainer
    {
        private readonly List<Item> _items;

        public ShowcaseContainer(
            List<Item> items,
            int capacity,
            int rows,
            InventoryMenu.highlightThisItem isItemEnabled)
            : base(items, capacity, rows, null, isItemEnabled)
        {
            _items = items;
            this.SetField("itemChangeBehavior", new behaviorOnItemChange(ProcessItemChanged));
            ItemsToGrabMenu.movePosition(0, (3 - rows) * Game1.tileSize);
        }

        private bool ProcessItemChanged(Item newItem, int position, Item oldItem, StorageContainer container, bool isRemoving)
        {
            return isRemoving ? OnItemRemoved(newItem, position, oldItem) : OnItemAdded(newItem, position, oldItem, container);
        }

        private bool OnItemRemoved(Item newItem, int position, Item oldItem)
        {
            if (oldItem?.Stack > 1 && !oldItem.Equals(newItem))
            {
                return false;
            }

            var newCellItem = oldItem != null && !oldItem.Equals(newItem) ? newItem : null;
            _items[position] = newCellItem;
            return true;
        }

        private bool OnItemAdded(Item newItem, int position, Item oldItem, StorageContainer container)
        {
            if (newItem.Stack > 1 || newItem.Stack == 1 && oldItem?.Stack == 1 && newItem.canStackWith(oldItem))
            {
                if (oldItem != null)
                {
                    if (oldItem.canStackWith(newItem))
                    {
                        container.ItemsToGrabMenu.actualInventory[position].Stack = 1;
                    }
                    else
                    {
                        Utility.addItemToInventory(oldItem, position, container.ItemsToGrabMenu.actualInventory);
                    }
                    container.heldItem = oldItem;
                    return false;
                }

                var num = newItem.Stack - 1;
                var one = newItem.getOne();
                one.Stack = num;
                container.heldItem = one;
                newItem.Stack = 1;
            }

            if (position < _items.Count)
            {
                _items[position] = newItem;
            }
            return true;
        }
    }
}