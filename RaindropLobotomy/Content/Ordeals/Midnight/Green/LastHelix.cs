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
using RaindropLobotomy.Enemies;

namespace RaindropLobotomy.Ordeals.Midnight.Green
{
    public class LastHelix : EnemyBase<LastHelix>
    {
        public static GameObject LaserPrefab;
        public static GameObject PlasmaPrefab;
        public static GameObject IndicatorPrefab;
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("LastHelixBody.prefab");
            prefabMaster = Load<GameObject>("LastHelixMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            LanguageAPI.Add("RL_LASTHELIX_NAME", "Helix of the End");
            LanguageAPI.Add("RL_LASTHELIX_SUB", "Ordeal of Green Midnight");


            // EFFECTS
            LaserPrefab = Load<GameObject>("HelixLaser.prefab");
            PlasmaPrefab = Load<GameObject>("PlasmaTrailSegment.prefab");
            IndicatorPrefab = Load<GameObject>("HelixLaserIndicator.prefab");

            PlasmaPrefab.AddComponent<PlasmaDamage>();
        }
    }

    public class PlasmaDamage : MonoBehaviour {
        public CharacterBody owner;
        public float damagePerSecond;
        private int ticksPerSecond = 5;
        private float delay => 1f / ticksPerSecond;
        private float stopwatch = 0f;
        private float damage => damagePerSecond / ticksPerSecond;
        private OverlapAttack attack;
        public void Start() {
            attack = new();
            attack.damage = damage;
            attack.attacker = owner.gameObject;
            attack.procCoefficient = 0;
            attack.damageColorIndex = DamageColorIndex.Poison;
            attack.hitBoxGroup = GetComponent<HitBoxGroup>();
            attack.isCrit = false;
        }

        public void FixedUpdate() {
            if (NetworkServer.active) {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= delay) {
                    stopwatch = 0f;
                    attack.ResetIgnoredHealthComponents();
                    attack.Fire();
                }
            }
        }
    }
}