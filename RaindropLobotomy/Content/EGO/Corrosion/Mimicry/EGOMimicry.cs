using System;
using System.Linq;
using RaindropLobotomy.Buffs;
using UnityEngine.SceneManagement;
using R2API.Networking.Interfaces;
using UnityEngine.Networking.Types;
using R2API.Networking;

namespace RaindropLobotomy.EGO.Viend {
    public class EGOMimicry : CorrosionBase<EGOMimicry>
    {
        public override string EGODisplayName => "Mimicry";

        public override string Description => "And the many shells cried out one word...";

        public override SurvivorDef TargetSurvivorDef => Assets.SurvivorDef.VoidSurvivor;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => new Color32(212, 0, 4, 255);

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdMimicry.asset");

        public override GameObject BodyPrefab => Load<GameObject>("MimicryBody.prefab");

        public override GameObject MasterPrefab => null;

        private static SkillDef MimicSkillDef;

        //

        public static Material matMimicrySlash;
        public static GameObject TracerHello;
        public static GameObject SlashEffect;
        //
        public static DamageAPI.ModdedDamageType WearShellType = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType ClawLifestealType = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType MistType = DamageAPI.ReserveDamageType();
        public static LazyIndex MimicryViendIndex = new LazyIndex("MimicryBody");
        public static SkillDef Goodbye;

        public static List<Type> SkillStates = new() {
            typeof(Hello),
            typeof(Claw),
            typeof(WearShell),
            typeof(GoodbyeSlash),
            typeof(GenericCharacterMain)
        };

        public static GameObject MistEffect;

        public static int[] ShellCounts = new int[6] {
            3, 5, 6, 6, 6, 3
        };

        public static List<SkillDef> HighPriority = new() {
            Assets.SkillDef.ImpBodyBlink,
            Assets.SkillDef.BisonBodyCharge,
            Assets.SkillDef.ImpBossBodyFireVoidspikes,
            Assets.SkillDef.VoidJailerChargeCapture,
            Assets.SkillDef.FireConstructBeam,
            Assets.SkillDef.RaidCrabMultiBeam,
            Assets.SkillDef.GrandParentChannelSun,
            Assets.SkillDef.HuntressBodyBlink,
            Assets.SkillDef.HuntressBodyMiniBlink,
            Assets.SkillDef.MageBodyWall,
            Assets.SkillDef.BanditBodyCloak,
            Assets.SkillDef.CaptainTazer,
            Assets.SkillDef.EngiBodyPlaceTurret,
            Assets.SkillDef.EngiBodyPlaceWalkerTurret,
            Assets.SkillDef.ThrowPylon,
            Assets.SkillDef.ThrowGrenade,
            Assets.SkillDef.MercBodyEvis,
            Assets.SkillDef.MercBodyEvisProjectile,
            Assets.SkillDef.RailgunnerBodyFireMineBlinding,
            Assets.SkillDef.RailgunnerBodyFireMineConcussive,
            Assets.SkillDef.VoidBlinkDown,
            Assets.SkillDef.VoidBlinkUp,
            Load<SkillDef>("SilentAdvance.asset"),
            Load<SkillDef>("Scream.asset"),
            Load<SkillDef>("SweeperUtility.asset"),
        };

        public static List<GameObject> DisallowedBodies = new() {
            Assets.GameObject.BeetleBody,
            Assets.GameObject.VoidInfestorBody,
            Assets.GameObject.GupBody,
            Assets.GameObject.GipBody,
            Assets.GameObject.GeepBody,
            Assets.GameObject.JellyfishBody,
            Assets.GameObject.BisonBody,
            Assets.GameObject.MagmaWormBody,
            Assets.GameObject.ElectricWormBody,
            Assets.GameObject.VerminBody,
            Assets.GameObject.BeetleGuardBody
        };

        public static SkillDef Fallback => Assets.SkillDef.CommandoSlide;

        public static List<BodyIndex> BlacklistedBodyIndexes;

        public class EGOMimicryConfig : ConfigClass
        {
            public override string Section => "EGO Corrosions :: Mimicry";
            public bool PlayGoodbyeAudio => base.Option<bool>("Goodbye Audio", "Play the Goodbye sound effect when using the Goodbye skill.", true);

            public override void Initialize()
            {
                _ = PlayGoodbyeAudio;
            }
        }

        public static EGOMimicryConfig config = new();

