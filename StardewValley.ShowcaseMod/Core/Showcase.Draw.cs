using System;
using System.Linq;
using Igorious.StardewValley.DynamicApi2.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace Igorious.StardewValley.ShowcaseMod.Core
{
    public partial class Showcase
    {
        private static float TileScale => (float)Game1.tileSize / spriteSheetTileSize;
        private static Vector2 TileSize => new Vector2(Game1.tileSize, Game1.tileSize);

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            var layerDepth = (boundingBox.Bottom - 8) / 10000f;
            DrawFurniture(spriteBatch, x, y, alpha, HasOverlay(), out var viewPosition, ref layerDepth);
            DrawItems(spriteBatch, alpha * Config.Alpha, viewPosition, ref layerDepth);
            if (HasOverlay()) DrawFurniture(spriteBatch, x, y, alpha, false, out viewPosition, ref layerDepth);
        }

        private void DrawFurniture(SpriteBatch spriteBatch, int x, int y, float alpha, bool useOverlay, out Vector2 viewPosition, ref float layerDepth)
        {
            viewPosition = x == -1
                ? Game1.GlobalToLocal(Game1.viewport, drawPosition)
                : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * Game1.tileSize, y * Game1.tileSize - (sourceRect.Height * Game1.pixelZoom - boundingBox.Height)));

            var baseSourceRect = useOverlay
                ? new Rectangle(sourceRect.X + sourceRect.Width, sourceRect.Y, sourceRect.Width, sourceRect.Height)
                : sourceRect;

            spriteBatch.Draw(
                furnitureTexture,
                viewPosition,
                baseSourceRect,
                Color.White * alpha,
                0,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                layerDepth);
            ChangeDepth(ref layerDepth);
        }

        private void ChangeDepth(ref float layerDepth)
        {
            layerDepth += (Config.InverseLayouts ? -1 : +1) * 0.0000001f;
        }

        private bool HasOverlay() => (Config.SpritesCount > 1);

        private void DrawItems(SpriteBatch spriteBatch, float alpha, Vector2 viewPosition, ref float layerDepth)
        {
            if (Items.All(i => i == null)) return;

            var itemProvider = new ItemContainerProvider(Items, Config.Rows, Config.Columns);

            float workWidth = sourceRect.Width - Config.Bounds.Left - Config.Bounds.Right;
            float workHeigth = sourceRect.Height - Config.Bounds.Top - Config.Bounds.Bottom;

            var rowsCount = itemProvider.BottomRow - itemProvider.TopRow + 1;
            var columnsCount = itemProvider.RightColumn - itemProvider.LeftColumn + 1;

            var leftEmptyOffset = Math.Max(0, workWidth - spriteSheetTileSize * columnsCount) / 2;
            var topEmptyOffset = Math.Max(0, workHeigth - spriteSheetTileSize * rowsCount) / 2;

            var horizontalItemOffset = columnsCount > 1 ? (workWidth - leftEmptyOffset * 2 - spriteSheetTileSize) / (columnsCount - 1) : 0;
            var verticalItemOffset = rowsCount > 1 ? (workHeigth - topEmptyOffset * 2 - spriteSheetTileSize) / (rowsCount - 1) : 0;

            var offset = new Vector2(Config.Bounds.Left + leftEmptyOffset, Config.Bounds.Top + topEmptyOffset) * TileScale;

            for (var i = itemProvider.TopRow; i <= itemProvider.BottomRow; ++i)
            {
                for (var j = itemProvider.LeftColumn; j <= itemProvider.RightColumn; ++j)
                {
                    var item = itemProvider[i, j];
                    if (item == null) continue;
                    var tileDelta = new Vector2((j - itemProvider.LeftColumn) * horizontalItemOffset, (i - itemProvider.TopRow) * verticalItemOffset);
                    DrawItem((dynamic)item, spriteBatch, viewPosition + offset + tileDelta * TileScale, alpha, layerDepth);
                    ChangeDepth(ref layerDepth);
                }
            }
        }

        private void DrawItem(Object item, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float layerDepth)
        {
            void DrawObjectSprite(int spriteIndex, Color color, float depth) => spriteBatch.Draw(
                Game1.objectSpriteSheet,
                viewPosition + TileSize / 2,
                TextureInfo.Objects.GetSourceRect(spriteIndex),
                color * alpha,
                0,
                new Vector2(spriteSheetTileSize, spriteSheetTileSize) / 2f,
                TileScale * Config.Scale,
                SpriteEffects.None,
                depth);

            DrawObjectSprite(item.ParentSheetIndex, Color.White, layerDepth);
            if (!(item is ColoredObject coloredObject)) return;
            DrawObjectSprite(item.ParentSheetIndex + 1, coloredObject.color, layerDepth + 0.0000001f);
        }

        private void DrawItem(Tool tool, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float layerDepth)
        {
            bool IsWeapon() => tool is MeleeWeapon || tool is Slingshot;

            float GetSpriteRotation() => (tool is Slingshot)
                ? 0
                : IsSwordLike() ? -5 * MathHelper.PiOver4 : -MathHelper.PiOver4;

            bool IsSwordLike()
            {
                var weaponType = (tool as MeleeWeapon)?.type;
                return (tool is Sword) || (weaponType != null && weaponType != MeleeWeapon.club);
            }

            if (!IsWeapon()) return;

            spriteBatch.Draw(
               Tool.weaponsTexture,
               viewPosition + new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f / 1.414f),
               TextureInfo.Weapons.GetSourceRect(tool.CurrentParentTileIndex),
               Color.White * alpha,
               GetSpriteRotation(),
               new Vector2(spriteSheetTileSize, spriteSheetTileSize) / 2f,
               TileScale * Config.Scale,
               SpriteEffects.None,
               layerDepth);
        }
    }
}