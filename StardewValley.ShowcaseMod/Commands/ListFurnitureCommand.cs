using System.Linq;
using Igorious.StardewValley.DynamicApi2;
using Igorious.StardewValley.DynamicApi2.Services;
using StardewModdingAPI;

namespace Igorious.StardewValley.ShowcaseMod.Commands
{
    public sealed class ListFurnitureCommand : ConsoleCommand
    {
        public ListFurnitureCommand(IMonitor monitor) : base(monitor, "list_furniture", "List furniture items from the game data.") { }

        public void Execute() => Execute("id");

        public void Execute(string sortingKey)
        {
            var furnitureData = DataService.Instance.GetFurniture().Select(kv => (ID: kv.Key, Name: kv.Value.Split('/').First()));
            var orderedFurniture = (sortingKey == "a")? furnitureData.OrderBy(f => f.Name) : furnitureData.OrderBy(f => f.ID);
            foreach (var furnitureInfo in orderedFurniture)
            {
                Info($"{furnitureInfo.ID,4}: {furnitureInfo.Name}");
            }
        }
    }
}