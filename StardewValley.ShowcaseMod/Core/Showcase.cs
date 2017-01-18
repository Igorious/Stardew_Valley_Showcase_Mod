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
            var cellsCount = (Items.Count + Config.Rows - 1) / Config.Rows * Config.Rows;
            Game1.activeClickableMenu = new ShowcaseContainer(Items, cellsCount, Config.Rows, Filter.IsPass);
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
            var prefferedCount = Config.Rows * Config.Columns;
            while (Items.Count > prefferedCount && Items.Remove(null)) { }
            while (Items.Count < prefferedCount)
            {
                Items.Add(null);
            }
        }

        private Chest GetChest()
        {
            if (heldObject == null)
            {
                var chest = new Chest(true);
                heldObject = chest;
                chest.items.AddRange(Enumerable.Repeat<Item>(null, Config.Rows * Config.Columns));
            }
            return (Chest)heldObject;
        }
    }
}