using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace Igorious.StardewValley.ShowcaseMod.Core
{
    internal class ItemContainerProvider
    {
        private IReadOnlyList<Item> Items { get; }
        private int Rows { get; }
        private int Columns { get; }

        public ItemContainerProvider(IReadOnlyList<Item> items, int rows, int columns)
        {
            Items = items;
            Rows = rows;
            Columns = columns;

            TopRow = GetTopRow();
            BottomRow = GetBottomRow();
            LeftColumn = GetLeftColumn();
            RightColumn = GetRightColumn();
        }

        public int TopRow { get; }
        public int BottomRow { get; }
        public int LeftColumn { get; }
        public int RightColumn { get; }

        private int GetTopRow()
        {
            return Enumerable.Range(0, Rows).First(i => Enumerable.Range(0, Columns).Any(j => this[i, j] != null));
        }

        private int GetBottomRow()
        {
            return Enumerable.Range(0, Rows).Reverse().First(i => Enumerable.Range(0, Columns).Any(j => this[i, j] != null));
        }

        private int GetLeftColumn()
        {
            return Enumerable.Range(0, Columns).First(j => Enumerable.Range(0, Rows).Any(i => this[i, j] != null));
        }

        private int GetRightColumn()
        {
            return Enumerable.Range(0, Columns).Reverse().First(j => Enumerable.Range(0, Rows).Any(i => this[i, j] != null));
        }

        public Item this[int i, int j]
        {
            get
            {
                var itemIndex = i * Columns + j;
                return (itemIndex < Items.Count)? Items[itemIndex] : null;
            }
        }
    }
}