        public override void Modify()
        {
            base.Modify();

            RuntimeAnimatorController animController = Assets.GameObject.VoidSurvivorDisplay.GetComponentInChildren<Animator>().runtimeAnimatorController;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Assets.RuntimeAnimatorController.animVoidSurvivor;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Assets.ItemDisplayRuleSet.idrsVoidSurvivor;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<ChildLocator>().FindChild("ScytheScaleBone").AddComponent<GoodbyeArmStretcher>();
            Load<GameObject>("MimicryDisplay.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = animController;
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Assets.GameObject.VoidSurvivorBody.GetComponent<CharacterBody>().defaultCrosshairPrefab;
            BodyPrefab.AddComponent<MimicryShellController>();

            matMimicrySlash = Load<Material>("matMimicrySlash.mat");
            matMimicrySlash.SetTexture("_RemapTex", Assets.Texture2D.texRampInfusion);
            matMimicrySlash.SetTexture("_Cloud1Tex", Assets.Texture2D.texCloudCaustic3);
            matMimicrySlash.SetTexture("_Cloud2Tex", Assets.Texture2D.texCloudWaterFoam1);
            matMimicrySlash.SetTexture("_MainTex", Assets.Texture2D.texOmniHitspark2Mask);
            matMimicrySlash.SetShaderKeywords(new string[] { "USE_CLOUDS" });

            TracerHello = PrefabAPI.InstantiateClone(Assets.GameObject.TracerCommandoBoost, "TracerHello");
            TracerHello.GetComponent<LineRenderer>().material = Assets.Material.matLunarGolemChargeGlow;
            TracerHello.GetComponent<Tracer>().speed = 370;
            ContentAddition.AddEffect(TracerHello);

            SlashEffect = PrefabAPI.InstantiateClone(Assets.GameObject.VoidSurvivorMeleeSlash3, "MimicrySlash");
            SlashEffect.transform.Find("Rotator").Find("SwingTrail").GetComponent<ParticleSystemRenderer>().material = matMimicrySlash;
            ParticleSystem.MainModule main = SlashEffect.transform.Find("Rotator").Find("SwingTrail").GetComponent<ParticleSystem>().main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            ContentAddition.AddEffect(SlashEffect);

            MistEffect = Load<GameObject>("MistCloud.prefab");
            ContentAddition.AddEffect(MistEffect);

            On.RoR2.Skills.SkillDef.IsReady += DisallowMimicWhenNoShell;
            On.RoR2.GlobalEventManager.OnCharacterDeath += WearShellOnKill;
            On.RoR2.GlobalEventManager.OnHitEnemy += Heal;
            On.RoR2.BodyCatalog.Init += FillDisallowedIndexes;

            MimicSkillDef = Load<SkillDef>("Mimic.asset");

            Goodbye = Load<SkillDef>("Goodbye.asset");

            TransformHooks();

            NetworkingAPI.RegisterMessageType<SyncShell>();
        }

        private void FillDisallowedIndexes(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();

            BlacklistedBodyIndexes = new();

            foreach (GameObject body in DisallowedBodies) {
                BlacklistedBodyIndexes.Add(body.GetComponent<CharacterBody>().bodyIndex);
            }

            BlacklistedBodyIndexes.Add(new LazyIndex("BobombBody"));
            BlacklistedBodyIndexes.Add(new LazyIndex("BodyBrassMonolith"));
            BlacklistedBodyIndexes.Add(new LazyIndex("CoilGolemBody"));
            BlacklistedBodyIndexes.Add(new LazyIndex("FrostWispBody"));
            BlacklistedBodyIndexes.Add(new LazyIndex("BobombBody"));
            BlacklistedBodyIndexes.Add(new LazyIndex("RunshroomBody"));
            BlacklistedBodyIndexes.Add(new LazyIndex("SteamMachineBody"));
        }

        private void Heal(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.HasModdedDamageType(ClawLifestealType) && damageInfo.attacker) {
                HealthComponent attackerHealth = damageInfo.attacker.GetComponent<HealthComponent>();

                if (attackerHealth) attackerHealth.Heal(damageInfo.damage * 0.4f, new(), true);
            }
        }

