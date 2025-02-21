using System;
using System.Collections;
using System.Linq;
using RoR2.CharacterAI;
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

        public override GameObject BodyPrefab => Load<GameObject>("EGOLampBody.prefab");

        public override GameObject MasterPrefab => null;
        //
        public static bool IsDarknessActive => RaindropLobotomy.EGO.Mage.DarknessController.DarknessActive;
        public static GameObject LampSeekerBolt;
        public static GameObject DarknessController;
        public static GameObject LampAreaProjectile;
        public static GameObject LampAreaPassive;
        public static DamageAPI.ModdedDamageType LightDamage = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType Enchanting = DamageAPI.ReserveDamageType();
        public static PostProcessProfile ppDarkness;
        public static BuffDef bdDarknessImmune;
        public static GameObject IlluminationEffect;
        public static SkillDef sdEverlastingDark;
        public static SkillDef sdDazzle;
        public static RampFog fog;
        public static Mage.DarknessController DC => Mage.DarknessController.instance;

        public override void Create()
        {
            base.Create();

            PostProcessProfile ppProfile = ScriptableObject.CreateInstance<PostProcessProfile>();
           //  UnityEngine.Object.DontDestroyOnLoad(ppProfile);
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
            
            GameObject guh = GameObject.Instantiate(DarknessController);
            GameObject.DontDestroyOnLoad(guh);

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<ChildLocator>().FindChild("MuzzleLantern").AddComponent<BoostIntensityDuringDarkness>();

            LampAreaProjectile = Load<GameObject>("LampArea.prefab");
            LampAreaProjectile.FindComponent<MeshRenderer>("AreaIndicator").sharedMaterial = Paths.Material.matTeamAreaIndicatorIntersectionPlayer;
            LampAreaProjectile.FindComponent<Light>("Light").AddComponent<BoostIntensityDuringDarkness>();
            LampAreaProjectile.AddComponent<ForceEnchantedToTargetObject>();
            LampAreaProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(LightDamage);
            ContentAddition.AddProjectile(LampAreaProjectile);

            LampAreaPassive = Load<GameObject>("LampAreaPassive.prefab");
            LampAreaPassive.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(LightDamage);
            LampAreaPassive.FindComponent<Light>("Light").AddComponent<BoostIntensityDuringDarkness>();
            ContentAddition.AddProjectile(LampAreaPassive);

            On.RoR2.HealthComponent.TakeDamageProcess += HandleDarkness;

            LampSeekerBolt = PrefabAPI.InstantiateClone(Paths.GameObject.ChildTrackingSparkBall, "LampSeekerBolt");
            GameObject LampSeekerGhost = PrefabAPI.InstantiateClone(Paths.GameObject.MinorConstructProjectileGhost, "LampSeekerBoltGhost");
            LampSeekerBolt.GetComponent<ProjectileController>().ghostPrefab = LampSeekerGhost;

            LampSeekerBolt.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 360;
            LampSeekerBolt.RemoveComponent<ProjectileSphereTargetFinder>();
            LampSeekerBolt.RemoveComponent<ProjectileDirectionalTargetFinder>();
            LampSeekerBolt.GetComponent<ProjectileSimple>().desiredForwardSpeed = 40;
            LampSeekerBolt.GetComponent<ProjectileDamage>().damageType = DamageTypeCombo.GenericPrimary;
            LampSeekerBolt.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Enchanting);
            LampSeekerBolt.GetComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(LightDamage);
            var explo = LampSeekerBolt.GetComponent<ProjectileImpactExplosion>();
            explo.impactEffect = Paths.GameObject.ChildTrackingSparkBallShootExplosion;
            explo.explosionEffect = Paths.GameObject.BoostedSearFireballProjectileExplosionVFX;
            explo.blastRadius = 2;
            explo.falloffModel = BlastAttack.FalloffModel.None;

            SceneManager.activeSceneChanged += OnSceneChange;

            // LampSeekerGhost.FindComponent<TrailRenderer>("Trail").gameObject.SetActive(false);

            ContentAddition.AddProjectile(LampSeekerBolt);

            ContentAddition.AddBody(LampAreaProjectile);

            sdDazzle = Load<SkillDef>("sdDazzle.asset");
            sdEverlastingDark = Load<SkillDef>("sdPermaDark.asset");

            On.RoR2.Skills.SkillDef.IsReady += DisallowWhenNoTarget;
            On.RoR2.CharacterBody.Start += OnStart;
            On.RoR2.GlobalEventManager.ProcessHitEnemy += OnHitEnemy;
            On.RoR2.GlobalEventManager.OnHitAllProcess += OnHitAll;
            On.RoR2.HealthComponent.TakeDamage += TakeDamage;
            On.RoR2.GlobalEventManager.OnCharacterDeath += OnDeath;

            bdDarknessImmune = Load<BuffDef>("bdDarknessImmune.asset");
            ContentAddition.AddBuffDef(bdDarknessImmune);

            IlluminationEffect = Load<GameObject>("LampTargetLight.prefab");
            IlluminationEffect.AddComponent<BoostIntensityDuringDarkness>();

            BodyPrefab.AddComponent<CastLight>();
        }

        private void OnSceneChange(Scene arg0, Scene arg1)
        {
            DC.End();
        }

        private void OnDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);

            DC.Refresh();
        }

        private void TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (IsDarknessActive && !self.body.HasBuff(bdDarknessImmune)) {
                damageInfo.procCoefficient = 0;
            }

            orig(self, damageInfo);
        }

        private void OnHitAll(On.RoR2.GlobalEventManager.orig_OnHitAllProcess orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            if (IsDarknessActive && hitObject) {
                CharacterBody body = hitObject.GetComponent<CharacterBody>();

                if (!body || !body.HasBuff(bdDarknessImmune)) {
                    damageInfo.procCoefficient = 0;
                }
            }

            orig(self, damageInfo, hitObject);
        }

        private void OnHitEnemy(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            if (IsDarknessActive && hitObject) {
                CharacterBody body = hitObject.GetComponent<CharacterBody>();

                if (!body || !body.HasBuff(bdDarknessImmune)) {
                    damageInfo.procCoefficient = 0;
                }
            }

            orig(self, damageInfo, hitObject);
        }

        private void OnStart(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);

            if (self.HasSkillEquipped(sdEverlastingDark)) {
                DC.AddPermanentProvider(self);
            }
        }

        private bool DisallowWhenNoTarget(On.RoR2.Skills.SkillDef.orig_IsReady orig, SkillDef self, GenericSkill skillSlot)
        {
            if (self == sdDazzle) {
                if (!skillSlot.GetComponent<LampTargetTracker>().target) {
                    return false;
                }
            }

            return orig(self, skillSlot);
        }

        public class CastLight : MonoBehaviour {
            public Transform lightInstance;
            public CharacterBody body;
            public InputBankTest bank;

            public void Start() {
                body = GetComponent<CharacterBody>();
                bank = GetComponent<InputBankTest>();

                lightInstance = GameObject.Instantiate(EGOLamp.IlluminationEffect).transform;
            }

            public void Update() {
                lightInstance.gameObject.SetActive(IsDarknessActive);

                if (!IsDarknessActive) return;

                if (!bank) {
                    lightInstance.gameObject.SetActive(false);
                    return;
                }

                Vector3 pos = bank.GetAimRay().GetPoint(400);

                if (Util.CharacterRaycast(body.gameObject, bank.GetAimRay(), out RaycastHit info, 600f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Ignore)) {
                    pos = info.point + (info.normal * 0.5f);
                }

                lightInstance.transform.position = pos;
            }
        }

        public class ForceEnchantedToTargetObject : MonoBehaviour {
            public float stopwatch = 0f;
            public float delay = 0.5f;
            public void FixedUpdate() {
                if (!NetworkServer.active) {
                    return;
                }

                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= delay)
                {
                    stopwatch = 0f;
                    if (NetworkServer.active)
                    {
                        List<HurtBox> buffer = new();
                        SphereSearch search = new()
                        {
                            radius = 50f,
                            origin = base.transform.position,
                            mask = LayerIndex.entityPrecise.mask
                        };
                        
                        search.RefreshCandidates();
                        search.FilterCandidatesByHurtBoxTeam(TeamMask.AllExcept(TeamIndex.Player));
                        search.FilterCandidatesByDistinctHurtBoxEntities();
                        search.OrderCandidatesByDistance();
                        search.GetHurtBoxes(buffer);
                        search.ClearCandidates();

                        foreach (HurtBox box in buffer)
                        {
                            if (box.healthComponent && box.healthComponent.body.HasBuff(Buffs.Enchanted.BuffIndex) && box.healthComponent.body.master)
                            {
                                foreach (BaseAI ai in box.healthComponent.body.master.aiComponents)
                                {
                                    ai.enemyAttention = 0f;
                                    ai.currentEnemy.Reset();
                                    ai.currentEnemy.gameObject = base.gameObject;
                                    ai.currentEnemy.Update();
                                }
                            }
                        }
                    }
                }
            }
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
            "RL_EGO_LAMP_PRIMARY_DESC".Add("Fire a <style=cIsUtility>seeking bolt</style> for <style=cIsDamage>240% damage</style> that <style=cIsDamage>enchants</style> targets.");

            "RL_EGO_LAMP_SECONDARY_NAME".Add("Dazzle");
            "RL_EGO_LAMP_SECONDARY_DESC".Add("<style=cIsDamage>Stunning.</style> Flash a blinding light for <style=cIsDamage>400% damage</style>. If the target is <style=cIsUtility>enchanted</style>, deal <style=cIsDamage>4000% damage</style> and <style=cDeath>break the enchantment.</style>");

            "RL_EGO_LAMP_UTILITY_NAME".Add("Illuminate");
            "RL_EGO_LAMP_UTILITY_DESC".Add("Place a bright flame that burns targets for <style=cIsDamage>200% damage per second</style>. <style=cIsUtility>Distracts</style> targets who are <style=cIsDamage>enchanted</style>.");

            "RL_EGO_LAMP_SPECIAL_NAME".Add("Spreading Darkness");
            "RL_EGO_LAMP_SPECIAL_DESC".Add("For the next <style=cIsUtility>15 seconds</style>, inflict <style=cDeath>Darkness</style> on <style=cDeath>ALL</style> characters. <style=cDeath>On-Hit</style> effects will fail to trigger in the dark, and enemies are more susceptible to <style=cIsUtility>light</style>.");

            "RL_EGO_LAMP_SPECIAL_ALT_NAME".Add("Everlasting Darkness");
            "RL_EGO_LAMP_SPECIAL_ALT_DESC".Add("<style=cDeath>Passively bring permanent Darkness.</style> Gain a temporary <style=cIsUtility>flame ring</style> that burns enemies for <style=cIsDamage>300% damage per second</style>.");

            "KEYWORD_ENCHANTED".Add(
                "<style=cKeywordName>Enchanting</style>Targets are <style=cIsUtility>enchanted</style> upon receiving <style=cIsDamage>6 stacks</style>. Requirement is <style=cDeath>doubled</style> against bosses."
            );

            "KEYWORD_DARKNESS".Add(
                "<style=cKeywordName>Darkness</style>Increases light damage by <style=cIsDamage>1.5x</style> and <style=cDeath>prevents on-hit effects for all characters</style>."
            );
        }
    }

    public class BoostIntensityDuringDarkness : MonoBehaviour {
        public float intensityMultiplier = 70f;
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
        public const float windUpDuration = 1f;
        public const float totalDuration = 15f - windUpDuration;
        private float[] stopwatch = new float[3];
        public static bool DarknessActive = false;
        public Color32 ambience1;
        public Color32 ambience2;
        public Color32 ambience3;
        public Color32 ambience4;
        public bool permanent = false;
        public static List<CharacterBody> bodies = new();
        public static DarknessController instance;
        public void Start() {
            instance = this;

            vol = base.gameObject.AddComponent<PostProcessVolume>();
            vol.isGlobal = true;
            vol.weight = 0f;
            vol.priority = float.MaxValue - 1;
            vol.profile = EGOLamp.ppDarkness;

            DarknessActive = false;

            GameObject.DontDestroyOnLoad(this.gameObject);

            End();
        }

        public void End() {
            if (DarknessActive) {
                RenderSettings.ambientSkyColor = ambience1;
                RenderSettings.ambientEquatorColor = ambience2;
                RenderSettings.ambientGroundColor = ambience3;
                RenderSettings.ambientLight = ambience4;

                DarknessActive = false;
            }
            else {
                ambience1 = RenderSettings.ambientSkyColor;
                ambience2 = RenderSettings.ambientEquatorColor;
                ambience3 = RenderSettings.ambientGroundColor;
                ambience4 = RenderSettings.ambientLight;
            }
            
            this.enabled = false;
            vol.weight = 0;
            
            Refresh();
        }

        public void AddPermanentProvider(CharacterBody prov) {
            bodies.Add(prov);
            permanent = true;
            if (!DarknessActive) {
                TriggerDarkness();
            }
            this.enabled = true;
        }

        public void Refresh() {
            bodies.RemoveAll(x => x == null || !x.healthComponent || !x.healthComponent.alive);
            permanent = bodies.Count > 0;
        }

        public IEnumerator CallEndAfterDelay() {
            yield return new WaitForSeconds(0.025f);
            End();
        }

        public void TriggerDarkness() {
            this.enabled = true;
            

            stopwatch = new float[] { 0f, 0f, 0f };

            DarknessActive = true;
        }

        public void Update() {
            stopwatch[0] += Time.deltaTime;

            if (stopwatch[0] <= windUpDuration) {
                vol.weight = Mathf.Clamp01(stopwatch[0]);

                RenderSettings.ambientSkyColor = Color.Lerp(ambience1, Color.black, stopwatch[0]);
                RenderSettings.ambientEquatorColor = Color.Lerp(ambience2, Color.black, stopwatch[0]);
                RenderSettings.ambientGroundColor = Color.Lerp(ambience3, Color.black, stopwatch[0]);
                RenderSettings.ambientLight = Color.Lerp(ambience4, Color.black, stopwatch[0]);
            }

            if (stopwatch[0] >= windUpDuration) {
                stopwatch[1] += Time.deltaTime;

                DarknessActive = true;

                if (stopwatch[1] >= totalDuration && !permanent) {
                    stopwatch[2] += Time.deltaTime;

                    RenderSettings.ambientSkyColor = Color.Lerp(ambience1, Color.black, 1f - stopwatch[2]);
                    RenderSettings.ambientEquatorColor = Color.Lerp(ambience2, Color.black, 1f - stopwatch[2]);
                    RenderSettings.ambientGroundColor = Color.Lerp(ambience3, Color.black, 1f - stopwatch[2]);
                    RenderSettings.ambientLight = Color.Lerp(ambience4, Color.black, 1f - stopwatch[2]);

                    vol.weight = Mathf.Clamp01(1f - (stopwatch[2]));

                    if (stopwatch[2] >= windUpDuration) {
                        vol.weight = 0f;
                        
                        End();
                    }
                }
            }
        }

        public void OnDisable() {
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