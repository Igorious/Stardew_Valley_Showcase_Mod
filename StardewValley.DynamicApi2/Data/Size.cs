namespace Igorious.StardewValley.DynamicApi2.Data
{
    public class Size
    {
        public static Size Default => new Size(-1, -1);

        public Size() { }
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public override string ToString()
        {
            return Width != -1 ? $"{Width} {Height}" : "-1";
        }
    }
}