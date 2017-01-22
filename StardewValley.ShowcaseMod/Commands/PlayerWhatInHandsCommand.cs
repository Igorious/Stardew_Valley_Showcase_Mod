using Igorious.StardewValley.DynamicApi2;
using StardewModdingAPI;
using StardewValley;

namespace Igorious.StardewValley.ShowcaseMod.Commands
{
    public sealed class PlayerWhatInHandsCommand : ConsoleCommand
    {
        public PlayerWhatInHandsCommand(IMonitor monitor) : base(monitor, "player_whatinhands", "Get info about item in hands.") { }

        public void Execute()
        {
            var item = Game1.player.ActiveObject ?? (Item)Game1.player.CurrentTool;
            Info(item != null ? $"{item.Name} [{item.GetType().Name} ID={item.parentSheetIndex}]" : "Nothing");
        }
    }
}