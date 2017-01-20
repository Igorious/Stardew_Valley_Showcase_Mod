using System.Collections.Generic;
using System.Linq;
using Igorious.StardewValley.ShowcaseMod.ModConfig;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.ShowcaseMod.Core
{
    public partial class Showcase : Furniture
    {
        private ShowcaseConfig Config { get; }
        private ItemFilter Filter { get; }
        private List<Item> Items => GetChest().items;
        private int LastRotation { get; set; } = -1;

        public Showcase(int id) : base(id, Vector2.Zero)
        {
            Config = ShowcaseMod.GetShowcaseConfig(ParentSheetIndex);
            Filter = new ItemFilter(Config.Filter);
        }

        public override bool clicked(Farmer who)
        {
            var chest = heldObject;
            heldObject = null;
            var result = base.clicked(who);
            heldObject = chest;
            return result;
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity) return true;
            RecalculateItems();
            var cellsCount = (Items.Count + GetRowsCount() - 1) / GetRowsCount() * GetRowsCount();
            Game1.activeClickableMenu = new ShowcaseContainer(Items, cellsCount, GetRowsCount(), Filter.IsPass);
            return true;
        }

        public override bool performObjectDropInAction(Object dropIn, bool probe, Farmer who)
        {
            if (!Utility.highlightShippableObjects(dropIn)) return false;
            RecalculateItems();
            var emptyCellIndex = Items.IndexOf(null);
            if (emptyCellIndex == -1) return false;
            if (probe) return true;
            Game1.playSound("woodyStep");
            who?.reduceActiveItemByOne();
            Items[emptyCellIndex] = dropIn.getOne();
            return true;
        }

        private void RecalculateItems()
        {
            var prefferedCount = GetRowsCount() * GetColumnsCount();
            while (Items.Count > prefferedCount && Items.Remove(null)) { }
            while (Items.Count < prefferedCount)
            {
                Items.Add(null);
            }
        }

        private Chest GetChest()
        {
            Chest chest;
            if (heldObject == null)
            {
                heldObject = chest = new Chest(true);
                chest.items.AddRange(Enumerable.Repeat<Item>(null, GetRowsCount() * GetColumnsCount()));
                LastRotation = currentRotation;
            }
            else
            {
                chest = (Chest)heldObject;
                UpdateRotation(chest);
            }
            return chest;
        }

        private bool IsHorizontal(int? rotation = null) => (rotation ?? currentRotation) % 2 == 1;

        private int GetRowsCount(int? rotation = null) => IsHorizontal(rotation ?? currentRotation)? Config.Columns : Config.Rows;

        private int GetColumnsCount(int? rotation = null) => IsHorizontal(rotation ?? currentRotation)? Config.Rows : Config.Columns;

        private void UpdateRotation(Chest chest)
        {
            if (LastRotation == currentRotation) return;

            var oldContainer = new ItemContainerProvider(chest.items.ToList(), GetRowsCount(LastRotation), GetColumnsCount(LastRotation));
            var newContainer = new ItemContainerProvider(chest.items, GetRowsCount(), GetColumnsCount());

            for (var i = 0; i < newContainer.Rows; ++i)
            {
                for (var j = 0; j < newContainer.Columns; ++j)
                {
                    newContainer[i, j] = oldContainer[j, oldContainer.Columns - 1 - i];
                }
            }

            LastRotation = currentRotation;
        }
    }
}