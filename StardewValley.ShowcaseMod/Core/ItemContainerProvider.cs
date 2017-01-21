using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace Igorious.StardewValley.ShowcaseMod.Core
{
    internal class ItemContainerProvider
    {
        private IList<Item> Items { get; }

        public ItemContainerProvider(IList<Item> items, int rows, int columns)
        {
            Items = items;
            Rows = rows;
            Columns = columns;

            TopRow = GetTopRow();
            BottomRow = GetBottomRow();
            LeftColumn = GetLeftColumn();
            RightColumn = GetRightColumn();
        }

        public int Rows { get; }
        public int Columns { get; }
        public int TopRow { get; }
        public int BottomRow { get; }
        public int LeftColumn { get; }
        public int RightColumn { get; }

        private int GetTopRow()
        {
            return Enumerable.Range(0, Rows).FirstOrDefault(i => Enumerable.Range(0, Columns).Any(j => this[i, j] != null));
        }

        private int GetBottomRow()
        {
            return Enumerable.Range(0, Rows).Reverse().FirstOrDefault(i => Enumerable.Range(0, Columns).Any(j => this[i, j] != null));
        }

        private int GetLeftColumn()
        {
            return Enumerable.Range(0, Columns).FirstOrDefault(j => Enumerable.Range(0, Rows).Any(i => this[i, j] != null));
        }

        private int GetRightColumn()
        {
            return Enumerable.Range(0, Columns).Reverse().FirstOrDefault(j => Enumerable.Range(0, Rows).Any(i => this[i, j] != null));
        }

        public Item this[int i, int j]
        {
            get
            {
                var itemIndex = i * Columns + j;
                return (itemIndex < Items.Count)? Items[itemIndex] : null;
            }
            set
            {
                var itemIndex = i * Columns + j;
                if (itemIndex >= Items.Count) return;
                Items[itemIndex] = value;
            }
        }
    }
}