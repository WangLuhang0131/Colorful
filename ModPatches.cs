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
            1 => new Color(254, 255, 255), // A01
            2 => new Color(72, 70, 78),    // B06
            3 => new Color(47, 43, 47),    // B07
            4 => new Color(0, 0, 0),       // B09
            5 => new Color(205, 232, 255), // H01
            6 => new Color(160, 226, 251), // H04
            7 => new Color(65, 204, 255),  // H05
            8 => new Color(62, 188, 226),  // H09
            9 => new Color(54, 119, 210),  // H13
            10 => new Color(24, 42, 132),  // H20
            11 => new Color(51, 58, 149),  // H21
            12 => new Color(52, 72, 142),  // H32
            13 => new Color(133, 142, 221),// J08
            14 => new Color(119, 134, 229),// J20
            15 => new Color(73, 79, 199),  // J21
            16 => new Color(255, 243, 235),// K01
            17 => new Color(252, 221, 210),// K02
            18 => new Color(255, 226, 206),// Z02
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