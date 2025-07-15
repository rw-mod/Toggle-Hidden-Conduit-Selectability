using HarmonyLib;
using RimWorld;
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

    [HarmonyPatch(typeof(PlaySettings), "DoMapControls")]
    static class DoMapControlsPatch
    {
        static void Postfix(WidgetRow row)
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