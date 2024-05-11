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

namespace RaindropLobotomy.Skills
{
    public abstract class SkillBase<T> : SkillBase where T : SkillBase<T>
    {
        public static T Instance { get; private set; }

        public SkillBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class SkillBase
    {
        public abstract SkillDef SkillDef { get; }
        public abstract SurvivorDef Survivor { get; }
        public abstract SkillSlot Slot { get; }
        public abstract UnlockableDef Unlock { get; }

        public virtual void CreateLanguage() {

        }

        public void Create() {
            CreateLanguage();
            ContentAddition.AddSkillDef(SkillDef);

            GameObject survivor = Survivor.bodyPrefab;
            SkillLocator skillLocator = survivor.GetComponent<SkillLocator>();

            SkillFamily family = null;

            switch (Slot) {
                case SkillSlot.Primary:
                    family = skillLocator.primary.skillFamily;
                    break;
                case SkillSlot.Secondary:
                    family = skillLocator.secondary.skillFamily;
                    break;
                case SkillSlot.Utility:
                    family = skillLocator.utility.skillFamily;
                    break;
                case SkillSlot.Special:
                    family = skillLocator.special.skillFamily;
                    break;
                default:
                    break;
            }

            if (family != null) {
                Array.Resize(ref family.variants, family.variants.Length + 1);
                    
                family.variants[family.variants.Length - 1] = new SkillFamily.Variant {
                    skillDef = SkillDef,
                    unlockableDef = Unlock,
                    viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
                };
            }
        }
    }
}