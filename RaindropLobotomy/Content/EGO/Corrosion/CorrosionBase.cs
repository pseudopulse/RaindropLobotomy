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
    }
}