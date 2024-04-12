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
using RoR2.ConVar;

namespace RaindropLobotomy.Enemies.ArbiterBoss {
    public class ArbiterBoss : EnemyBase<ArbiterBoss>
    {
        public static GameObject ArbiterSlashEffect;
        public static GameObject FairyTracerEffect;
        public static GameObject FairyTracerSlashEffect;
        public static GameObject FairyMuzzleFlash;
        //
        public static GameObject ShockwaveChargeEffect;
        public static GameObject ShockwaveTelegraphEffect;
        public static GameObject ShockwaveEffect;
        //
        public static GameObject PillarProjectile;
        public static GameObject PillarPortalEffect;
        //
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("BinahBody.prefab");
            prefabMaster = Load<GameObject>("BinahMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            "RL_ARBITER_NAME".Add("An Arbiter");

            ArbiterMaterials.InitMaterials();

            CreateVFX();

            On.RoR2.HealthComponent.TakeDamage += (orig, self, info) => {
                orig(self, info);
                if (self.name.Contains("BinahBody")) {
                    self.body.skillLocator.special.ExecuteIfReady();
                }
            };

            PillarProjectile = Load<GameObject>("PillarProjectile.prefab");
            PillarProjectile.AddComponent<DelayedPillarShot>();
            ContentAddition.AddProjectile(PillarProjectile);
        }

        public void CreateVFX() {
            // standard fairy charge
            ArbiterSlashEffect = PrefabAPI.InstantiateClone(Assets.GameObject.MercSwordFinisherSlash, "ArbiterSlashEffect");
            ArbiterSlashEffect.transform.Find("Sparks").gameObject.SetActive(false);
            ArbiterSlashEffect.transform.Find("SwingTrail").GetComponent<ParticleSystemRenderer>().material = ArbiterMaterials.matArbiterSlashMat;
            ArbiterSlashEffect.transform.GetComponent<DestroyOnTimer>().enabled = false;

            // standard fairy tracer
            FairyTracerEffect = PrefabAPI.InstantiateClone(Assets.GameObject.VoidSurvivorBeamTracer, "FairyTracerEffect");
            FairyTracerEffect.GetComponent<LineRenderer>().sharedMaterials = new Material[] { Assets.Material.matConstructBeamInitial };
            FairyTracerEffect.GetComponent<LineRenderer>().textureMode = LineTextureMode.Tile;
            FairyTracerEffect.GetComponent<LineRenderer>().widthMultiplier = 2f;
            FairyTracerEffect.GetComponent<LineRenderer>().startColor = new Color32(241, 255, 0, 49);
            FairyTracerEffect.GetComponent<LineRenderer>().endColor = new Color32(250, 255, 0, 255);
            FairyTracerEffect.transform.localPosition = Vector3.zero;
            ContentAddition.AddEffect(FairyTracerEffect);

            // fairy tracer slash
            FairyTracerSlashEffect = PrefabAPI.InstantiateClone(Assets.GameObject.ImpactMercEvis, "FairyTracerSlash");
            FairyTracerSlashEffect.transform.Find("Hologram").GetComponent<ParticleSystemRenderer>().enabled = false;
            FairyTracerSlashEffect.transform.Find("Flash").GetComponent<ParticleSystemRenderer>().material = ArbiterMaterials.matArbiterSlashMat;
            FairyTracerSlashEffect.transform.Find("SwingTrail").GetComponent<ParticleSystemRenderer>().enabled = false;
            FairyTracerSlashEffect.transform.Find("Point Light").gameObject.SetActive(false);
            ParticleSystem.MainModule main = FairyTracerSlashEffect.transform.Find("Flash").GetComponent<ParticleSystem>().main;
            main.startLifetime = 1.5f;
            main.startSizeMultiplier *= 0.2f;
            main.simulationSpeed = 2f;
            for (int i = 0; i < 8; i++) {
                Transform target = FairyTracerSlashEffect.transform.Find("Flash");
                GameObject duplicate = GameObject.Instantiate(target.gameObject, FairyTracerSlashEffect.transform);
                duplicate.transform.localPosition = target.transform.localPosition;
                duplicate.transform.localRotation = Quaternion.LookRotation(Random.onUnitSphere.normalized);
            }
            FairyTracerSlashEffect.AddComponent<DestroyOnTimer>().duration = 3.5f;
            ContentAddition.AddEffect(FairyTracerSlashEffect);

            // fairy muzzle flash
            FairyMuzzleFlash = PrefabAPI.InstantiateClone(Assets.GameObject.MuzzleflashFireMeatBall, "FairyMuzzleFlash");
            FairyMuzzleFlash.GetComponentInChildren<ParticleSystemRenderer>().material = Assets.Material.matMagmaWormFireballTrail;
            ContentAddition.AddEffect(FairyMuzzleFlash);

            // shockwave charge
            ShockwaveChargeEffect = PrefabAPI.InstantiateClone(Assets.GameObject.VoidSurvivorChargeMegaBlaster, "ShockwaveChargeEffect");
            ShockwaveChargeEffect.transform.Find("Base").gameObject.SetActive(false);
            ShockwaveChargeEffect.transform.Find("Base (1)").gameObject.SetActive(false);
            ShockwaveChargeEffect.transform.Find("Sparks, In").gameObject.SetActive(false);
            ShockwaveChargeEffect.transform.Find("Sparks, Misc").gameObject.SetActive(false);
            ShockwaveChargeEffect.transform.Find("OrbCore").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Assets.Material.matGrandParentMoonCore, Assets.Material.matVoidBlinkPortal };
            ShockwaveChargeEffect.transform.GetComponent<ObjectScaleCurve>().enabled = false;
            ShockwaveChargeEffect.transform.localScale = Vector3.zero;
            
