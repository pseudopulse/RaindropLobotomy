using System;
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy.EGO.Merc {
    public class IndexMerc : CorrosionBase<IndexMerc>
    {
        public override string EGODisplayName => "Index Messenger Mercenary";

        public override string Description => "guh?";

        public override SurvivorDef TargetSurvivorDef => Assets.SurvivorDef.Merc;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => new Color32(159, 166, 100, 255);

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdIndexMerc.asset");

        public override GameObject BodyPrefab => Load<GameObject>("IndexMercBody.prefab");

        public override GameObject MasterPrefab => null;

        public override string RequiresAbsentMod => "com.rob.Paladin"; // if paladin is present, index variant goes to him instead
        public static LazyIndex IndexMercBody = new("IndexMercBody");
        public static LazyIndex IndexPaladinBody = new("IndexPaladinBody");
        public static GameObject GiantFistBody;
        public static GameObject GiantFistMaster;
        public static GameObject LockingBolt;
        public static SkillDef Attack;
        public static SkillDef Defense;
        public static SkillDef Evade;
        public static SkillDef Energize;
        public static DamageAPI.ModdedDamageType LockType = DamageAPI.ReserveDamageType();
        public static bool HasRunSharedSetup = false;

        public override void Modify()
        {
            base.Modify();
            
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Assets.RuntimeAnimatorController.animMerc;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Assets.ItemDisplayRuleSet.idrsMerc;
            // Load<GameObject>("IndexMercDisplay.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Assets.RuntimeAnimatorController.animMercDisplay;
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Assets.GameObject.MercBody.GetComponent<CharacterBody>().defaultCrosshairPrefab;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Assets.GameObject.GenericFootstepDust;
            EntityStateMachine.FindByCustomName(BodyPrefab, "Body").mainStateType = new(typeof(IndexMercMain));
            BodyPrefab.AddComponent<IndexPrescriptTargeter>();
            // BodyPrefab.AddComponent<IndexLockTargeter>();
            
            SharedSetup();
        }

        public static void SharedSetup() {
            if (HasRunSharedSetup) {
                return;
            }

            HasRunSharedSetup = true;

            GameObject sword = Load<GameObject>("CrashingSwordProjectile.prefab");
            sword.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Erosion.InflictTenErosion);
            sword.AddComponent<DistortedBladeProjectile>();
            ContentAddition.AddProjectile(sword);

            GiantFistBody = Load<GameObject>("GiantFistBody.prefab");
            GiantFistBody.AddComponent<GiantFistBehaviour>();
            GiantFistMaster = Load<GameObject>("GiantFistMaster.prefab");

            LanguageAPI.Add("RL_GIANTFIST_NAME", "Giant Fist");

            ContentAddition.AddBody(GiantFistBody);
            ContentAddition.AddMaster(GiantFistMaster);

            On.RoR2.CharacterBody.Start += OnBodyStart;

            LockingBolt = Load<GameObject>("LockingBolt.prefab");
            LockingBolt.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(LockType);
            LockingBolt.GetComponent<ProjectileImpactExplosion>().explosionEffect = Assets.GameObject.ExplosionLunarSun;
            ContentAddition.AddProjectile(LockingBolt);

            "RL_INDEXMERC_PASSIVE_NAME".Add("Ominous Power");
            "RL_INDEXMERC_PASSIVE_DESC".Add("You are accompanied by two <style=cIsDamage>Giant Fist</style> minions. <style=cIsDamage>Giant Fists</style> deal <style=cIsDamage>+50% damage</style> to <style=cIsDamage>Erroded</style> targets, and <style=cIsUtility>receive double the effect from prescripts</style>.");

            "RL_INDEXMERC_PRIMARY_NAME".Add("Blind Faith");
            "RL_INDEXMERC_PRIMARY_DESC".Add("<style=cIsDamage>Eroding.</style> Slice forward for <style=cIsDamage>210% damage</style>. Every third hit strikes in a greater area and inflicts <style=cIsUtility>double</style> <style=cIsDamage>Erosion</style>.");

            "RL_INDEXMERC_SECONDARY_NAME".Add("Lock");
            "RL_INDEXMERC_SECONDARY_DESC".Add("Lock a target for <style=cIsDamage>240% damage</style>. If the target is attacking, <style=cDeath>interrupt</style> them and deal <style=cIsDamage>800% damage</style> instead.");

            "RL_INDEXMERC_UTILITY_NAME".Add("Distorted Blade");
            "RL_INDEXMERC_UTILITY_DESC".Add("<style=cIsDamage>Eroding.</style> Slam your blade down from the sky, dealing <style=cIsDamage>1400%</style> damage in targeted area area and inflicting <style=cIsUtility>10</style> <style=cIsDamage>Erosion</style>.");

            "RL_INDEXMERC_SPECIAL_NAME".Add("Deliver Prescript");
            "RL_INDEXMERC_SPECIAL_DESC".Add("Select 1 of 4 <style=cIsUtility>prescripts</style> to grant to yourself or an ally.");

            "RL_KEYWORD_EROSION".Add(
                """
                <style=cKeywordName>Eroding</style> Targets with <style=cIsDamage>Erosion</style> take damage equal to <style=cIsDamage>20% damage</style> per stack of <style=cIsDamage>Erosion</style> when struck, and then lose <style=cIsUtility>1</style> <style=cIsDamage>Erosion</style>.
                """
            );

            Attack = Load<SkillDef>("PrescriptAttack.asset");
            Defense = Load<SkillDef>("PrescriptDefense.asset");
            Evade = Load<SkillDef>("PrescriptEvade.asset");
            Energize = Load<SkillDef>("PrescriptEnergize.asset");

            string PrescriptAttack_Name = "Prescript: Attack";
            string PrescriptAttack_Desc = "Gain <style=cIsDamage>+25% damage</style>, <style=cIsDamage>+25% attack speed</style>, and <style=cIsUtility>chance-based items activate more</style>. <style=cDeath>Lasts 7.5 seconds</style>.";
            string PrescriptDefense_Name = "Prescript: Defense";
            string PrescriptDefense_Desc = "Gain <style=cIsDamage>35 armor</style>, <style=cIsHealing>+150% health regeneration</style>, and <style=cIsDamage>20% maximum health</style> as <style=cIsUtility>shield</style>. Reflect <style=cIsDamage>200%</style> of damage taken to attackers. <style=cDeath>Lasts 10 seconds</style>.";
            string PrescriptEvasion_Name = "Prescript: Evasion";
            string PrescriptEvasion_Desc = "Gain <style=cIsUtility>+50% movement speed</style>, <style=cIsUtility>+200% acceleration</style>, and <style=cIsHealing>2 extra jumps</style>. <style=cDeath>Lasts 7.5 seconds</style>.";
            string PrescriptEnergize_Name = "Prescript: Recharge";
            string PrescriptEnergize_Desc = "Reduce <style=cIsDamage>skill cooldowns</style> by <style=cIsUtility>35%</style>, and gain <style=cIsDamage>+1 skill charges</style>. <style=cIsUtility>Refresh all cooldowns</style>. <style=cDeath>Lasts 10 seconds</style>.";

            "RL_PRESCRIPT_ATTACK_NAME".Add(PrescriptAttack_Name);
            "RL_PRESCRIPT_ATTACK_DESC".Add(PrescriptAttack_Desc);

            "RL_PRESCRIPT_DEFENSE_NAME".Add(PrescriptDefense_Name);
            "RL_PRESCRIPT_DEFENSE_DESC".Add(PrescriptDefense_Desc);

            "RL_PRESCRIPT_EVASION_NAME".Add(PrescriptEvasion_Name);
            "RL_PRESCRIPT_EVASION_DESC".Add(PrescriptEvasion_Desc);

            "RL_PRESCRIPT_ENERGIZE_NAME".Add(PrescriptEnergize_Name);
            "RL_PRESCRIPT_ENERGIZE_DESC".Add(PrescriptEnergize_Desc);

            "RL_KEYWORD_PRESCRIPTS".Add(
               $"""
                <style=cKeywordName>{PrescriptAttack_Name}</style>{PrescriptAttack_Desc}

                <style=cKeywordName>{PrescriptDefense_Name}</style>{PrescriptDefense_Desc}

                <style=cKeywordName>{PrescriptEvasion_Name}</style>{PrescriptEvasion_Desc}

                <style=cKeywordName>{PrescriptEnergize_Name}</style>{PrescriptEnergize_Desc}
                """
            );

            On.RoR2.HealthComponent.TakeDamage += OnTakeDamage;
        }

        private static void OnTakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(LockType)) {
                bool attacking = self.body.GetIsAttacking();

                damageInfo.damage *= attacking ? 8f : 2.5f;

                if (attacking) {
                    SetStateOnHurt hurt = self.GetComponent<SetStateOnHurt>();

                    if (hurt) {
                        hurt.SetStun(2);
                    }
                }
            }

            orig(self, damageInfo);
        }

        public class IndexLockTargeter : HurtboxTracker {
            public override void Start()
            {
                base.targetingIndicatorPrefab = Assets.GameObject.HuntressTrackingIndicator;
                base.maxSearchAngle = 25f;
                base.maxSearchDistance = 60f;
                base.searchDelay = 0.1f;
                base.targetType = TargetType.Enemy;
                base.isActiveCallback = () => {
                    return body.skillLocator.secondary.stock > 0;
                };
                base.Start();
            }
        }

        public class IndexPrescriptTargeter : HurtboxTracker {
            public override void Start()
            {
                base.targetingIndicatorPrefab = Assets.GameObject.HuntressTrackingIndicator;
                base.maxSearchAngle = 25f;
                base.maxSearchDistance = 60f;
                base.searchDelay = 0.1f;
                base.targetType = TargetType.Friendly;
                base.isActiveCallback = () => {
                    return body.skillLocator.special.stock > 0;
                };
                base.Start();
            }
        }

        private static void OnBodyStart(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);

            if (self.bodyIndex == IndexMercBody || self.bodyIndex == IndexPaladinBody) {
                SpawnGiantFist(self);
                SpawnGiantFist(self);
            }
        }

        public static void SpawnGiantFist(CharacterBody owner, int iterations = 1) {
            Vector3[] positions = MiscUtils.GetSafePositionsWithinDistance(owner.transform.position, 45f * iterations);

            if (positions.Length == 1) {
                if (iterations > 10) {
                    return;
                }

                SpawnGiantFist(owner, iterations + 1);
                return;
            }

            Vector3 pos = positions.GetRandom(Run.instance.spawnRng);

            MasterSummon summon = new();
            summon.ignoreTeamMemberLimit = true;
            summon.masterPrefab = GiantFistMaster;
            summon.position = pos + (Vector3.upVector * 5f);
            summon.rotation = Quaternion.identity;
            summon.summonerBodyObject = owner.gameObject;
            summon.useAmbientLevel = false;
            summon.teamIndexOverride = TeamIndex.Player;
           
            summon.Perform();
        }

        public class DistortedBladeProjectile : MonoBehaviour {
            public float timer = 1f;
            public void FixedUpdate() {
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f && NetworkServer.active) {
                    GetComponent<ProjectileExplosion>().DetonateServer();
                    this.enabled = false;
                }
            }
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_INDEXMERC_NAME".Add("Index Messenger Mercenary");
        }
    }

    public class GiantFistBehaviour : MonoBehaviour {
        public Transform owner;
        public CharacterBody body;
        public CharacterMaster master;
        public bool areWeIntangible = false;
        public void Start() {
            body = GetComponent<CharacterBody>();
        }
        public void FixedUpdate() {
            if (!body.HasBuff(RoR2Content.Buffs.HiddenInvincibility)) {
                body.SetBuffCount(RoR2Content.Buffs.HiddenInvincibility.buffIndex, 1);
            }

            if (!owner && master) {
                owner = master.minionOwnership.ownerMaster?.GetBody()?.transform ?? null;
            }

            if (!master) {
                master = body.master;
            }
        }
    }
    
    public class IndexMercMain : GenericCharacterMain {
        public override void UpdateAnimationParameters()
        {
            base.characterAnimParamAvailability.isSprinting = false;
            base.UpdateAnimationParameters();
        }
    }
}