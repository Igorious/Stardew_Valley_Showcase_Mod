using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Igorious.StardewValley.DynamicApi2.Data
{
    public sealed class TextureInfo
    {
        public static TextureInfo Furniture { get; } = new TextureInfo(nameof(Furniture), 16, 16);
        public static TextureInfo Objects { get; } = new TextureInfo(nameof(Objects), 16, 16);

        public TextureInfo(string name, int spriteWidth, int spriteHeigth)
        {
            Name = name;
            SpriteWidth = spriteWidth;
            SpriteHeigth = spriteHeigth;
        }

        public string Name { get; }
        public int SpriteWidth { get; }
        public int SpriteHeigth { get; }

        public Rectangle GetSourceRect(Texture2D texture, int index, int spriteTileWidth = 1, int spriteTileHeight = 1)
        {
            var rowLength = texture.Width / SpriteWidth;
            var x = index % rowLength * SpriteWidth;
            var spritePixelWidth = SpriteWidth * spriteTileWidth;
            if (x + spritePixelWidth > texture.Width)
            {
                spriteTileHeight += (x + spritePixelWidth) / texture.Width;
                x = 0;
                spritePixelWidth = texture.Width;
            }
            return new Rectangle(x, index / rowLength * SpriteHeigth, spritePixelWidth, SpriteHeigth * spriteTileHeight);
        }
    }
}