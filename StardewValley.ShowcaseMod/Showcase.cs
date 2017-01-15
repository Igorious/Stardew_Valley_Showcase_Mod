using System;
using System.Collections.Generic;
using System.Linq;
using Igorious.StardewValley.DynamicApi2.Data;
using Igorious.StardewValley.ShowcaseMod.ModConfig;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.ShowcaseMod
{
    public sealed class Showcase : Furniture
    {
        private ShowcaseConfig Config { get; }
        private List<Item> Items => GetChest().items;

        public Showcase(int id) : base(id, Vector2.Zero)
        {
            Config = ShowcaseMod.GetShowcaseConfig(ParentSheetIndex);
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
            var totalItemsCount = Math.Max(Items.Count(i => i != null), Config.Rows * Config.Columns);
            Game1.activeClickableMenu = new StorageContainer(Items, totalItemsCount, Config.Rows, OnContainterChanging, Utility.highlightShippableObjects);
            return true;
        }

        public override bool performObjectDropInAction(Object dropIn, bool probe, Farmer who)
        {
            if (!Utility.highlightShippableObjects(dropIn)) return false;
            var emptyCellIndex = Items.IndexOf(null);
            if (emptyCellIndex == -1) return false;
            if (probe) return true;
            Game1.playSound("woodyStep");
            who?.reduceActiveItemByOne();
            Items[emptyCellIndex] = dropIn.getOne();
            return true;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            DrawFurniture(spriteBatch, x, y, alpha, out var viewPosition, out var layerDepth);
            DrawItems(spriteBatch, alpha, viewPosition, layerDepth);
        }

        private void DrawFurniture(SpriteBatch spriteBatch, int x, int y, float alpha, out Vector2 viewPosition, out float layerDepth)
        {
            layerDepth = (boundingBox.Bottom - 8) / 10000f;
            viewPosition = x == -1
                ? Game1.GlobalToLocal(Game1.viewport, drawPosition)
                : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize, y * Game1.tileSize - (sourceRect.Height * Game1.pixelZoom - boundingBox.Height)));

            spriteBatch.Draw(
                furnitureTexture,
                viewPosition,
                sourceRect,
                Color.White * alpha,
                0,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                layerDepth);
        }

        private void DrawItems(SpriteBatch spriteBatch, float alpha, Vector2 viewPosition, float layerDepth)
        {
            if (Items.All(i => i == null)) return;

            float workWidth = sourceRect.Width - Config.Bounds.Left - Config.Bounds.Right;
            float workHeigth = sourceRect.Height - Config.Bounds.Top - Config.Bounds.Bottom;

            var topRow = GetTopRow();
            var bottomRow = GetBottomRow();
            var leftColumn = GetLeftColumn();
            var rightColumn = GetRightColumn();

            var rowsCount = bottomRow - topRow + 1;
            var columnsCount = rightColumn - leftColumn + 1;

            var leftOffset = Math.Max(0, workWidth - spriteSheetTileSize * columnsCount) / 2;
            var topOffset = Math.Max(0, workHeigth - spriteSheetTileSize * rowsCount) / 2;
            var itemSpaceX = columnsCount > 1? (workWidth - leftOffset * 2 - spriteSheetTileSize) / (columnsCount - 1) : 0;
            var itemSpaceY = rowsCount > 1? (workHeigth - topOffset * 2 - spriteSheetTileSize) / (rowsCount - 1) : 0;

            var viewScale = (float)Game1.tileSize / spriteSheetTileSize;
            for (var i = topRow; i <= bottomRow; ++i)
            {
                for (var j = leftColumn; j <= rightColumn; ++j)
                {
                    var itemIndex = GetItemIndex(i, j);
                    var item = Items[itemIndex] as Object;
                    if (item == null) continue;
                    var itemX = viewPosition.X + (Config.Bounds.Left + leftOffset + j * itemSpaceX) * viewScale;
                    var itemY = viewPosition.Y + (Config.Bounds.Top + topOffset + i * itemSpaceY) * viewScale;
                    DrawItem(item, spriteBatch, itemX, itemY, alpha, layerDepth + 0.00002f * (itemIndex + 1));
                }
            }
        }

        private int GetTopRow()
        {
            for (var i = 0; i < Config.Rows; ++i)
            {
                for (var j = 0; j < Config.Columns; ++j)
                {
                    if (GetItem(i, j) != null) return i;
                }
            }
            return 0;
        }

        private int GetBottomRow()
        {
            for (var i = Config.Rows - 1; i >= 0; --i)
            {
                for (var j = 0; j < Config.Columns; ++j)
                {
                    if (GetItem(i, j) != null) return i;
                }
            }
            return 0;
        }

        private int GetLeftColumn()
        {
            for (var j = 0; j < Config.Columns; ++j)
            {
                for (var i = 0; i < Config.Rows; ++i)
                {
                    if (GetItem(i, j) != null) return j;
                }
            }
            return 0;
        }

        private int GetRightColumn()
        {
            for (var j = Config.Columns - 1; j >= 0; --j)
            {
                for (var i = 0; i < Config.Rows; ++i)
                {
                    if (GetItem(i, j) != null) return j;
                }
            }
            return 0;
        }

        private int GetItemIndex(int i, int j) => i * Config.Columns + j;

        private Item GetItem(int i, int j) => Items[GetItemIndex(i, j)];

        private void RecalculateItems()
        {
            var prefferedCount = Config.Rows * Config.Columns;
            while (Items.Count > prefferedCount && Items.Remove(null)) { }
            while (Items.Count < prefferedCount)
            {
                Items.Add(null);
            }
        }

        private bool OnContainterChanging(Item newItem, int position, Item oldItem, StorageContainer container, bool isRemoving)
        {
            return isRemoving? OnItemRemoved(newItem, position, oldItem) : OnItemAdded(newItem, position, oldItem, container);
        }

        private bool OnItemRemoved(Item newItem, int position, Item oldItem)
        {
            if (oldItem?.Stack > 1 && !oldItem.Equals(newItem))
            {
                return false;
            }

            var newCellItem = oldItem != null && !oldItem.Equals(newItem) ? newItem : null;
            Items[position] = newCellItem;
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

            if (position < Items.Count)
            {
                Items[position] = newItem;
            }
            return true;
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

        private void DrawItem(Object item, SpriteBatch spriteBatch, float viewX, float viewY, float alpha, float layerDepth)
        {
            var itemSize = Game1.tileSize * Config.Scale;
            var delta = (Game1.tileSize - itemSize) / 2f;
            var destRect = new Rectangle((int)(viewX + delta), (int)(viewY + delta), (int)itemSize, (int)itemSize);
            spriteBatch.Draw(
                Game1.objectSpriteSheet,
                destRect,
                TextureInfo.Objects.GetSourceRect(Game1.objectSpriteSheet, item.ParentSheetIndex),
                Color.White * alpha,
                0,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);

            if (!(item is ColoredObject colored)) return;
            spriteBatch.Draw(
                Game1.objectSpriteSheet,
                destRect,
                TextureInfo.Objects.GetSourceRect(Game1.objectSpriteSheet, item.ParentSheetIndex + 1),
                colored.color * alpha,
                0,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth + 0.0000001f);
        }
    }
}