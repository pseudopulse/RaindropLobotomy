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

namespace RaindropLobotomy.EGO
{
    public abstract class EGOSkillBase<T> : EGOSkillBase where T : EGOSkillBase<T>
    {
        public static T Instance { get; private set; }

        public EGOSkillBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class EGOSkillBase
    {
        public abstract SkillDef SkillDef { get; }
        public abstract SurvivorDef Survivor { get; }
        public abstract SkillSlot Slot { get; }
        public abstract UnlockableDef Unlock { get; }
        private static bool setHooks = false;

        public abstract void OnSkillChangeUpdate(CharacterModel model, bool equipped);

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

            CharacterModel body = Survivor.bodyPrefab.GetComponentInChildren<CharacterModel>();
            CharacterModel display = Survivor.displayPrefab.GetComponentInChildren<CharacterModel>();

            SetupRendererStore(GetRendererStore(body), body);
            SetupRendererStore(GetRendererStore(display), display);

            NetworkUser.onLoadoutChangedGlobal += OnLoadoutChanged;

            RendererStore.init += (s, e) => {
                OnLoadoutChanged(LocalUserManager.GetFirstLocalUser().currentNetworkUser);
            };

            if (!setHooks) {
                setHooks = true;
                IL.RoR2.SkinDef.RuntimeSkin.Apply += DontOverwrite;
            }
        }

        private void DontOverwrite(ILContext il)
        {
            ILCursor c = new(il);

            bool found = c.TryGotoNext(MoveType.Before, 
                x => x.MatchStfld(typeof(CharacterModel), nameof(CharacterModel.baseRendererInfos))
            );

            if (found) {
                c.Index -= 3;
                c.RemoveRange(4);
                c.Emit(OpCodes.Nop);
                c.Emit(OpCodes.Nop);
                c.Emit(OpCodes.Nop);
                c.Emit(OpCodes.Nop);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Action<SkinDef.RuntimeSkin, GameObject>>((skin, obj) => {
                    RendererStore store = obj.GetComponent<RendererStore>();
                    CharacterModel model = obj.GetComponent<CharacterModel>();
                    
                    for (int i = 0; i < SkinDef.RuntimeSkin.rendererInfoBuffer.Count; i++) {
                        bool disallow = false;

                        if (store != null) {
                            for (int j = 0; j < store.noUpdateIndices.Length; j++) {
                                int index = store.noUpdateIndices[j];
                                if (i == index) {
                                    disallow = true;
                                    break;
                                }
                            }
                        }

                        if (!disallow && i < model.baseRendererInfos.Length) {
                            model.baseRendererInfos[i] = SkinDef.RuntimeSkin.rendererInfoBuffer[i];
                        }
                    }
                });
            }
            else {
                // Debug.Log("Failed to match for ego IL hook.");
            }
        }

        public void OnLoadoutChanged(NetworkUser user) {
            Loadout loadout = Loadout.RequestInstance();
            user.networkLoadout.CopyLoadout(loadout);

            if (user.GetSurvivorPreference() == Survivor) {
                BodyIndex index = BodyCatalog.FindBodyIndex(Survivor.bodyPrefab);
                foreach (RendererStore store in GameObject.FindObjectsOfType<RendererStore>()) {
                    if (store.targetSkill == SkillDef) {
                        OnSkillChangeUpdate(store.GetComponent<CharacterModel>(), HasSkillVariantEnabled(loadout, index, Slot, SkillDef));
                    }
                }
            }
        }

