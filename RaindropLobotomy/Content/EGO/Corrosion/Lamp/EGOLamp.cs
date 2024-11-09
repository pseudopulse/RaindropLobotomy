using System;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace RaindropLobotomy.EGO.Mage {
    public class EGOLamp : CorrosionBase<EGOLamp>
    {
        public override string EGODisplayName => "Lamp";

        public override string Description => "There was no such 'beast' in the forest.";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.Mage;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => new Color32(255, 255, 255, 255);

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdLamp.asset");

        public override GameObject BodyPrefab => Load<GameObject>("LampBody.prefab");

        public override GameObject MasterPrefab => null;
        //
        public static bool IsDarknessActive => RaindropLobotomy.EGO.Mage.DarknessController.DarknessActive;
        public static GameObject LampSeekerBolt;
        public static GameObject DarknessController;
        public static GameObject LampAreaProjectile;
        public static DamageAPI.ModdedDamageType LightDamage = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType Enchanting = DamageAPI.ReserveDamageType();
        public static PostProcessProfile ppDarkness;
        public static RampFog fog;
        //
        // TODO:
        // - implement m2
        // - implement enchantment
        // - make LampAreaProjectile distract enemies in the darkness
        // - implement darkness on hit blocking

        public override void Create()
        {
            base.Create();

            PostProcessProfile ppProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
            UnityEngine.Object.DontDestroyOnLoad(ppProfile);
            ppProfile.name = "LampDarkness";
            fog = ppProfile.AddSettings<RampFog>();
            fog.SetAllOverridesTo(true);
            fog.fogColorStart.value = new Color32(0, 0, 0, 255);
            fog.fogColorMid.value = new Color32(0, 0, 0, 255);
            fog.fogColorEnd.value = new Color32(0, 0, 0, 255);
            fog.skyboxStrength.value = 0.02f;
            fog.fogPower.value = 1f;
            fog.fogIntensity.value = 0.95f;
            fog.fogZero.value = 0f;
            fog.fogOne.value = 0.01f;
            fog.fogHeightStart.value = 0f;
            fog.fogHeightEnd.value = 100f;
            fog.fogHeightIntensity.value = 0f;

            ppDarkness = ppProfile;
        }

        public override void Modify()
        {
            base.Modify();

            BodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandard;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animMage;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Paths.ItemDisplayRuleSet.idrsMage;
            Load<GameObject>("LampDisplay.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animMageDisplay;
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Paths.GameObject.CommandoBody.GetComponent<CharacterBody>().defaultCrosshairPrefab;
            // BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Paths.GameObject.GenericFootstepDust;

            BodyPrefab.AddComponent<LampTargetTracker>();

            DarknessController = PrefabAPI.InstantiateClone(new("Darkness"), "DarknessController");
            DarknessController.AddComponent<DarknessController>();
            DarknessController.layer = LayerIndex.postProcess.intVal;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<ChildLocator>().FindChild("MuzzleLantern").AddComponent<BoostIntensityDuringDarkness>();

            LampAreaProjectile = Load<GameObject>("LampArea.prefab");
            LampAreaProjectile.FindComponent<MeshRenderer>("AreaIndicator").sharedMaterial = Paths.Material.matTeamAreaIndicatorIntersectionPlayer;
            LampAreaProjectile.FindComponent<Light>("Light").AddComponent<BoostIntensityDuringDarkness>();
            ContentAddition.AddProjectile(LampAreaProjectile);

            On.RoR2.HealthComponent.TakeDamageProcess += HandleDarkness;

            LampSeekerBolt = PrefabAPI.InstantiateClone(Paths.GameObject.ChildTrackingSparkBall, "LampSeekerBolt");
            GameObject LampSeekerGhost = PrefabAPI.InstantiateClone(Paths.GameObject.ChildTrackingSparkBallGhost, "LampSeekerBoltGhost");
            LampSeekerBolt.GetComponent<ProjectileController>().ghostPrefab = LampSeekerGhost;

            LampSeekerBolt.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 240;
            LampSeekerBolt.GetComponent<ProjectileSimple>().desiredForwardSpeed = 90;
            var explo = LampSeekerBolt.GetComponent<ProjectileImpactExplosion>();
            explo.impactEffect = Paths.GameObject.ChildTrackingSparkBallShootExplosion;
            explo.explosionEffect = Paths.GameObject.BoostedSearFireballProjectileExplosionVFX;
            explo.blastRadius = 2;

            LampSeekerGhost.FindComponent<TrailRenderer>("Trail").gameObject.SetActive(false);

            ContentAddition.AddProjectile(LampSeekerBolt);
        }

        private void HandleDarkness(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (IsDarknessActive) {
                if (damageInfo.HasModdedDamageType(LightDamage)) {
                    damageInfo.damage *= 1.5f;
                }
            }

            orig(self, damageInfo);
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_EGO_LAMP_NAME".Add("Lamp");

            "RL_EGO_LAMP_PASSIVE_NAME".Add("Watchful Eyes");
            "RL_EGO_LAMP_PASSIVE_DESC".Add("Looking at an enemy nullifies the negative effects of <style=cDeath>Darkness</style>.");

            "RL_EGO_LAMP_PRIMARY_NAME".Add("Eternally-Lit Lamp");
            "RL_EGO_LAMP_PRIMARY_DESC".Add("Focus a beam that for <style=cIsDamage>300% damage per second</style> that <style=cIsUtility>enchants</style> targets.");

            "RL_EGO_LAMP_SECONDARY_NAME".Add("Dazzle");
            "RL_EGO_LAMP_SECONDARY_DESC".Add("<style=cIsDamage>Stunning.</style> Flash a blinding light for <style=cIsDamage>400% damage</style>. If the target is <style=cIsUtility>enchanted</style>, deal <style=cIsDamage>2000% damage</style> and <style=cDeath>break the enchantment.</style>");

            "RL_EGO_LAMP_UTILITY_NAME".Add("Illuminate");
            "RL_EGO_LAMP_UTILITY_DESC".Add("Place a bright flame that burns targets for <style=cIsDamage>200% damage per second</style>. <style=cIsUtility>Distracts</style> targets in the <style=cDeath>darkness</style>.");

            "RL_EGO_LAMP_SPECIAL_NAME".Add("Spreading Darkness");
            "RL_EGO_LAMP_SPECIAL_DESC".Add("For the next <style=cIsUtility>15 seconds</style>, inflict <style=cDeath>Darkness</style> on <style=cDeath>ALL</style> characters. <style=cDeath>On-Hit</style> effects will fail to trigger in the dark, and enemies are more susceptible to <style=cIsUtility>light</style>.");
        }
    }

    public class BoostIntensityDuringDarkness : MonoBehaviour {
        public float intensityMultiplier = 100f;
        public float rangeMultiplier = 2f;
        private Light light;
        private bool lastDarknessState = false;
        private float stopwatch;
        private float origIntens;
        private float origRange;

        public void Start() {
            light = GetComponent<Light>();
            origIntens = light.intensity;
            origRange = light.range;
        }

        public void FixedUpdate() {
            if (lastDarknessState != DarknessController.DarknessActive) {
                stopwatch = 0f;
                lastDarknessState = DarknessController.DarknessActive;
            }

            stopwatch += Time.fixedDeltaTime;
            stopwatch = Mathf.Clamp01(stopwatch);

            switch (lastDarknessState) {
                case true:
                    light.intensity = Mathf.Lerp(origIntens, origIntens * intensityMultiplier, stopwatch);
                    light.range = Mathf.Lerp(origRange, origRange * rangeMultiplier, stopwatch);
                    break;
                case false:
                    light.intensity = Mathf.Lerp(origIntens * intensityMultiplier, origIntens, stopwatch);
                    light.range = Mathf.Lerp(origRange * rangeMultiplier, origRange, stopwatch);
                    break;
            }
        }
    }

    public class LampTargetTracker : HurtboxTracker {
        public override void Start()
        {
            base.targetingIndicatorPrefab = Load<GameObject>("LampBeamIndicator.prefab");
            base.maxSearchAngle = 45f;
            base.maxSearchDistance = 70f;
            base.targetType = TargetType.Enemy;
            base.userIndex = TeamIndex.Player;
            base.Start();
        }
    }

    public class DarknessController : MonoBehaviour {
        public PostProcessVolume vol;
        public const float windUpDuration = 2f;
        public const float totalDuration = 15f - windUpDuration;
        private float[] stopwatch = new float[3];
        public static bool DarknessActive = false;
        public Color32 ambience1;
        public Color32 ambience2;
        public Color32 ambience3;
        public Color32 ambience4;
        public void Start() {
            vol = base.gameObject.AddComponent<PostProcessVolume>();
            vol.isGlobal = true;
            vol.weight = 0f;
            vol.priority = float.MaxValue - 1;
            vol.sharedProfile = EGOLamp.ppDarkness;
            vol.profile = EGOLamp.ppDarkness;

            DarknessActive = true;

            ambience1 = RenderSettings.ambientSkyColor;
            ambience2 = RenderSettings.ambientEquatorColor;
            ambience3 = RenderSettings.ambientGroundColor;
            ambience4 = RenderSettings.ambientLight;
        }

        public void Update() {
            stopwatch[0] += Time.deltaTime;

            if (stopwatch[0] <= windUpDuration) {
                vol.weight = Mathf.Clamp01(stopwatch[0] * 0.5f);

                RenderSettings.ambientSkyColor = Color.Lerp(ambience1, Color.black, stopwatch[0] * 0.5f);
                RenderSettings.ambientEquatorColor = Color.Lerp(ambience2, Color.black, stopwatch[0] * 0.5f);
                RenderSettings.ambientGroundColor = Color.Lerp(ambience3, Color.black, stopwatch[0]);
                RenderSettings.ambientLight = Color.Lerp(ambience4, Color.black, stopwatch[0] * 0.5f);
            }

            if (stopwatch[0] >= windUpDuration) {
                stopwatch[1] += Time.deltaTime;

                if (stopwatch[1] >= totalDuration) {
                    stopwatch[2] += Time.deltaTime;

                    RenderSettings.ambientSkyColor = Color.Lerp(ambience1, Color.black, 1f - stopwatch[2] * 0.5f);
                    RenderSettings.ambientEquatorColor = Color.Lerp(ambience2, Color.black, 1f - stopwatch[2] * 0.5f);
                    RenderSettings.ambientGroundColor = Color.Lerp(ambience3, Color.black, 1f - stopwatch[2]);
                    RenderSettings.ambientLight = Color.Lerp(ambience4, Color.black, 1f - stopwatch[2] * 0.5f);

                    vol.weight = Mathf.Clamp01(1f - (stopwatch[2] * 0.5f));

                    if (stopwatch[2] >= windUpDuration) {
                        Destroy(this.gameObject);
                    }
                }
            }
        }

        public void OnDestroy() {
            DarknessActive = false;
        }
    }

    /*public class DarknessController : MonoBehaviour {
        public Color32 ambience1;
        public Color32 ambience2;
        public Color32 ambience3;
        public Color32 ambience4;
        public AmbientMode ambientMode;
        public float[] stopwatch = new float[4];
        public float duration = 15f;
        public float inOutTime = 1f;
        public float ambientIntensity;
        public float ambientIntensity2;
        public static bool DarknessActive = false;
        public Light[] lights;
        public float[] intensities;
        public PostProcessVolume[] pp;
        public float[] weights;
        public PostProcessProfile[] ppVols;
        public Material skybox;
        public MeshRenderer[] renderer;
        public bool[] snowStatus;
        public Color tint;
        public string activeScene;
        public void Start() {
            ambience1 = RenderSettings.ambientSkyColor;
            ambience2 = RenderSettings.ambientEquatorColor;
            ambience3 = RenderSettings.ambientGroundColor;
            ambience4 = RenderSettings.ambientLight;
            ambientMode = RenderSettings.ambientMode;
            ambientIntensity2 = RenderSettings.ambientIntensity;
            DarknessActive = true;
            skybox = RenderSettings.skybox;
            tint = skybox.GetColor("_Tint");

            activeScene = SceneManager.GetActiveScene().name;

            pp = FindObjectsOfType<PostProcessVolume>().Where(x => IsInShitAssStage() || x.isGlobal).ToArray();
            weights = new float[pp.Length];
            ppVols = new PostProcessProfile[pp.Length];

            for (int i = 0; i < pp.Length; i++) {
                weights[i] = pp[i].weight;
                if (!IsInShitAssStage()) pp[i].weight = 0f;
            }
            
            Shader shader = Paths.Shader.HGTriplanarTerrainBlend;
            renderer = FindObjectsOfType<MeshRenderer>().Where(x => x.material.shader == shader).ToArray();
            snowStatus = new bool[renderer.Length];

            for (int i = 0; i < renderer.Length; i++) {
                snowStatus[i] = renderer[i].material.IsKeywordEnabled("MICROFACET_SNOW");
                renderer[i].material.DisableKeyword("MICROFACET_SNOW");
            }

            lights = FindObjectsOfType<Light>().Where(x => !x.GetComponent<BoostIntensityDuringDarkness>()).ToArray();
            intensities = new float[lights.Length];

            for (int i = 0; i < lights.Length; i++) {
                intensities[i] = lights[i].intensity;

                if (lights[i].GetComponent<NGSS_Directional>()) {
                    lights[i].GetComponent<NGSS_Directional>().enabled = false;
                }
            }

            ApplySceneSpecificTweaks();
        }

        public bool IsInShitAssStage() {
            return activeScene == Scenes.SirensCall || activeScene == Scenes.AbyssalDepths || activeScene == Scenes.WetlandAspect;
        }

        public void ApplySceneSpecificTweaks() {
            if (activeScene == Scenes.RallypointDelta) {
                for (int i = 0; i < lights.Length; i++) {
                    if (lights[i].GetComponent<NGSS_Directional>()) {
                        lights[i].gameObject.SetActive(false);
                    }
                }
            }

            if (IsInShitAssStage()) {
                PostProcessProfile ppProf = "RoR2/Base/title/PostProcessing/ppLocalTPActivation.asset".Load<PostProcessProfile>();

                for (int i = 0; i < pp.Length; i++) {
                    // weights[i] = pp[i].weight;
                    ppVols[i] = pp[i].profile;
                    pp[i].profile = ppProf;
                }
            }
        }

        public void UndoSceneSpecificTweaks() {
            if (activeScene == Scenes.RallypointDelta) {
                for (int i = 0; i < lights.Length; i++) {
                    if (lights[i].GetComponent<NGSS_Directional>()) {
                        lights[i].gameObject.SetActive(true);
                    }
                }
            }

            if (IsInShitAssStage()) {
                for (int i = 0; i < pp.Length; i++) {
                    //weights[i] = pp[i].weight;
                    pp[i].profile = ppVols[i];
                }
            }
        }

        public void FixedUpdate() {
            if (stopwatch[2] <= inOutTime) {
                stopwatch[2] += Time.fixedDeltaTime;
                
                if (!IsInShitAssStage()) {
                    RenderSettings.ambientSkyColor = Color.Lerp(ambience1, Color.black, stopwatch[2]);
                    RenderSettings.ambientEquatorColor = Color.Lerp(ambience2, Color.black, stopwatch[2]);
                    RenderSettings.ambientGroundColor = Color.Lerp(ambience3, Color.black, stopwatch[2]);
                    RenderSettings.ambientLight = Color.Lerp(ambience4, Color.black, stopwatch[2]);
                    RenderSettings.ambientIntensity = Mathf.Lerp(ambientIntensity2, 20f, stopwatch[2]);
                }

                skybox.SetColor("_Tint", Color.Lerp(tint, Color.black, stopwatch[2]));

                if (!IsInShitAssStage()) {
                    for (int i = 0; i < pp.Length; i++) {
                        pp[i].weight = 0f;
                    }
                }

                for (int i = 0; i < lights.Length; i++) {
                    if (!lights[i]) continue;
                    lights[i].intensity = Mathf.Lerp(intensities[i], 0f, stopwatch[2]);
                }
                
            }

            if (stopwatch[3] > 0f) {
                stopwatch[3] -= Time.fixedDeltaTime;
                
                if (activeScene != Scenes.SirensCall) {
                    RenderSettings.ambientSkyColor = Color.Lerp(Color.black, ambience1, 1f - stopwatch[3]);
                    RenderSettings.ambientEquatorColor = Color.Lerp(Color.black, ambience2, 1f - stopwatch[3]);
                    RenderSettings.ambientGroundColor = Color.Lerp(Color.black, ambience3, 1f - stopwatch[3]);
                    RenderSettings.ambientLight = Color.Lerp(Color.black, ambience4, 1f - stopwatch[3]);
                    RenderSettings.ambientIntensity = Mathf.Lerp(20f, ambientIntensity2, 1f - stopwatch[3]);
                }

                for (int i = 0; i < lights.Length; i++) {
                    if (!lights[i]) continue;
                    lights[i].intensity = Mathf.Lerp(0f, intensities[i], 1f - stopwatch[3]);
                }

                skybox.SetColor("_Tint", Color.Lerp(Color.black, tint, 1f - stopwatch[3]));

                if (!IsInShitAssStage()) {
                    for (int i = 0; i < pp.Length; i++) {
                        pp[i].weight = weights[i];
                    }
                }

                for (int i = 0; i < renderer.Length; i++) {
                    if (snowStatus[i]) {
                        renderer[i].material.EnableKeyword("MICROFACET_SNOW");
                    }
                }

                if (stopwatch[3] <= 0f) {
                    Destroy(base.gameObject);
                    return;
                }
            }

            if (stopwatch[2] >= inOutTime && stopwatch[3] <= 0f) {
                stopwatch[1] += Time.fixedDeltaTime;

                if (stopwatch[1] >= duration && stopwatch[3] <= 0f) {
                    stopwatch[3] = inOutTime;
                }
            }
        }

        public void OnDestroy() {
            DarknessActive = false;
            RenderSettings.ambientMode = ambientMode;

            UndoSceneSpecificTweaks();

            for (int i = 0; i < lights.Length; i++) {
                if (!lights[i]) continue;

                if (lights[i].GetComponent<NGSS_Directional>()) {
                    lights[i].GetComponent<NGSS_Directional>().enabled = true;
                }
            }

            for (int i = 0; i < renderer.Length; i++) {
                if (snowStatus[i]) {
                    renderer[i].material.EnableKeyword("MICROFACET_SNOW");
                }
            }
        }
    }*/
}