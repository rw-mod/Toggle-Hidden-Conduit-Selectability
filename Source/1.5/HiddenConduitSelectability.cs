using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace THCS;

[StaticConstructorOnStartup]
public static class HiddenConduitSelectability
{
    private static readonly Texture2D HiddenConduitSelected = ContentFinder<Texture2D>.Get("UI/Buttons/THCS_toggleBtn");

    private static readonly ThingDef HiddenConduit = ThingDefOf.HiddenConduit;
    private static readonly bool DefaultValue = HiddenConduit.selectable;

    static HiddenConduitSelectability()
    {
        Harmony harmony = new Harmony("rw.mod.hiddenconduitselectability");
        harmony.PatchAll();
    }

    [HarmonyPatch(typeof(PlaySettings), MethodType.Constructor)]
    static class ConstructorPatch
    {
        public static void Postfix()
        {
            HiddenConduit.selectable = DefaultValue;
        }
    }

    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    static class DoPlaySettingsPatch
    {
        private static readonly MethodInfo ResourceRocGetter = typeof(Prefs).GetProperty(nameof(Prefs.ResourceReadoutCategorized)).GetGetMethod();
        private static readonly MethodInfo ToggleableIconMethod = typeof(WidgetRow).GetMethod(nameof(WidgetRow.ToggleableIcon));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions);

            codeMatcher.MatchStartForward(CodeMatch.Calls(ResourceRocGetter));
            codeMatcher.ThrowIfInvalid($"Could not find getter call to for {nameof(Prefs.ResourceReadoutCategorized)}");

            codeMatcher.MatchStartBackwards(CodeMatch.WithOpcodes(new HashSet<OpCode>() { OpCodes.Call, OpCodes.Callvirt, OpCodes.Calli }));
            codeMatcher.ThrowIfInvalid($"Could not find a call before {nameof(Prefs.ResourceReadoutCategorized)}");
            codeMatcher.Advance(1);

            List<CodeInstruction> newInstructions = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, ((Action<WidgetRow>)AddHiddenConduitSelectableButton).Method),
            };
            codeMatcher.Insert(newInstructions);

            return codeMatcher.InstructionEnumeration();
        }

        static void AddHiddenConduitSelectableButton(WidgetRow row)
        {
            row.ToggleableIcon(ref HiddenConduit.selectable, HiddenConduitSelected, "THCS_HiddenConduitSelectabilityToggleButton".Translate(), SoundDefOf.Mouseover_ButtonToggle);
        }
    }

    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.ExposeData))]
    static class ExposeDataPatch
    {
        public static void Postfix()
        {
            Scribe_Values.Look(ref HiddenConduit.selectable, "hiddenConduitSelectable", DefaultValue);
        }
    }
}