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
            this.SetField<behaviorOnItemChange>("itemChangeBehavior", ProcessItemChanged);
            ItemsToGrabMenu.movePosition(0, (3 - rows) * Game1.tileSize);
        }

        private bool ProcessItemChanged(Item newItem, int position, Item oldItem, StorageContainer container, bool isRemoving)
        {
            return isRemoving ? OnItemRemoved(newItem, position, oldItem) : OnItemAdded(newItem, position, oldItem, container);
        }

        private bool OnItemRemoved(Item containerItem, int position, Item handItem)
        {
            if (handItem?.Stack > 1 && !handItem.Equals(containerItem))
            {
                return false;
            }

            var newCellItem = handItem != null && !handItem.Equals(containerItem) ? containerItem : null;
            _items[position] = newCellItem;
            return true;
        }

        private bool OnItemAdded(Item handItem, int position, Item containerItem, StorageContainer container)
        {
            if (handItem.Stack > 1 || handItem.Stack == 1 && containerItem?.Stack == 1 && handItem.canStackWith(containerItem))
            {
                if (containerItem != null)
                {
                    if (containerItem.canStackWith(handItem))
                    {
                        container.ItemsToGrabMenu.actualInventory[position].Stack = 1;
                        container.heldItem = containerItem;
                    }
                    else
                    {
                        Utility.addItemToInventory(containerItem, position, container.ItemsToGrabMenu.actualInventory);
                        container.heldItem = handItem;
                    }
                    return false;
                }

                var newStack = handItem.Stack - 1;
                var one = handItem.getOne();
                one.Stack = newStack;
                container.heldItem = one;
                handItem.Stack = 1;
            }

            if (position < _items.Count)
            {
                _items[position] = handItem;
            }
            return true;
        }
    }
}