        private void WearShellOnKill(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport report)
        {
            if (report.damageInfo.HasModdedDamageType(WearShellType) && report.attackerBody && report.attackerBody.GetComponent<MimicryShellController>()) {
                report.attackerBody.GetComponent<MimicryShellController>().UpdateShell(report.victimBody);
                new SyncShell(report.attacker, report.victimBody).Send(NetworkDestination.Clients);
            }

            orig(self, report);

            if (report.damageInfo.HasModdedDamageType(MistType)) {
                EffectManager.SpawnEffect(MistEffect, new EffectData {
                    origin = report.victimBody.corePosition,
                    scale = report.victimBody.bestFitRadius
                }, true);

                if (report.victimBody.modelLocator?.modelTransform) GameObject.Destroy(report.victimBody.modelLocator.modelTransform.gameObject);
            }
        }

        private bool DisallowMimicWhenNoShell(On.RoR2.Skills.SkillDef.orig_IsReady orig, SkillDef self, GenericSkill skillSlot)
        {   
            if (self == MimicSkillDef) {
                if (!skillSlot.GetComponent<CharacterBody>().HasBuff(WornShell.Instance.Buff)) {
                    return false;
                }
            }

            return orig(self, skillSlot);
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_EGO_MIMICRY_NAME".Add("Void Fiend :: Mimicry");


            "RL_EGO_MIMICRY_PASSIVE_NAME".Add("IMi?taTio??N");
            "RL_EGO_MIMICRY_PASSIVE_DESC".Add("Gain stacks of <style=cIsDamage>Imitation</style> as you acquire unique shells. After reaching enough <style=cIsDamage>Imitation</style>, replace your special with <style=cDeath>G?oOd??ByE?</style>");

            "RL_EGO_MIMICRY_PRIMARY_NAME".Add("H??eLlO?");
            "RL_EGO_MIMICRY_PRIMARY_DESC".Add("Fire a medium-range blast for <style=cIsDamage>325% damage</style>.");

            "RL_EGO_MIMICRY_SECONDARY_NAME".Add("C?lA?w");
            "RL_EGO_MIMICRY_SECONDARY_DESC".Add("Lunge and swipe forward, dealing <style=cIsDamage>500% damage</style> and <style=cIsHealing>healing 40% of damage dealt</style>. Hold up to 2 charges.");

            "RL_EGO_MIMICRY_UTILITY_NAME".Add("M?iMiC??");
            "RL_EGO_MIMICRY_UTILITY_DESC".Add("Activate the effect of your <style=cIsUtility>current shell</style>, if you have one.");

            "RL_EGO_MIMICRY_SPECIAL_NAME".Add("W?eAr S??hElL");
            "RL_EGO_MIMICRY_SPECIAL_DESC".Add("Perform a devastating slash for <style=cIsDamage>1400% damage</style>. <style=cDeath>If this kills, wear the target's shell.</style>");

            "RL_EGO_MIMICRY_GOODBYE_NAME".Add("G?oOd??ByE?");
            "RL_EGO_MIMICRY_GOODBYE_DESC".Add("Leap forward and perform a devastating slash, dealing <style=cIsDamage>2200% damage</style>. <style=cDeath>If this kills, wear the target's shell.</style>");
        }

        public class SyncShell : INetMessage
        {
            public CharacterBody target;
            public GameObject applyTo;
            private NetworkInstanceId _target;
            private NetworkInstanceId _applyTo;
            public void Deserialize(NetworkReader reader)
            {
                _applyTo = reader.ReadNetworkId();
                _target = reader.ReadNetworkId();

                applyTo = Util.FindNetworkObject(_applyTo);
                target = Util.FindNetworkObject(_target).GetComponent<CharacterBody>();
            }

            public void OnReceived()
            {
                applyTo.GetComponent<MimicryShellController>().UpdateShell(target);
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(applyTo.GetComponent<NetworkIdentity>().netId);
                writer.Write(target.GetComponent<NetworkIdentity>().netId);
            }

            public SyncShell() {

            }

            public SyncShell(GameObject applyTo, CharacterBody target) {
                this.applyTo = applyTo;
                this.target = target;
            }
        }

        public class GoodbyeArmStretcher : MonoBehaviour {
            public bool isStretching = false;
            public int windState = 0;
            public Vector3 targetScale = new(4f, 4f, 4f);
            public Vector3 originalScale;
            public float stopwatch = 0f;
            public float totalDuration = 0.2f;
            public float stretchDurationPerc = 0.5f;
            public float shrinkDurationPerc = 0.4f;
            public Vector3 lockedScale = Vector3.zero;

            public float shrinkDur => totalDuration * shrinkDurationPerc;
            public float stretchDur => totalDuration * stretchDurationPerc;

            public void BeginGoodbye(int windstate) {
                stopwatch = 0f;
                isStretching = true;
                windState = windstate;
                lockedScale = Vector3.zero;
            }

