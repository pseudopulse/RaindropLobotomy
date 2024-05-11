using System;

namespace RaindropLobotomy.EGO.Bandit {
    public class EGOMagicBullet : CorrosionBase<EGOMagicBullet>
    {
        public override string EGODisplayName => "Magic Bullet";
        public override string Description => "This magical bullet can truly hit anyone, just like you say.";

        public override SurvivorDef TargetSurvivorDef => Assets.SurvivorDef.Bandit2;

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

        public override void Modify()
        {
            base.Modify();

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Assets.RuntimeAnimatorController.animBandit2;
            Load<GameObject>("DisplayMagicBullet.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Assets.RuntimeAnimatorController.animBandit2Display;

            BulletPrefab = Load<GameObject>("MagicBulletProjectile.prefab");
            BulletPrefab.GetComponentInChildren<MeshRenderer>().sharedMaterials = new Material[] {
                Assets.Material.matHelfirePuff,
                Assets.Material.matLunarTeleporterWater,
                Assets.Material.matMoonbatteryGlassDistortion,
                Assets.Material.matHelfirePuff
            };
            ContentAddition.AddProjectile(BulletPrefab);

            PortalPrefab = Load<GameObject>("MagicBulletPortal.prefab");
            PortalPrefab.AddComponent<MagicBulletPortal>();


            matMagicBulletPortal2 = Load<Material>("matMagicPortal2.mat");
            matMagicBulletPortal2.SetTexture("_MainTex", Assets.Texture2D.texAlphaGradient3Mask);
            matMagicBulletPortal2.SetTexture("_RemapTex", Assets.Texture2D.texRampBanditAlt);
            matMagicBulletPortal2.SetTexture("_Cloud1Tex", Assets.Texture2D.texCloudDirtyFire);
            matMagicBulletPortal2.SetTexture("_Cloud2Tex", Assets.Texture2D.texCloudIce);
            matMagicBulletPortal2.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            matMagicBulletPortal2.SetShaderKeywords(new string[] { "USE_CLOUDS", "VERTEXALPHA", "_EMISSION" });

            matMagicBulletPortal1 = Load<Material>("matMagicPortal1.mat");
            matMagicBulletPortal1.SetTexture("_RemapTex", Assets.Texture2D.texRampBombOrb);
            matMagicBulletPortal1.SetTexture("_Cloud1Tex", Assets.Texture2D.texCloudCrackedIce);
            matMagicBulletPortal1.SetTexture("_Cloud2Tex", Assets.Texture2D.texCloudOrganic2);
            matMagicBulletPortal1.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            matMagicBulletPortal1.SetTexture("_MainTex", Assets.Texture2D.texCloudWaterRipples);
            matMagicBulletPortal1.SetShaderKeywords(new string[] { "USE_CLOUDS", "VERTEXALPHA", "_EMISSION" });

            TracerPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.VoidSurvivorBeamTracer, "FruitLoopsBullet");
            TracerPrefab.GetComponent<LineRenderer>().widthMultiplier = 1f;
            TracerPrefab.GetComponent<LineRenderer>().startWidth = 1f;
            TracerPrefab.GetComponent<LineRenderer>().endWidth = 1f;
            TracerPrefab.GetComponent<LineRenderer>().material = matMagicBulletPortal2;
            TracerPrefab.GetComponentInChildren<Light>().color = Color;
            ContentAddition.AddEffect(TracerPrefab);

            FloodingBulletsPP = Load<GameObject>("MagicBulletPP.prefab");

            TeleportEffect = PrefabAPI.InstantiateClone(Assets.GameObject.Bandit2SmokeBomb, "FruitLoopsSmokebomb");
            TeleportEffect.transform.Find("Core").Find("Sparks").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matHelfirePuff;
            TeleportEffect.transform.Find("Core").Find("Smoke, Edge Circle").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matHelfirePuff;
            TeleportEffect.transform.Find("Core").Find("Dust, CenterSphere").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matOnHelfire;
            TeleportEffect.transform.Find("Core").Find("Dust, CenterTube").gameObject.SetActive(false);
            ContentAddition.AddEffect(TeleportEffect);

            MagicBulletSlash = PrefabAPI.InstantiateClone(Assets.GameObject.AssassinSlash, "MagicBulletSlash");
            MagicBulletSlash.transform.Find("SwingTrail").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matHelfirePuff;
            ContentAddition.AddEffect(MagicBulletSlash);
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_EGO_MAGICBULLET_NAME".Add("Bandit :: Magic Bullet");

            "RL_EGO_MAGICBULLET_PASSIVE_NAME".Add("The Seventh Bullet");
            "RL_EGO_MAGICBULLET_PASSIVE_DESC".Add("Upon reaching <style=cIsDamage>7</style> stacks of <style=cIsUtility>Magic Bullet</style>, replace your primary and special with <style=cDeath>Bullet of Despair</style>.");

            "RL_EGO_MAGICBULLET_PRIMARY_NAME".Add("Magic Bullet");
            "RL_EGO_MAGICBULLET_PRIMARY_DESC".Add("<style=cDeath>Inevitable</style>. Fire a <style=cIsUtility>piercing</style> bullet for <style=cIsDamage>380% damage</style>. The bullet will <style=cIsUtility>automatically target</style> an enemy near your aim.");

            "RL_EGO_MAGICBULLET_SECONDARY_NAME".Add("Ignition");
            "RL_EGO_MAGICBULLET_SECONDARY_DESC".Add("Strike forward, dealing <style=cIsDamage>450% damage</style> and inflicting <style=cIsUtility>Dark Flame</style>.");

            "RL_EGO_MAGICBULLET_UTILITY_NAME".Add("Silent Advance");
            "RL_EGO_MAGICBULLET_UTILITY_DESC".Add("<style=cIsUtility>Teleport</style> a medium distance. Hit nearby targets for <style=cIsDamage>200%</style>, inflicting <style=cIsUtility>Dark Flame</style>.");

            "RL_EGO_MAGICBULLET_SPECIAL_NAME".Add("Flooding Bullets");
            "RL_EGO_MAGICBULLET_SPECIAL_DESC".Add("<style=cDeath>Inevitable</style>. Fire <style=cIsUtility>piercing</style> bullets at <style=cIsDamage>all</style> targets who have <style=cIsUtility>Dark Flame</style>, dealing <style=cIsDamage>3x900%</style> damage.");
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
            EffectManager.SimpleEffect(Assets.GameObject.OmniImpactVFXHuntress, this.transform.position, Quaternion.identity, true);

            foreach (MagicBulletPortal portal in outputPortals) {
                attack.aimVector = portal.transform.forward;
                attack.weapon = portal.gameObject;
                attack.origin = portal.transform.position;
                attack.maxDistance = 900f;
                attack.Fire();

                EffectManager.SimpleEffect(Assets.GameObject.OmniImpactVFXHuntress, portal.transform.position, Quaternion.identity, true);
            }
        }

        /*public void OnTriggerEnter(Collider collider) {
            if (isOutput) {
                Debug.Log("output portal, returning.");
                return;
            }

            Debug.Log("object has entered our trigger");
            Debug.Log(collider.gameObject.name);

            if (collider.gameObject.name == "PortalCollider") {
                GameObject proj = collider.transform.root.gameObject;
                ProjectileController controller = proj.GetComponent<ProjectileController>();
                ProjectileDamage damage = controller.GetComponent<ProjectileDamage>();

                Debug.Log("proceeded with teleport");

                for (int i = 0; i < outputPortals.Count; i++) {
                    if (!NetworkServer.active) break;

                    MagicBulletPortal output = outputPortals[i];

                    Debug.Log("outputting to portal: " + output);
                    Debug.Log("output portal is: " + output.isOutput);
                    
                    ProjectileManager.instance.FireProjectile(proj, output.transform.position, Util.QuaternionSafeLookRotation(output.transform.forward),
                    controller.owner, damage.damage, 0f, damage.crit);
                }

                Debug.Log("destroying self");

                GameObject.Destroy(proj);
            }
        }*/
    }
}