        private static bool HasSkillVariantEnabled(Loadout loadout, BodyIndex bodyIndex, SkillSlot slot, SkillDef skillDef)
        {
            GameObject survivor = BodyCatalog.GetBodyPrefab(bodyIndex);
            SkillLocator skillLocator = survivor.GetComponent<SkillLocator>();

            SkillFamily family = null;

            switch (slot) {
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

            int num = FindSlotIndex(bodyIndex, family);
            int num2 = FindVariantIndex(family, skillDef);
            if (num == -1 || num2 == -1)
            {
                return false;
            }
            return loadout.bodyLoadoutManager.GetSkillVariant(bodyIndex, num) == num2;
        }

        private static int FindSlotIndex(BodyIndex bodyIndex, SkillFamily skillFamily) {
            GenericSkill[] bodyPrefabSkillSlots = BodyCatalog.GetBodyPrefabSkillSlots(bodyIndex);
            for (int i = 0; i < bodyPrefabSkillSlots.Length; i++)
            {
                if (bodyPrefabSkillSlots[i].skillFamily == skillFamily)
                {
                    return i;
                }
            }
            return -1;
        }

        private static int FindVariantIndex(SkillFamily skillFamily, SkillDef skillDef)
        {
            SkillFamily.Variant[] variants = skillFamily.variants;
            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i].skillDef == skillDef)
                {
                    return i;
                }
            }
            return -1;
        }

        public RendererStore GetRendererStore(CharacterModel model) {
            RendererStore store = model.GetComponent<RendererStore>();
            if (!store) {
                store = model.AddComponent<RendererStore>();
                store.targetSkill = SkillDef;
            }

            return store;
        }

        public void FillStore(RendererStore store, CharacterModel model) {
            for (int i = 0; i < model.baseRendererInfos.Length; i++) {
                CharacterModel.RendererInfo rendInfo = model.baseRendererInfos[i];
                RendererStore.Info info = store[i];
                info.mat = rendInfo.defaultMaterial;
                
                if (rendInfo.renderer is SkinnedMeshRenderer) {
                    info.mesh = (rendInfo.renderer as SkinnedMeshRenderer).sharedMesh;
                }
                else if (rendInfo.renderer is MeshRenderer) {
                    info.mesh = rendInfo.renderer.GetComponent<MeshFilter>().mesh;
                }

                if (info.mesh) {
                    // Debug.Log("set info mesh to: " + info.mesh);
                    info.rotation = rendInfo.renderer.transform.localRotation;
                    info.scale = rendInfo.renderer.transform.localScale;
                }
            }
        }

        public void ReloadFromStore(CharacterModel.RendererInfo info, RendererStore.Info store) {
            info.defaultMaterial = store.mat;
            info.renderer.material = store.mat;

            // Debug.Log("store mesh: " + store.mesh);
            
            if (info.renderer is SkinnedMeshRenderer) {
                (info.renderer as SkinnedMeshRenderer).sharedMesh = store.mesh;
            }
            else if (info.renderer is MeshRenderer) {
                info.renderer.GetComponent<MeshFilter>().sharedMesh = store.mesh;
            }

            info.renderer.transform.localRotation = store.rotation;
            info.renderer.transform.localScale = store.scale;
        }

        public abstract void SetupRendererStore(RendererStore store, CharacterModel model);
    }

    public class RendererStore : MonoBehaviour {
        public class Info {
            public Mesh mesh;
            public Material mat;
            public Quaternion rotation;
            public Vector3 scale;
            public int index;
        }

        public SkillDef targetSkill;
        public int[] noUpdateIndices = new int[0];

        private List<Info> infos = new();

        public static event EventHandler init;

        public void OnEnable() {
            FillStore(this, GetComponent<CharacterModel>());
            
            init?.Invoke(null, null);
        }

        public void FillStore(RendererStore store, CharacterModel model) {
            for (int i = 0; i < model.baseRendererInfos.Length; i++) {
                CharacterModel.RendererInfo rendInfo = model.baseRendererInfos[i];
                RendererStore.Info info = store[i];
                info.mat = rendInfo.defaultMaterial;
                
                if (rendInfo.renderer is SkinnedMeshRenderer) {
                    info.mesh = (rendInfo.renderer as SkinnedMeshRenderer).sharedMesh;
                }
                else if (rendInfo.renderer is MeshRenderer) {
                    info.mesh = rendInfo.renderer.GetComponent<MeshFilter>().mesh;
                }

                // Debug.Log("set info mesh to: " + info.mesh);

                info.rotation = rendInfo.renderer.transform.localRotation;
                info.scale = rendInfo.renderer.transform.localScale;
            }
        }

        public Info this[int index] {
            get {
                Info info = infos.FirstOrDefault(x => x.index == index);
                if (info == null) {
                    info = new();
                    info.index = index;
                    infos.Add(info);
                }

                return info;
            }
        }

        public int Length => infos.Count;
    }
}