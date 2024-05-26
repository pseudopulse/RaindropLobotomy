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

namespace RaindropLobotomy.Enemies.Fragment
{
    public class UniverseFragment : EnemyBase<UniverseFragment>
    {
        public static BuffDef BewitchedDebuff;
        public static GameObject ScreamEffect;
        public static GameObject SpearThrust;
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("FragmentBody.prefab");
            prefabMaster = Load<GameObject>("FragmentMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            "RL_FRAGMENT_NAME".Add("Fragment Of The Universe");

            Material FragmentOverlay = Load<Material>("matFragmentOverlay.mat");
            FragmentOverlay.SetTexture("_MainTex", Assets.Texture2D.texWhite);
            FragmentOverlay.SetTexture("_RemapTex", Assets.Texture2D.texRampArcaneCircle);
            FragmentOverlay.SetTexture("_Cloud1Tex", Assets.Texture2D.texMagmaCloud);
            FragmentOverlay.SetTexture("_Cloud2Tex", Assets.Texture2D.texCloudLightning1);

            ScreamEffect = PrefabAPI.InstantiateClone(Assets.GameObject.Bandit2SmokeBomb, "FragmentScream");
            ScreamEffect.GetComponent<EffectComponent>().applyScale = true;
            ScreamEffect.transform.Find("Core").Find("Sparks").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matLifeStealOnHitAuraTrails;
            ScreamEffect.transform.Find("Core").Find("Smoke, Edge Circle").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matLifeStealOnHitAuraTrails;
            ScreamEffect.transform.Find("Core").Find("Dust, CenterSphere").gameObject.SetActive(false);
            ScreamEffect.transform.Find("Core").Find("Dust, CenterTube").gameObject.SetActive(false);
            ScreamEffect.MakeAbideByScaleRecursively();
            ContentAddition.AddEffect(ScreamEffect);

            SpearThrust = Load<GameObject>("SpearThrust.prefab");
        }
    }
}