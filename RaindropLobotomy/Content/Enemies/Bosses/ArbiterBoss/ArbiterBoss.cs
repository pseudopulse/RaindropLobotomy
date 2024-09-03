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
using RaindropLobotomy.Buffs;

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

        public override string ConfigName => "An Arbiter";

        //

        public override void Create()
        {
            // unfinished, dont load
        }

        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("BinahBody.prefab");
            prefabMaster = Load<GameObject>("BinahMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            "RL_ARBITER_NAME".Add("An Arbiter");

            ArbiterMaterials.InitMaterials();

            CreateVFX();

            On.RoR2.HealthComponent.TakeDamageProcess += (orig, self, info) => {
                orig(self, info);
                if (self.name.Contains("BinahBody")) {
                    self.body.skillLocator.special.ExecuteIfReady();
                }
            };

            PillarProjectile = Load<GameObject>("PillarProjectile.prefab");
            PillarProjectile.AddComponent<ArbiterArcingPillarBehaviour>();
            DamageAPI.ModdedDamageTypeHolderComponent holder = PillarProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>();
            holder.Add(Fairy.TripleFairyOnHit);
            ContentAddition.AddProjectile(PillarProjectile);
        }

        public void CreateVFX() {
            // standard fairy charge
            ArbiterSlashEffect = PrefabAPI.InstantiateClone(Paths.GameObject.MercSwordFinisherSlash, "ArbiterSlashEffect");
            ArbiterSlashEffect.transform.Find("Sparks").gameObject.SetActive(false);
            ArbiterSlashEffect.transform.Find("SwingTrail").GetComponent<ParticleSystemRenderer>().material = ArbiterMaterials.matArbiterSlashMat;
            ArbiterSlashEffect.transform.GetComponent<DestroyOnTimer>().enabled = false;

            // standard fairy tracer
            FairyTracerEffect = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorBeamTracer, "FairyTracerEffect");
            FairyTracerEffect.GetComponent<LineRenderer>().sharedMaterials = new Material[] { Paths.Material.matConstructBeamInitial };
            FairyTracerEffect.GetComponent<LineRenderer>().textureMode = LineTextureMode.Tile;
            FairyTracerEffect.GetComponent<LineRenderer>().widthMultiplier = 2f;
            FairyTracerEffect.GetComponent<LineRenderer>().startColor = new Color32(241, 255, 0, 49);
            FairyTracerEffect.GetComponent<LineRenderer>().endColor = new Color32(250, 255, 0, 255);
            FairyTracerEffect.transform.localPosition = Vector3.zero;
            ContentAddition.AddEffect(FairyTracerEffect);

            // fairy tracer slash
            FairyTracerSlashEffect = PrefabAPI.InstantiateClone(Paths.GameObject.ImpactMercEvis, "FairyTracerSlash");
            FairyTracerSlashEffect.transform.Find("Hologram").GetComponent<ParticleSystemRenderer>().enabled = false;
            FairyTracerSlashEffect.transform.Find("Flash").GetComponent<ParticleSystemRenderer>().material = ArbiterMaterials.matArbiterSlashMat;
            FairyTracerSlashEffect.transform.Find("SwingTrail").GetComponent<ParticleSystemRenderer>().enabled = false;
            FairyTracerSlashEffect.transform.Find("Point Light").gameObject.SetActive(false);
            ParticleSystem.MainModule main = FairyTracerSlashEffect.transform.Find("Flash").GetComponent<ParticleSystem>().main;
            main.startLifetime = 1.5f;
            main.startSizeMultiplier *= 0.3f;
            main.simulationSpeed = 2.5f;
            for (int i = 0; i < 8; i++) {
                Transform target = FairyTracerSlashEffect.transform.Find("Flash");
                GameObject duplicate = GameObject.Instantiate(target.gameObject, FairyTracerSlashEffect.transform);
                duplicate.transform.localPosition = target.transform.localPosition;
                duplicate.transform.localRotation = Quaternion.LookRotation(Random.onUnitSphere.normalized);
            }
            FairyTracerSlashEffect.AddComponent<DestroyOnTimer>().duration = 3.5f;
            ContentAddition.AddEffect(FairyTracerSlashEffect);

            // fairy muzzle flash
            FairyMuzzleFlash = PrefabAPI.InstantiateClone(Paths.GameObject.MuzzleflashFireMeatBall, "FairyMuzzleFlash");
            FairyMuzzleFlash.GetComponentInChildren<ParticleSystemRenderer>().material = Paths.Material.matMagmaWormFireballTrail;
            ContentAddition.AddEffect(FairyMuzzleFlash);

            // shockwave charge
            ShockwaveChargeEffect = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorChargeMegaBlaster, "ShockwaveChargeEffect");
            ShockwaveChargeEffect.transform.Find("Base").gameObject.SetActive(false);
            ShockwaveChargeEffect.transform.Find("Base (1)").gameObject.SetActive(false);
            ShockwaveChargeEffect.transform.Find("Sparks, In").gameObject.SetActive(false);
            ShockwaveChargeEffect.transform.Find("Sparks, Misc").gameObject.SetActive(false);
            ShockwaveChargeEffect.transform.Find("OrbCore").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Paths.Material.matGrandParentMoonCore, Paths.Material.matVoidBlinkPortal };
            ShockwaveChargeEffect.transform.GetComponent<ObjectScaleCurve>().enabled = false;
            ShockwaveChargeEffect.transform.localScale = Vector3.zero;
            
            // shockwave telegraph
            ShockwaveTelegraphEffect = PrefabAPI.InstantiateClone(Paths.GameObject.VagrantNovaAreaIndicator, "ShockwaveAreaIndicator");
            ShockwaveTelegraphEffect.GetComponentInChildren<ParticleSystemRenderer>().material = Paths.Material.matGrandParentSunChannelStartBeam;
            ShockwaveTelegraphEffect.GetComponent<MeshRenderer>().sharedMaterials = new Material[] {
                Paths.Material.matVoidDeathBombAreaIndicatorBack,
                Paths.Material.matMoonbatteryGlassDistortion,
                Paths.Material.matParentTeleportIndicator,
                Paths.Material.matAreaIndicatorRim
            };
            ShockwaveTelegraphEffect.GetComponent<ObjectScaleCurve>().enabled = false;
            ShockwaveTelegraphEffect.transform.localScale = Vector3.zero;

            // shockwave effect
            ShockwaveEffect = PrefabAPI.InstantiateClone(Paths.GameObject.RailgunnerMineExplosion, "ShockwaveEffect");
            ShockwaveEffect.transform.Find("Sphere, Distortion").gameObject.SetActive(false);
            ShockwaveEffect.transform.Find("Core").gameObject.SetActive(false);
            ShockwaveEffect.transform.Find("Flash, White").gameObject.SetActive(false);
            ShockwaveEffect.transform.Find("Flash, Colored").gameObject.SetActive(false);
            ShockwaveEffect.transform.Find("SparksOut").GetComponent<ParticleSystemRenderer>().material = Paths.Material.matGrandParentSunGlow;
            ShockwaveEffect.transform.Find("Sphere, Color").GetComponent<ParticleSystemRenderer>().material = Paths.Material.matMagmaWormExplosionSphere;
            ShockwaveEffect.transform.Find("Point Light").GetComponent<Light>().color = new Color32(255, 191, 42, 255);
            
            // portal effect
            PillarPortalEffect = Load<GameObject>("PortalEffect.prefab");
            // PillarPortalEffect.transform.Find("Plane").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Paths.Material.matOmniRing1ArchWisp, Paths.Material.matGrandParentSunGlow };
            // PillarPortalEffect.transform.Find("Plane (1)").GetComponent<MeshRenderer>().sharedMaterials = new Material[] { Paths.Material.matArtifactShellDistortion, Paths.Material.matMegaDroneFlare1 };
        }

        public class ArbiterArcingPillarBehaviour : MonoBehaviour {
            public ProjectileController projectile;
            public ProjectileDamage projectileDamage;
            public BeizerCurve arcPath;
            public Renderer pillarRenderer;
            public Timer timer = new(2f);
            public GameObject portalEffect;
            //
            private Vector3 TargetPos;
            private Transform Spearhead;
            private float TargetY = 6.8f;
            private float TotalYToRaise = (1.99f + 6.8f);
            private float speed = 20f;
            private float totalDistTraversed = 0f;
            public int waveProjectileCount = 6;
            public GameObject waveProjectilePrefab => Paths.GameObject.BrotherSunderWave;
            private float yPerSec => TotalYToRaise / timer.duration;

            public void Start() {
                projectile = GetComponent<ProjectileController>();
                projectileDamage = GetComponent<ProjectileDamage>();
                Vector3? tPos = (GetComponent<ProjectileTargetComponent>().target.transform.position + new Vector3(0f, 1f, 0f)).GroundPoint();

                if (!tPos.HasValue) {
                    this.enabled = false;
                    Destroy(this.gameObject);
                    return;
                }

                Spearhead = transform.Find("Target");

                Vector3 cPos = Spearhead.transform.position + new Vector3(0f, TotalYToRaise, 0f);
                Vector3 midpoint = PickArcPathMidpoint(tPos.Value, cPos, 30f);

                arcPath = new(cPos, midpoint, tPos.Value);

                pillarRenderer = Spearhead.GetComponentInChildren<MeshRenderer>();
                portalEffect = GameObject.Instantiate(PillarPortalEffect, base.transform.position, Quaternion.LookRotation(Vector3.up));

                pillarRenderer.material.SetVector("_ClippingView", -Vector3.up);
                pillarRenderer.material.SetVector("_ObjectPos", portalEffect.transform.position);
                pillarRenderer.material.SetFloat("_ShouldClip", 0f);

                TargetPos = tPos.Value;
            }

            private Vector3 PickArcPathMidpoint(Vector3 start, Vector3 dest, float arcHeight) {
                Vector3 mid = Vector3.Lerp(start, dest, 0.5f);
                mid.y += arcHeight;

                if (Physics.Raycast(mid, Vector3.up, arcHeight + 7f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore)) {
                    mid.y -= 7f;
                }

                return mid;
            }

            public void FixedUpdate() {
                if (timer.Tick()) {
                    pillarRenderer.material.SetFloat("_ShouldClip", 1f);

                    totalDistTraversed += speed * Time.fixedDeltaTime;
                    
                    Vector3 next = arcPath.GetBeizerPointAtDistance(totalDistTraversed);
                    Vector3 forward = arcPath.GetRotationAlongCurve(totalDistTraversed);
                    Vector3 rotation = Vector3.RotateTowards(Spearhead.up, -forward, (200f * (Mathf.PI / 180)), float.PositiveInfinity);
                    Spearhead.up = rotation;
                    Spearhead.position = next;

                    // Debug.Log(Spearhead.position + " : sp");
                    // Debug.Log(TargetPos + " : tp");

                    if (Vector3.Distance(Spearhead.position, TargetPos) < 0.5f) {
                        Detonate();
                        timer.expired = true;
                    }
                }
                else {
                    Spearhead.localPosition += new Vector3(0f, TotalYToRaise * Time.fixedDeltaTime, 0f);
                }
            }

            public void Detonate() {
                FireRingAuthority(Spearhead.transform.position, base.transform.forward);
                base.enabled = false;
            }

            private void FireRingAuthority(Vector3 footPosition, Vector3 normal)
            {
                float num = 360f / (float)waveProjectileCount;
                Vector3 vector = Vector3.ProjectOnPlane(normal, Vector3.up);
                for (int i = 0; i < waveProjectileCount; i++)
                {
                    Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * vector;
                    if (NetworkServer.active)
                    {
                        ProjectileManager.instance.FireProjectile(waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), projectile.owner, projectileDamage.damage, 4000f, false);
                    }
                }
            }
        }

        public class DetonateOnImpact : MonoBehaviour, IProjectileImpactBehavior
        {
            public int waveProjectileCount = 6;
            public GameObject waveProjectilePrefab => Paths.GameObject.BrotherSunderWave;
            public ProjectileController projectile;
            public ProjectileDamage projectileDamage;
            public bool hasFiredRing = false;

            public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
            {
                if (!hasFiredRing) {
                    hasFiredRing = true;
                    FireRingAuthority(impactInfo.estimatedPointOfImpact, impactInfo.estimatedImpactNormal);
                }
            }

            private void Start() {
                projectile = GetComponent<ProjectileController>();
                projectileDamage = GetComponent<ProjectileDamage>();
            }

            private void FireRingAuthority(Vector3 footPosition, Vector3 normal)
            {
                float num = 360f / (float)waveProjectileCount;
                Vector3 vector = Vector3.ProjectOnPlane(normal, Vector3.up);
                for (int i = 0; i < waveProjectileCount; i++)
                {
                    Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * vector;
                    if (NetworkServer.active)
                    {
                        ProjectileManager.instance.FireProjectile(waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), projectile.owner, projectileDamage.damage, 4000f, false);
                    }
                }
            }
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
                    GetComponent<Rigidbody>().useGravity = true;
                    GetComponent<BoxCollider>().enabled = true;
                }
            }
        }
    }
}