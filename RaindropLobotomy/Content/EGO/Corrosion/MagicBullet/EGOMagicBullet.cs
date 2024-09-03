using System;

namespace RaindropLobotomy.EGO.Bandit {
    public class EGOMagicBullet : CorrosionBase<EGOMagicBullet>
    {
        public override string EGODisplayName => "Magic Bullet";
        public override string Description => "This magical bullet can truly hit anyone, just like you say.";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.Bandit2;

        public override UnlockableDef RequiredUnlock => null;

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdMagicBulletEGO.asset");

        public override GameObject BodyPrefab => Load<GameObject>("MagicBulletBody.prefab");

        public override GameObject MasterPrefab => null;

        public override Color Color => new Color32(91, 100, 255, 255);

        public static GameObject BulletPrefab;
        public static GameObject PortalPrefab;

        public static Material matMagicBulletPortal2;
        public static Material matMagicBulletPortal1;

        public static GameObject TracerPrefab;
        public static GameObject FloodingBulletsPP;

        public static GameObject TeleportEffect;
        public static GameObject MagicBulletSlash;

        public static GameObject MegaTracerPrefab;

        public static SkillDef Despair;
        public static BuffDef MB => Buffs.MagicBullet.Instance.Buff;
        public static DamageAPI.ModdedDamageType DespairDamage = DamageAPI.ReserveDamageType();

        public class MagicBulletConfig : ConfigClass
        {
            public override string Section => "EGO Corrosions :: Magic Bullet";
            public bool UseVanillaSounds => Option<bool>("Use Vanilla Sounds", "Should Magic Bullet use vanilla sound effects?", false);

            public override void Initialize()
            {
                _ = UseVanillaSounds;
            }
        }

        public static MagicBulletConfig config = new();

