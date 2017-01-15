using System;
using System.Collections.Generic;
using System.Linq;
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

        private List<Item> Items => GetChest().items;

        private ShowcaseConfig Config => ShowcaseMod.Config.Showcase;

        public Showcase(int id) : base(id, Vector2.Zero) { }

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
            Game1.activeClickableMenu = new StorageContainer(Items, Math.Max(Items.Count(i => i != null), Config.Rows * Config.Columns), Config.Rows, OnContainterChanging, Utility.highlightShippableObjects);
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

        private bool OnContainterChanging(Item newItem, int position, Item oldItem, StorageContainer container, bool isRemoving)
        {
            if (isRemoving)
            {
                if (oldItem?.Stack > 1 && !oldItem.Equals(newItem))
                {
                    return false;
                }
            }
            else
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
            }

            if (position < Items.Count)
            {
                var newCellItem = !isRemoving || oldItem != null && !oldItem.Equals(newItem) ? newItem : null;
                Items[position] = newCellItem;
            }
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
            var layerDepth = (boundingBox.Bottom - 8) / 10000f;
            var viewPosition = x == -1
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

            var items = Items.Where(i => i != null).ToList();
            if (items.Count == 0) return;

            var deadLeft = Config.Bounds.Left;
            var deadRigth = Config.Bounds.Right;
            var deadTop = Config.Bounds.Top;
            var deadBottom = Config.Bounds.Bottom;

            float workWidth = sourceRect.Width - deadLeft - deadRigth;
            float workHeigth = sourceRect.Height - deadTop - deadBottom;

            (var n, var m) = CalculateRowsAndColumnsCount(items.Count);

            var leftEmpty = Math.Max(0, workWidth - spriteSheetTileSize * m) / 2;
            var topEmpty = Math.Max(0, workHeigth - spriteSheetTileSize * n) / 2;
            var dx = m > 1? (workWidth - leftEmpty * 2 - spriteSheetTileSize) / (m - 1) : 0;
            var dy = n > 1? (workHeigth - topEmpty * 2 - spriteSheetTileSize) / (n - 1) : 0;

            for (var i = 0; i < n; ++i)
            {
                for (var j = 0; j < m; ++j)
                {
                    var index = i * m + j;
                    if (index == items.Count) break;
                    var itemX = viewPosition.X + (deadLeft + leftEmpty + j * dx) * ((float)Game1.tileSize / spriteSheetTileSize);
                    var itemY = viewPosition.Y + (deadTop + topEmpty + i * dy) * ((float)Game1.tileSize / spriteSheetTileSize);
                    DrawItem((Object)items[index], spriteBatch, itemX, itemY, alpha, layerDepth + 0.00002f * (index + 1));
                }
            }
        }

        private static (int n, int m) CalculateRowsAndColumnsCount(int itemsCount)
        {
            var k = (int)Math.Ceiling(Math.Sqrt(itemsCount));
            return (k - 1) * k >= itemsCount? (k - 1, k) : (k, k);
        }

        private static void DrawItem(Object item, SpriteBatch spriteBatch, float viewX, float viewY, float alpha, float layerDepth)
        {
            spriteBatch.Draw(
                Game1.objectSpriteSheet,
                new Rectangle(
                    (int)viewX,
                    (int)viewY,
                    Game1.tileSize,
                    Game1.tileSize),
                Game1.currentLocation.getSourceRectForObject(item.ParentSheetIndex),
                Color.White * alpha,
                0,
                Vector2.Zero,
                SpriteEffects.None,
                layerDepth);
        }
    }
}