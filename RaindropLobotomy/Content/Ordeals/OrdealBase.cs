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

namespace RaindropLobotomy.Ordeals
{
    public abstract class OrdealBase<T> : OrdealBase where T : OrdealBase<T>
    {
        public static T Instance { get; private set; }

        public OrdealBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class OrdealBase
    {
        public abstract OrdealLevel OrdealLevel { get; }
        public abstract string Name { get; }
        public abstract string Subtitle { get; }
        public abstract string RiskTitle { get; }
        public abstract Color32 Color { get; }

        public abstract void OnSpawnOrdeal(RoR2.Stage stage);

        public void Create() {
            OrdealManager.ordeals[OrdealLevel].Add(this);
        }
    }

    public enum OrdealLevel {
        DAWN,
        NOON,
        DUSK,
        MIDNIGHT
    }
}