        public override void Modify()
        {
            base.Modify();

            BodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandard;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animBandit2;
            Load<GameObject>("DisplayMagicBullet.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animBandit2Display;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Paths.ItemDisplayRuleSet.idrsBandit2;
            BodyPrefab.AddComponent<MagicBulletTargeter>();
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Paths.GameObject.VoidSurvivorCrosshair;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Paths.GameObject.GenericFootstepDust;

            BulletPrefab = Load<GameObject>("MagicBulletProjectile.prefab");
            BulletPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterials = new Material[] {
                Paths.Material.matHelfirePuff,
                Paths.Material.matLunarTeleporterWater,
                Paths.Material.matMoonbatteryGlassDistortion,
                Paths.Material.matHelfirePuff
            };
            ContentAddition.AddProjectile(BulletPrefab);

            PortalPrefab = Load<GameObject>("MagicBulletPortal.prefab");
            PortalPrefab.AddComponent<MagicBulletPortal>();

            Material matMagicBulletPortal = Load<Material>("matMagicPortal.mat");
            matMagicBulletPortal.SetTexture("_RemapTex", Paths.Texture2D.texRampMoonArenaWall);
            matMagicBulletPortal.SetTexture("_Cloud1Tex", Paths.Texture2D.texCloudStroke1);


            matMagicBulletPortal2 = Load<Material>("matMagicPortal2.mat");
            matMagicBulletPortal2.SetTexture("_MainTex", Paths.Texture2D.texAlphaGradient3Mask);
            matMagicBulletPortal2.SetTexture("_RemapTex", Paths.Texture2D.texRampBanditAlt);
            matMagicBulletPortal2.SetTexture("_Cloud1Tex", Paths.Texture2D.texCloudDirtyFire);
            matMagicBulletPortal2.SetTexture("_Cloud2Tex", Paths.Texture2D.texCloudIce);
            matMagicBulletPortal2.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            matMagicBulletPortal2.SetShaderKeywords(new string[] { "USE_CLOUDS", "VERTEXALPHA", "_EMISSION" });

            matMagicBulletPortal1 = Load<Material>("matMagicPortal1.mat");
            matMagicBulletPortal1.SetTexture("_RemapTex", Paths.Texture2D.texRampBombOrb);
            matMagicBulletPortal1.SetTexture("_Cloud1Tex", Paths.Texture2D.texCloudCrackedIce);
            matMagicBulletPortal1.SetTexture("_Cloud2Tex", Paths.Texture2D.texCloudOrganic2);
            matMagicBulletPortal1.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            matMagicBulletPortal1.SetTexture("_MainTex", Paths.Texture2D.texCloudWaterRipples);
            matMagicBulletPortal1.SetShaderKeywords(new string[] { "USE_CLOUDS", "VERTEXALPHA", "_EMISSION" });

            TracerPrefab = PrefabAPI.InstantiateClone(Paths.GameObject.VoidSurvivorBeamTracer, "FruitLoopsBullet");
            TracerPrefab.GetComponent<LineRenderer>().widthMultiplier = 1f;
            TracerPrefab.GetComponent<LineRenderer>().startWidth = 1f;
            TracerPrefab.GetComponent<LineRenderer>().endWidth = 1f;
            TracerPrefab.GetComponent<LineRenderer>().material = matMagicBulletPortal2;
            TracerPrefab.GetComponentInChildren<Light>().color = Color;
            ContentAddition.AddEffect(TracerPrefab);

            MegaTracerPrefab = PrefabAPI.InstantiateClone(Paths.GameObject.TracerRailgunCryo, "FruitLoopsMegaBullet");
            MegaTracerPrefab.FindParticle("BeamParticles, Rings").material = Paths.Material.matEliteLunarDonut;
            MegaTracerPrefab.FindComponent<LineRenderer>("Beam, Linger").material = Paths.Material.matLunarWispMinigunTracer;
            MegaTracerPrefab.FindComponent<LineRenderer>("Beam, Linger").startColor = new Color32(0, 65, 255, 255);
            MegaTracerPrefab.FindComponent<LineRenderer>("Beam, Linger").startColor = new Color32(23, 0, 255, 255);
            MegaTracerPrefab.FindComponent<ObjectScaleCurve>("mdlRailgunnerBeam").gameObject.SetActive(false);
            ContentAddition.AddEffect(MegaTracerPrefab);

            FloodingBulletsPP = Load<GameObject>("MagicBulletPP.prefab");

            TeleportEffect = PrefabAPI.InstantiateClone(Paths.GameObject.Bandit2SmokeBomb, "FruitLoopsSmokebomb");
            TeleportEffect.transform.Find("Core").Find("Sparks").GetComponent<ParticleSystemRenderer>().material = Paths.Material.matHelfirePuff;
            TeleportEffect.transform.Find("Core").Find("Smoke, Edge Circle").GetComponent<ParticleSystemRenderer>().material = Paths.Material.matHelfirePuff;
            TeleportEffect.transform.Find("Core").Find("Dust, CenterSphere").gameObject.SetActive(false);
            TeleportEffect.transform.Find("Core").Find("Dust, CenterTube").gameObject.SetActive(false);
            var main = TeleportEffect.FindComponent<ParticleSystem>("Smoke, Edge Circle").main;
            main.startSpeed = main.startSpeed.constant * 2f;
            main.startSizeMultiplier = 0.6f;
            ContentAddition.AddEffect(TeleportEffect);

            MagicBulletSlash = Load<GameObject>("MagicBulletSlash.prefab");

            Despair = Load<SkillDef>("DespairBullet.asset");

            On.RoR2.HealthComponent.TakeDamageProcess += DespairMultiplier;
        }

        private void DespairMultiplier(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self.body.teamComponent.teamIndex != TeamIndex.Player && damageInfo.HasModdedDamageType(DespairDamage)) {
                damageInfo.damage *= 10f;
                damageInfo.damageType &= ~DamageType.NonLethal;
            }

            orig(self, damageInfo);
        }

        public static bool GiveAmmo(CharacterBody body) {
            SkillLocator loc = body.skillLocator;

            if (NetworkServer.active) body.AddBuff(MB);

            if (body.GetBuffCount(MB) >= 7) {
                if (body.hasAuthority) {
                    loc.primary.SetSkillOverride(body, Despair, GenericSkill.SkillOverridePriority.Contextual);
                    loc.special.SetSkillOverride(body, Despair, GenericSkill.SkillOverridePriority.Contextual);
                }

                return true;
            }

            return false;
        }

