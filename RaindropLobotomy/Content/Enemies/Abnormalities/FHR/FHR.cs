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

namespace RaindropLobotomy.Enemies.FHR
{
    public class FHR : EnemyBase<FHR>, Abnormality
    {
        public static GameObject VineProjectile;
        public static GameObject BloodShot;
        public static GameObject MuzzleFlashBloodShot;
        public static List<CharacterBody> ActiveFHR = new();

        public RiskLevel ThreatLevel => RiskLevel.Waw;

        public SpawnCard SpawnCard => Load<CharacterSpawnCard>("cscFragment.asset");

        public bool IsTool => false;

        public override string ConfigName => "Four Hundred Roses";
        public static LazyIndex FHRIndex = new("FHRBody");

        // TODO:
        // - make spawn card
        // - add bloodfeast mechanic
        // -- has an aura that applies a debuff to nearby enemies
        // -- enemies with the debuff who take bleed damage will send red orbs towards the 4HR closest to them
        // -- red orbs will heal 4HR and give it a stacking buff to its stats for a limited time

        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("FHRBody.prefab");
            prefabMaster = Load<GameObject>("FHRMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            "RL_FHR_NAME".Add("Four Hundred Roses");

            VineProjectile = Load<GameObject>("FHRVineShot.prefab");

            BloodShot = PrefabAPI.InstantiateClone(Paths.GameObject.CrocoSpit, "BloodShotProjectile");
            var BloodShotGhost = PrefabAPI.InstantiateClone(Paths.GameObject.CrocoSpitGhost, "BloodShotGhost");

            BloodShot.GetComponent<ProjectileController>().ghostPrefab = BloodShotGhost;
            BloodShot.GetComponent<Rigidbody>().useGravity = true;

            BloodShot.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;

            BloodShot.GetComponent<ProjectileImpactExplosion>().explosionEffect = Paths.GameObject.BleedOnHitAndExplodeExplosion;
            BloodShot.GetComponent<ProjectileImpactExplosion>().impactEffect = null;

            ContentAddition.AddProjectile(BloodShot);

            BloodShotGhost.FindParticle("Flashes").material = Paths.Material.matMageFirebolt;
            BloodShotGhost.FindParticle("Goo, WS").material = Load<Material>("matSweeperSlash.mat");
            BloodShotGhost.FindParticle("Goo Drippings").material = Paths.Material.matBloodSiphon;
            BloodShotGhost.FindComponent<TrailRenderer>("Trail").material = Load<Material>("matSweeperSlash.mat");

            ContentAddition.AddProjectile(VineProjectile);

            MuzzleFlashBloodShot = Paths.GameObject.BleedOnHitAndExplodeExplosion;

            prefab.transform.Find("BloodfeastWard").GetComponentInChildren<MeshRenderer>().sharedMaterial = Paths.Material.matSpiteBombSphereIndicator;

            prefab.AddComponent<FHRMarker>();
        }

        public static CharacterBody GetNearbyRecipient(Vector3 position) {
            float radius = 25f;
            CharacterBody fhr = null;
            
            for (int i = 0; i < ActiveFHR.Count; i++) {
                CharacterBody cb = ActiveFHR[i];

                if (Vector3.Distance(position, cb.corePosition) <= radius) {
                    fhr = cb;
                    break;
                }
            }

            return fhr;
        }

        public class FHRMarker : MonoBehaviour {
            private CharacterBody cb;
            public void Start() {
                cb = GetComponent<CharacterBody>();
                ActiveFHR.Add(cb);
            }

            public void OnDestroy() {
                ActiveFHR.Remove(cb);
            }
        }
    }
}