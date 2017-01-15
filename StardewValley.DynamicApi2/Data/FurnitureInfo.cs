namespace Igorious.StardewValley.DynamicApi2.Data
{
    public class FurnitureInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Size Size { get; set; }
        public Size BoundingBox { get; set; }
        public int Rotations { get; set; }
        public int Price { get; set; }

        public override string ToString()
        {
            return $"{Name}/{Type}/{Size}/{BoundingBox}/{Rotations}/{Price}";
        }
    }
}