        public static void SpendAmmo(CharacterBody body) {
            SkillLocator loc = body.skillLocator;

            if (NetworkServer.active) body.SetBuffCount(MB.buffIndex, 0);

            if (body.hasAuthority) {
                loc.primary.UnsetSkillOverride(body, Despair, GenericSkill.SkillOverridePriority.Contextual);
                loc.special.UnsetSkillOverride(body, Despair, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_EGO_MAGICBULLET_NAME".Add("Bandit :: Magic Bullet");

            "RL_EGO_MAGICBULLET_PASSIVE_NAME".Add("The Seventh Bullet");
            "RL_EGO_MAGICBULLET_PASSIVE_DESC".Add("Upon reaching <style=cIsDamage>7</style> stacks of <style=cIsUtility>Magic Bullet</style>, replace your primary and special with <style=cDeath>Bullet of Despair</style>.");

            "RL_EGO_MAGICBULLET_PRIMARY_NAME".Add("Magic Bullet");
            "RL_EGO_MAGICBULLET_PRIMARY_DESC".Add("<style=cDeath>Inevitable</style>. Fire a <style=cIsUtility>piercing</style> bullet for <style=cIsDamage>480% damage</style>. The bullet will <style=cIsUtility>automatically target</style> an enemy near your aim.");

            "RL_EGO_MAGICBULLET_SECONDARY_NAME".Add("Ignition");
            "RL_EGO_MAGICBULLET_SECONDARY_DESC".Add("Strike forward, dealing <style=cIsDamage>550% damage</style> and inflicting <style=cIsUtility>Dark Flame</style>.");

            "RL_EGO_MAGICBULLET_UTILITY_NAME".Add("Silent Advance");
            "RL_EGO_MAGICBULLET_UTILITY_DESC".Add("<style=cIsUtility>Teleport</style> a medium distance. Hit nearby targets for <style=cIsDamage>300%</style> damage, inflicting <style=cIsUtility>Dark Flame</style>.");

            "RL_EGO_MAGICBULLET_SPECIAL_NAME".Add("Flooding Bullets");
            "RL_EGO_MAGICBULLET_SPECIAL_DESC".Add("<style=cDeath>Inevitable</style>. Fire <style=cIsUtility>piercing</style> bullets at up to <style=cIsDamage>3 targets</style>, dealing <style=cIsDamage>3x1100%</style> damage.");

            "RL_EGO_MAGICBULLET_KYS_NAME".Add("Bullet of Despair");
            "RL_EGO_MAGICBULLET_KYS_DESC".Add("<style=cDeath>Indiscriminate.</style>. Fire a powerful piercing shot straight through yourself, dealing <style=cIsDamage>10% of your max hp</style> to you and <style=cIsDamage>10x that to enemies</style>. <style=cStack>This skill cannot kill you.</style>");
        }
    }

    public class MagicBulletTargeter : HurtboxTracker {
        public bool shouldTrack = true;
        public override void Start()
        {
            targetingIndicatorPrefab = Paths.GameObject.HuntressTrackingIndicator;
            maxSearchAngle = 25f;
            maxSearchDistance = 60f;
            targetType = TargetType.Enemy;
            isActiveCallback = () => {
                return shouldTrack;
            };

            base.Start();
        }
    }

    public class MagicBulletPortal : MonoBehaviour {
        public bool isOutput = false;
        public List<MagicBulletPortal> outputPortals = new();

        public Transform aimTarget;

        public void Update() {
            if (aimTarget) {
                base.transform.forward = (aimTarget.transform.position - base.transform.position).normalized;
            }
        }

        public void FireBullet(BulletAttack attack) {
            EffectManager.SimpleEffect(Paths.GameObject.OmniImpactVFXHuntress, this.transform.position, Quaternion.identity, true);

            foreach (MagicBulletPortal portal in outputPortals) {
                attack.aimVector = portal.transform.forward;
                attack.weapon = portal.gameObject;
                attack.origin = portal.transform.position;
                attack.maxDistance = 900f;
                attack.tracerEffectPrefab = EGOMagicBullet.MegaTracerPrefab;
                attack.Fire();

                EffectManager.SimpleEffect(Paths.GameObject.OmniImpactVFXHuntress, portal.transform.position, Quaternion.identity, true);
            }
        }

        /*public void OnTriggerEnter(Collider collider) {
            if (isOutput) {
                // Debug.Log("output portal, returning.");
                return;
            }

            // Debug.Log("object has entered our trigger");
            // Debug.Log(collider.gameObject.name);

            if (collider.gameObject.name == "PortalCollider") {
                GameObject proj = collider.transform.root.gameObject;
                ProjectileController controller = proj.GetComponent<ProjectileController>();
                ProjectileDamage damage = controller.GetComponent<ProjectileDamage>();

                // Debug.Log("proceeded with teleport");

                for (int i = 0; i < outputPortals.Count; i++) {
                    if (!NetworkServer.active) break;

                    MagicBulletPortal output = outputPortals[i];

                    // Debug.Log("outputting to portal: " + output);
                    // Debug.Log("output portal is: " + output.isOutput);
                    
                    ProjectileManager.instance.FireProjectile(proj, output.transform.position, Util.QuaternionSafeLookRotation(output.transform.forward),
                    controller.owner, damage.damage, 0f, damage.crit);
                }

                // Debug.Log("destroying self");

                GameObject.Destroy(proj);
            }
        }*/
    }
}