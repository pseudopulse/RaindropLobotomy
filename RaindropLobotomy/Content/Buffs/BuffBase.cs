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
using System.Linq;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace RaindropLobotomy.Buffs
{
    public abstract class BuffBase<T> : BuffBase where T : BuffBase<T>
    {
        public static T Instance { get; private set; }

        public BuffBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class BuffBase
    {
        public abstract BuffDef Buff { get; }

        public abstract void PostCreation();
        
        public void Create() {
            ContentAddition.AddBuffDef(Buff);
            PostCreation();
        }
    }
}