            // shockwave telegraph
            ShockwaveTelegraphEffect = PrefabAPI.InstantiateClone(Assets.GameObject.VagrantNovaAreaIndicator, "ShockwaveAreaIndicator");
            ShockwaveTelegraphEffect.GetComponentInChildren<ParticleSystemRenderer>().material = Assets.Material.matGrandParentSunChannelStartBeam;
            ShockwaveTelegraphEffect.GetComponent<MeshRenderer>().sharedMaterials = new Material[] {
                Assets.Material.matVoidDeathBombAreaIndicatorBack,
                Assets.Material.matMoonbatteryGlassDistortion,
                Assets.Material.matParentTeleportIndicator,
                Assets.Material.matAreaIndicatorRim
            };
            ShockwaveTelegraphEffect.GetComponent<ObjectScaleCurve>().enabled = false;
            ShockwaveTelegraphEffect.transform.localScale = Vector3.zero;

            // shockwave effect
            ShockwaveEffect = PrefabAPI.InstantiateClone(Assets.GameObject.RailgunnerMineExplosion, "ShockwaveEffect");
            ShockwaveEffect.transform.Find("Sphere, Distortion").gameObject.SetActive(false);
            ShockwaveEffect.transform.Find("Core").gameObject.SetActive(false);
            ShockwaveEffect.transform.Find("Flash, White").gameObject.SetActive(false);
            ShockwaveEffect.transform.Find("Flash, Colored").gameObject.SetActive(false);
            ShockwaveEffect.transform.Find("SparksOut").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matGrandParentSunGlow;
            ShockwaveEffect.transform.Find("Sphere, Color").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matMagmaWormExplosionSphere;
            ShockwaveEffect.transform.Find("Point Light").GetComponent<Light>().color = new Color32(255, 191, 42, 255);
            
            // portal effect
            PillarPortalEffect = Load<GameObject>("PortalEffect.prefab");
            PillarPortalEffect.transform.Find("Plane").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Assets.Material.matOmniRing1ArchWisp, Assets.Material.matGrandParentSunGlow };
            PillarPortalEffect.transform.Find("Plane (1)").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Assets.Material.matArtifactShellDistortion, Assets.Material.matMegaDroneFlare1 };
        }

        
        public class DelayedPillarShot : MonoBehaviour {
            public Timer timer = new(2f, expires: true);
            public MeshRenderer renderer;
            public Material mat;
            public GameObject portalEffect;
            public Transform pillar;
            
            public void Start() {
                renderer = GetComponentInChildren<MeshRenderer>();
                portalEffect = GameObject.Instantiate(PillarPortalEffect, base.transform.position, Quaternion.LookRotation(-base.transform.forward));
                mat = renderer.material;
                pillar = renderer.transform;
                mat.SetVector("_ClippingView", portalEffect.transform.forward);
                mat.SetVector("_ObjectPos", portalEffect.transform.position);
            }
            public void FixedUpdate() {
                if (portalEffect) {
                    mat.SetVector("_ClippingView", portalEffect.transform.forward);
                    mat.SetVector("_ObjectPos", portalEffect.transform.position);
                }

                if (pillar.transform.localPosition.z < 0 && timer.cur >= 1f) {
                    pillar.transform.localPosition += new Vector3(0, 0, (5.38f / (timer.duration - 1f)) * Time.fixedDeltaTime);
                }

                if (timer.Tick()) {
                    GetComponent<ProjectileSimple>().enabled = true;
                }
            }
        }
    }
}