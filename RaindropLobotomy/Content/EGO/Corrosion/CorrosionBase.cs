using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using System;
using System.Collections.Generic;
using EntityStates;
using RoR2.ExpansionManagement;
using Unity;
using HarmonyLib;
using RoR2.CharacterAI;
using RaindropLobotomy.Survivors;
using Survariants;

namespace RaindropLobotomy.EGO
{
    public abstract class CorrosionBase<T> : CorrosionBase where T : CorrosionBase<T>
    {
        public static T Instance { get; private set; }

        public CorrosionBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class CorrosionBase : SurvivorBase
    {
        public abstract string EGODisplayName { get; }
        public abstract string Description { get; }
        public abstract SurvivorDef TargetSurvivorDef { get; }
        public abstract UnlockableDef RequiredUnlock { get; }
        public abstract Color Color { get; }
        public SurvivorVariantDef EGO;
        public virtual string RequiresAbsentMod { get; } = null;
        public virtual string RequiresMod { get; } = null;
        public bool AreWeAllowedToLoad() {
            if (RequiresAbsentMod != null && MainConfig.SuppressDuplicateVariants) {
                Debug.Log(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RequiresAbsentMod));
                return !BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RequiresAbsentMod);
            }

            if (RequiresMod != null) {
                Debug.Log(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RequiresMod));
                return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RequiresMod);
            }

            return true;
        }
        public override void Create()
        {
            base.Create();

            EGO = ScriptableObject.CreateInstance<SurvivorVariantDef>();
            (EGO as ScriptableObject).name = EGODisplayName;
            EGO.DisplayName = EGODisplayName;
            EGO.VariantSurvivor = Survivor;
            EGO.TargetSurvivor = TargetSurvivorDef;
            EGO.RequiredUnlock = RequiredUnlock;
            EGO.Color = Color;
            EGO.Description = Description;

            Survivor.hidden = true;

            SurvivorVariantCatalog.AddSurvivorVariant(EGO);
        }

        public SurvivorDef CreateSurvivorDef(GameObject body, GameObject display, string name, string desc, string outroLoss = "", string outroWin = "") {
            SurvivorDef def = ScriptableObject.CreateInstance<SurvivorDef>();
            def.bodyPrefab = body;
            def.displayPrefab = display;
            def.displayNameToken = name;
            def.descriptionToken = desc;
            def.outroFlavorToken = outroWin;
            def.mainEndingEscapeFailureFlavorToken = outroLoss;
            def.hidden = true;

            return def;
        }

        public void ReplaceSkill(GenericSkill slot, SkillFamily family) {
            slot._skillFamily = family;
        }
    }
}