            public void LateUpdate() {
                if (isStretching) {
                    stopwatch += Time.fixedDeltaTime;

                    if (windState == 1) {
                        base.transform.localScale = Vector3.Lerp(targetScale, originalScale, (stopwatch / totalDuration));
                    }
                    else {
                        base.transform.localScale = Vector3.Lerp(originalScale, targetScale, (stopwatch / totalDuration));
                    }

                    if (stopwatch >= totalDuration) {
                        base.transform.localScale = windState == 1 ? originalScale : targetScale;
                        isStretching = false;
                        lockedScale = windState == 1 ? originalScale : targetScale;
                    }
                }

                if (lockedScale != Vector3.zero) {
                    base.transform.localScale = lockedScale;
                }
            }

            public void Start() {
                originalScale = transform.localScale;
            }
        }

        public class MimicryShellController : MonoBehaviour {
            public SkillDef WornShellSD;
            public BodyIndex TargetShell = BodyIndex.None;
            public GenericSkill mimicSlot;
            public List<BodyIndex> shellsWorn = new();
            private bool assignedMimicry = false;
            private CharacterBody us;
            private float shellCooldown = 0f;
            private float lastEnemyHp = 0f;

            public void Start() {
                mimicSlot = GetComponent<SkillLocator>().utility;
                us = GetComponent<CharacterBody>();
            }

            public void FixedUpdate() {
                if (shellCooldown >= 0f) {
                    shellCooldown -= Time.fixedDeltaTime;
                }

                if (shellCooldown <= 0f) {
                    lastEnemyHp = 0f;
                }
            }

            public void UpdateShell(CharacterBody body) {
                // Debug.Log("Wearing a shell!");

                bool useFallbackSkill = false;

                foreach (BodyIndex index in BlacklistedBodyIndexes) {
                    if (body.bodyIndex == index) useFallbackSkill = true;
                }

                bool onlyAllowHigherShell = shellCooldown >= 0f;

                if (onlyAllowHigherShell) {
                    if (body.maxHealth >= lastEnemyHp) {
                        lastEnemyHp = body.maxHealth;
                    }
                    else {
                        return;
                    }
                }
                
                if (shellCooldown <= 0f) {
                    shellCooldown = 2f;
                }

                if (!shellsWorn.Contains(body.bodyIndex)) {
                    shellsWorn.Add(body.bodyIndex);
                    
                    if (NetworkServer.active) GetComponent<CharacterBody>().AddBuff(Buffs.Imitation.Instance.Buff);

                    int index = 0;

                    if (SceneCatalog.mostRecentSceneDef == Assets.SceneDef.moon || SceneCatalog.mostRecentSceneDef == Assets.SceneDef.moon2) {
                        index = 5;
                    }
                    else {
                        index = Run.instance.stageClearCount + 1 % 5;
                        if (index == 0) index = 5;
                        index--;
                    }

                    // Debug.Log($"target shell count is {ShellCounts[index]} at index {index}");

                    if (shellsWorn.Count >= ShellCounts[index] && !assignedMimicry) {
                        if (us.hasAuthority) GetComponent<SkillLocator>().special.SetSkillOverride(base.gameObject, Goodbye, GenericSkill.SkillOverridePriority.Upgrade);
                        assignedMimicry = true;
                    }
                }

                SkillDef copySkill = EGOMimicry.Fallback;

                if (!useFallbackSkill) {
                    List<GenericSkill> skills = body.GetComponents<GenericSkill>().ToList();
                    skills = skills.OrderBy(x => x.skillDef.baseRechargeInterval).ToList();

                    GenericSkill skill = skills.FirstOrDefault(x => HighPriority.Contains(x.skillDef));

                    if (!skill) {
                        skill = skills.FirstOrDefault(x => x.skillDef.activationStateMachineName != "Body");
                        if (!skill) skill = skills.First();
                    }

                    copySkill = skill.skillDef;
                }

                TargetShell = body.bodyIndex;

                if (mimicSlot && mimicSlot.skillDef == MimicSkillDef) {
                    mimicSlot.skillDef.activationStateMachineName = "Mimic";
                    mimicSlot.skillDef.activationState = copySkill.activationState;
                    mimicSlot.skillDef.beginSkillCooldownOnSkillEnd = true;
                    mimicSlot.skillDef.baseMaxStock = copySkill.baseMaxStock;
                    mimicSlot.skillDef.stockToConsume = copySkill.stockToConsume;
                    mimicSlot.skillDef.rechargeStock = copySkill.rechargeStock;
                    mimicSlot.skillDef.interruptPriority = InterruptPriority.Any;
                    mimicSlot.skillDef.fullRestockOnAssign = true;
                    mimicSlot.skillDef.isCombatSkill = copySkill.isCombatSkill;
                    mimicSlot.skillDef.resetCooldownTimerOnUse = copySkill.resetCooldownTimerOnUse;
                    mimicSlot.skillDef.mustKeyPress = copySkill.mustKeyPress;
                    mimicSlot.skillDef.cancelSprintingOnActivation = copySkill.cancelSprintingOnActivation;

                    float targetCD = copySkill.baseRechargeInterval;
                    targetCD *= 0.65f;

                    // Debug.Log("target's modified cd: " + targetCD);
                    
                    float newCD = Mathf.Min(targetCD, 5f);
                    // Debug.Log("clamped cd: " + newCD);

                    mimicSlot.skillDef.baseRechargeInterval = newCD;
                    mimicSlot.RecalculateFinalRechargeInterval();
                }

                CharacterBody body2 = GetComponent<CharacterBody>();
                if (NetworkServer.active) body2.SetBuffCount(Buffs.WornShell.Instance.Buff.buffIndex, 1);
                Buffs.WornShell.Instance.Buff.buffColor = body.bodyColor;
            }
        }
        
