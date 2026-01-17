using StardewModdingAPI;

namespace Colorful;

public class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        ModPatches.Apply();
    }
}
