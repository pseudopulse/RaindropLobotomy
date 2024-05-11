using System;
using System.Linq;
using RaindropLobotomy.Buffs;

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

        // TODO:
        // - fix m2 interrupting itself [ DONE ]
        // - fix camera bug caused by improper positioning of collider [ DONE ]
        // - fix vfx being too large
        // - add g??OdB?yE [ DONE ]
        // - potentially make anims? [ NOT DOING ]
        // - make stolen skills inherit charge stats [ DONE ]
        // - fix not using the correct value to determine cd lmao [ DONE ]
        // - implement Imitation
        // - fix broken skills: [ DONE ]
        // -- Clay Templar Minigun

        public static List<SkillDef> HighPriority = new() {
            Assets.SkillDef.ImpBodyBlink,
            Assets.SkillDef.BisonBodyCharge,
            Assets.SkillDef.ImpBossBodyFireVoidspikes,
            Assets.SkillDef.VoidJailerChargeCapture,
            Assets.SkillDef.FireConstructBeam,
            Assets.SkillDef.RaidCrabMultiBeam,
            Assets.SkillDef.GrandParentChannelSun,
            Assets.SkillDef.BeetleGuardBodySunder,
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
            Assets.SkillDef.VoidBlinkUp
        };

        public static List<GameObject> DisallowedBodies = new() {
            Assets.GameObject.BeetleBody,
            Assets.GameObject.VoidInfestorBody,
            Assets.GameObject.GupBody,
            Assets.GameObject.GipBody,
            Assets.GameObject.GeepBody,
            Assets.GameObject.JellyfishBody
        };

        public static List<BodyIndex> BlacklistedBodyIndexes;

        public override void Modify()
        {
            base.Modify();

            RuntimeAnimatorController animController = Assets.GameObject.VoidSurvivorDisplay.GetComponentInChildren<Animator>().runtimeAnimatorController;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Assets.RuntimeAnimatorController.animVoidSurvivor;
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
        }

        private void FillDisallowedIndexes(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();

            BlacklistedBodyIndexes = new();

            foreach (GameObject body in DisallowedBodies) {
                BlacklistedBodyIndexes.Add(body.GetComponent<CharacterBody>().bodyIndex);
            }
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
            }

            orig(self, report);

            if (report.damageInfo.HasModdedDamageType(MistType)) {
                EffectManager.SpawnEffect(MistEffect, new EffectData {
                    origin = report.victimBody.corePosition,
                    scale = report.victimBody.radius
                }, true);

                GameObject.Destroy(report.victimBody.gameObject);
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
            "RL_EGO_MIMICRY_PRIMARY_DESC".Add("Fire a medium-range blast for <style=cIsDamage>275% damage</style>.");

            "RL_EGO_MIMICRY_SECONDARY_NAME".Add("C?lA?w");
            "RL_EGO_MIMICRY_SECONDARY_DESC".Add("Swipe forward, dealing <style=cIsDamage>500% damage</style> and <style=cIsHealing>healing 40% of damage dealt</style>.");

            "RL_EGO_MIMICRY_UTILITY_NAME".Add("M?iMiC??");
            "RL_EGO_MIMICRY_UTILITY_DESC".Add("Activate the effect of your <style=cIsUtility>current shell</style>, if you have one.");

            "RL_EGO_MIMICRY_SPECIAL_NAME".Add("W?eAr S??hElL");
            "RL_EGO_MIMICRY_SPECIAL_DESC".Add("Perform a devastating slash for <style=cIsDamage>1400% damage</style>. <style=cDeath>If this kills, wear the target's shell.</style>");

            "RL_EGO_MIMICRY_GOODBYE_NAME".Add("G?oOd??ByE?");
            "RL_EGO_MIMICRY_GOODBYE_DESC".Add("Leap forward and perform a devastating slash, dealing <style=cIsDamage>2200% damage</style>. <style=cDeath>If this kills, wear the target's shell.</style>");
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

            public void Start() {
                mimicSlot = GetComponent<SkillLocator>().utility;
                GetComponent<SkillLocator>().special.SetSkillOverride(base.gameObject, Goodbye, GenericSkill.SkillOverridePriority.Contextual);
            }

            public void UpdateShell(CharacterBody body) {
                Debug.Log("Wearing a shell!");

                foreach (BodyIndex index in BlacklistedBodyIndexes) {
                    if (body.bodyIndex == index) return;
                }

                List<GenericSkill> skills = body.GetComponents<GenericSkill>().ToList();
                skills = skills.OrderBy(x => x.skillDef.baseRechargeInterval).ToList();

                GenericSkill skill = skills.FirstOrDefault(x => HighPriority.Contains(x.skillDef));

                if (!skill) {
                    skill = skills.FirstOrDefault(x => x.skillDef.activationStateMachineName != "Body");
                    if (!skill) skill = skills.First();
                }

                TargetShell = body.bodyIndex;

                if (mimicSlot && mimicSlot.skillDef == MimicSkillDef) {
                    mimicSlot.skillDef.activationStateMachineName = "Weapon";
                    mimicSlot.skillDef.activationState = skill.skillDef.activationState;
                    mimicSlot.skillDef.beginSkillCooldownOnSkillEnd = true;
                    mimicSlot.skillDef.baseMaxStock = skill.skillDef.baseMaxStock;
                    mimicSlot.skillDef.stockToConsume = skill.skillDef.stockToConsume;
                    mimicSlot.skillDef.rechargeStock = skill.skillDef.rechargeStock;
                    mimicSlot.skillDef.interruptPriority = InterruptPriority.Any;
                    mimicSlot.skillDef.fullRestockOnAssign = true;
                    mimicSlot.skillDef.isCombatSkill = skill.skillDef.isCombatSkill;
                    mimicSlot.skillDef.resetCooldownTimerOnUse = skill.skillDef.resetCooldownTimerOnUse;
                    mimicSlot.skillDef.mustKeyPress = skill.skillDef.mustKeyPress;

                    float targetCD = skill.skillDef.baseRechargeInterval;
                    targetCD *= 0.65f;

                    Debug.Log("target's modified cd: " + targetCD);
                    
                    float newCD = Mathf.Min(targetCD, 5f);
                    Debug.Log("clamped cd: " + newCD);

                    mimicSlot.skillDef.baseRechargeInterval = newCD;
                    mimicSlot.RecalculateFinalRechargeInterval();
                }

                CharacterBody body2 = GetComponent<CharacterBody>();
                body2.SetBuffCount(Buffs.WornShell.Instance.Buff.buffIndex, 1);
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
                    return null;
                }

                return orig(self, "MuzzleHandBeam");
            };

            On.ChildLocator.FindChildIndex_string += (orig, self, str) => {
                int c = orig(self, str);
                if (c != -1) {
                    return c;
                }

                return orig(self, "MuzzleHandBeam");
            };

            On.EntityStates.EntityState.PlayAnimation_string_string += (orig, self, str, str2) => {
                if (self.characterBody && self.characterBody.bodyIndex == MimicryViendIndex) {
                    Animator anim = self.GetModelAnimator();
                    bool state = anim.HasState(anim.GetLayerIndex(str), Animator.StringToHash(str2));

                    Debug.Log("has state: " + state);

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

                    Debug.Log("has state: " + state);

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