        public void TransformHooks() {
            On.ChildLocator.FindChild_string += (orig, self, str) => {
                Transform transform = orig(self, str);
                if (transform) {
                    return transform;
                }

                if (str == "HealthBarOrigin") {
                    return orig(self, str);
                }

                Transform handBeam = orig(self, "MuzzleHandBeam");

                if (handBeam) return handBeam;
                return orig(self, str);
            };

            On.ChildLocator.FindChildIndex_string += (orig, self, str) => {
                int c = orig(self, str);
                if (c != -1) {
                    return c;
                }

                int handBeam = orig(self, "MuzzleHandBeam");

                if (handBeam != -1) return handBeam;
                return orig(self, str);
            };

            On.EntityStates.EntityState.PlayAnimation_string_string += (orig, self, str, str2) => {
                if (self.characterBody && self.characterBody.bodyIndex == MimicryViendIndex) {
                    Animator anim = self.GetModelAnimator();
                    bool state = anim.HasState(anim.GetLayerIndex(str), Animator.StringToHash(str2));

                    // Debug.Log("has state: " + state);

                    if (!state) {
                        orig(self, "LeftArm, Override", "FireHandBeam");
                        return;
                    }
                }

                orig(self, str, str2);
            };

            On.EntityStates.EntityState.PlayAnimation_string_string_string_float += (orig, self, str, str2, str3, f) => {
                if (self.characterBody && self.characterBody.bodyIndex == MimicryViendIndex) {
                    Animator anim = self.GetModelAnimator();
                    bool state = anim.HasState(anim.GetLayerIndex(str), Animator.StringToHash(str2));

                    // Debug.Log("has state: " + state);

                    if (!state) {
                        orig(self, "LeftArm, Override", "FireHandBeam", "HandBeam.playbackRate", f);
                        return;
                    }
                }

                orig(self, str, str2, str3, f);
            };

            On.EntityStates.BaseState.OnEnter += (orig, self) => {
                orig(self);

                if (self.characterBody && self.characterBody.bodyIndex == MimicryViendIndex) {
                    if (IsAStolenSkill(self)) {
                        self.damageStat *= 1.75f;
                        self.attackSpeedStat *= 1.75f;
                    }
                }
            };

            On.RoR2.EntityStateMachine.FixedUpdate += (orig, self) => {
                bool wasS1Down = false;
                bool stolen = self.state != null && IsAStolenSkill(self.state);
                bool didWeEvenRun = false;

                if (self.state != null && self.state.characterBody && self.state.characterBody.bodyIndex == MimicryViendIndex && stolen) {
                    wasS1Down = self.state.inputBank.skill1.down;
                    self.state.inputBank.skill1.down = self.state.inputBank.skill3.down;

                    didWeEvenRun = true;
                }

                orig(self);

                if (didWeEvenRun) {
                    self.state.inputBank.skill1.down = wasS1Down;
                }
            };
        }

        public bool IsAStolenSkill(EntityState state) {
            foreach (Type stateType in SkillStates) {
                if (state.GetType() == stateType) {
                    return false;
                }
            }

            return true;
        }
    }
}