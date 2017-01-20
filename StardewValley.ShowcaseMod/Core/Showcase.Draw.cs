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
            DrawFurniture(spriteBatch, x, y, alpha, Config.IsTwoLayer, out var viewPosition, ref layerDepth);
            DrawItems(spriteBatch, alpha * Config.Alpha, viewPosition, ref layerDepth);
            if (Config.IsTwoLayer) DrawFurniture(spriteBatch, x, y, alpha, false, out viewPosition, ref layerDepth);
        }

        private void DrawFurniture(SpriteBatch spriteBatch, int x, int y, float alpha, bool useOverlay, out Vector2 viewPosition, ref float layerDepth)
        {
            var globalPosition = x == -1
                ? drawPosition
                : new Vector2(x * Game1.tileSize, y * Game1.tileSize - (sourceRect.Height * Game1.pixelZoom - boundingBox.Height));

            viewPosition = Game1.GlobalToLocal(Game1.viewport, globalPosition);

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
                flipped? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                layerDepth);

            ChangeDepth(ref layerDepth);
        }

        private void ChangeDepth(ref float layerDepth)
        {
            layerDepth += (Config.InverseLayouts ? -1 : +1) * 0.0000001f;
        }

        private void DrawItems(SpriteBatch spriteBatch, float alpha, Vector2 viewPosition, ref float layerDepth)
        {
            if (Items.All(i => i == null)) return;

            var itemProvider = new ItemContainerProvider(Items, GetRowsCount(), GetColumnsCount());

            var bounds = IsHorizontal()? Config.AltSpriteBounds : Config.SpriteBounds;

            float workWidth = sourceRect.Width - bounds.Left - bounds.Right;
            float workHeigth = sourceRect.Height - bounds.Top - bounds.Bottom;

            var rowsCount = itemProvider.BottomRow - itemProvider.TopRow + 1;
            var columnsCount = itemProvider.RightColumn - itemProvider.LeftColumn + 1;

            var leftEmptyOffset = (workWidth - spriteSheetTileSize * columnsCount) / 2;
            if (workWidth >= spriteSheetTileSize && leftEmptyOffset < 0)
            {
                leftEmptyOffset = 0;
            }

            var topEmptyOffset = (workHeigth - spriteSheetTileSize * rowsCount) / 2;
            if (workHeigth >= spriteSheetTileSize && topEmptyOffset < 0)
            {
                topEmptyOffset = 0;
            }

            var horizontalItemOffset = columnsCount > 1 ? (workWidth - leftEmptyOffset * 2 - spriteSheetTileSize) / (columnsCount - 1) : 0;
            var verticalItemOffset = rowsCount > 1 ? (workHeigth - topEmptyOffset * 2 - spriteSheetTileSize) / (rowsCount - 1) : 0;

            var offset = new Vector2((flipped? bounds.Right : bounds.Left) + leftEmptyOffset, bounds.Top + topEmptyOffset) * TileScale;

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

        private static int ScytheID = 47;

        private void DrawItem(Tool tool, SpriteBatch spriteBatch, Vector2 viewPosition, float alpha, float layerDepth)
        {
            float GetSpriteRotation()
            {
                if (tool is Slingshot) return 0;
                if (IsSwordLike()) return -5 * MathHelper.PiOver4;
                if (IsWeapon() || tool is Pickaxe || tool is Axe || tool is FishingRod || tool is Hoe) return -MathHelper.PiOver4;
                return 0;
            }

            bool IsSwordLike()
            {
                var weaponType = (tool as MeleeWeapon)?.type;
                return (tool is Sword) || (weaponType != null && weaponType != MeleeWeapon.club && tool.indexOfMenuItemView != ScytheID);
            }

            bool IsWeapon() => (tool is MeleeWeapon) || (tool is Slingshot);


            var textureInfo = IsWeapon()? TextureInfo.Weapons : TextureInfo.Tools;
            var rotation = GetSpriteRotation();

            spriteBatch.Draw(
                textureInfo.Texture,
                viewPosition + new Vector2(Game1.tileSize / 2f, Game1.tileSize / 2f / ((rotation != 0)? 1.414f : 1)),
                textureInfo.GetSourceRect(tool.indexOfMenuItemView),
                Color.White * alpha,
                rotation,
                new Vector2(spriteSheetTileSize, spriteSheetTileSize) / 2f,
                Game1.pixelZoom * Config.Scale,
                SpriteEffects.None,
                layerDepth);
        }
    }
}