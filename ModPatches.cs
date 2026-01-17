using System.Reflection.Emit;
using HarmonyLib;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Graphics;

internal static class ModPatches
{
    private static readonly Harmony harmony = new("WangLuhang.Colorful");

    public static void Apply()
    {
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.draw)),
            transpiler: new HarmonyMethod(typeof(ModPatches), nameof(DiscreteColorPickerDrawTranspiler))
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.receiveLeftClick)),
            transpiler: new HarmonyMethod(typeof(ModPatches), nameof(DiscreteColorPickerDrawTranspiler))
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
            transpiler: new HarmonyMethod(typeof(ModPatches), nameof(ChestDrawTranspiler))
        );

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool) }),
            transpiler: new HarmonyMethod(typeof(ModPatches), nameof(ChestDrawTranspiler))
        );
    }

    private static IEnumerable<CodeInstruction> ChestDrawTranspiler(
        IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(new CodeMatch(CodeInstruction.LoadField(typeof(Chest), nameof(Chest.playerChoiceColor))))
            .Repeat(
                static matcher =>
                {
                    matcher.Advance(2).InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        CodeInstruction.Call(typeof(ModPatches), nameof(GetNewColor)));
                })
            .InstructionEnumeration();

    private static IEnumerable<CodeInstruction> DiscreteColorPickerDrawTranspiler(
        IEnumerable<CodeInstruction> instructions) =>
        new CodeMatcher(instructions)
            .MatchEndForward(
                new CodeMatch(
                    static instruction => instruction.Calls(AccessTools.DeclaredMethod(typeof(DiscreteColorPicker),
                        nameof(DiscreteColorPicker.getColorFromSelection)))))
            .SetInstruction(
                CodeInstruction.Call(typeof(ModPatches), nameof(GetColorFromSelection)))
            .InstructionEnumeration();

    private static Color GetColorFromSelection(int selection)
    {
        return selection switch
        {
            1 => new Color(255, 0, 0),
            2 => new Color(0, 255, 0),
            3 => new Color(0, 0, 255),
            4 => new Color(255, 255, 0),
            5 => new Color(255, 0, 255),
            6 => new Color(0, 255, 255),
            7 => new Color(255, 128, 0),
            8 => new Color(128, 255, 0),
            9 => new Color(0, 128, 255),
            10 => new Color(128, 0, 255),
            11 => new Color(255, 0, 128),
            12 => new Color(0, 255, 128),
            13 => new Color(200, 50, 50),
            14 => new Color(50, 200, 50),
            15 => new Color(50, 50, 200),
            16 => new Color(200, 200, 50),
            17 => new Color(200, 50, 200),
            18 => new Color(50, 200, 200),
            19 => new Color(200, 200, 200),
            20 => new Color(254, 254, 254),
            _ => Color.Black,
        };
    }

    private static Color GetNewColor(Color oldColor, Chest chest)
    {
        var selection = DiscreteColorPicker.getSelectionFromColor(oldColor);
        return GetColorFromSelection(selection);
    }
}