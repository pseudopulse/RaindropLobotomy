using System;
using EntityStates.AffixVoid;

namespace RaindropLobotomy.EGO.FalseSon {
    public class EGOJustitia : CorrosionBase<EGOJustitia>
    {
        public override string EGODisplayName => "Justitia";

        public override string Description => "When I lift this scale, I wish for immortality upon the guilty.";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.FalseSon;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => new Color32(0, 196, 198, 255);

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdJustitia.asset");

        public override GameObject BodyPrefab => Load<GameObject>("JustitiaBody.prefab");

        public override GameObject MasterPrefab => null;
        public static GameObject AirSlashProjectile;
        public static GameObject AirSlashSinProjectile;
        public static GameObject JustitiaShriek;
        public static LazyIndex JustitiaBody = new("JustitiaBody");

        public override void Modify()
        {
            base.Modify();

            BodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpFalseSon;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Paths.ItemDisplayRuleSet.idrsFalseSon;
            Load<GameObject>("JustitiaDisplay.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animFalseSonDisplay;
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Paths.GameObject.MercBody.GetComponent<CharacterBody>().defaultCrosshairPrefab;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animFalseSon;

            SharedSetup();
        }

        public static void SharedSetup() {
            if (AirSlashProjectile) return;

            AirSlashProjectile = Load<GameObject>("JustitiaAirSlash.prefab");
            AirSlashProjectile.GetComponent<ProjectileOverlapAttack>().impactEffect = Paths.GameObject.OmniImpactVFXSlashMerc;
            AirSlashProjectile.GetComponent<ProjectileDamage>().damageType = DamageTypeCombo.GenericPrimary;
            ContentAddition.AddProjectile(AirSlashProjectile);

            AirSlashSinProjectile = PrefabAPI.InstantiateClone(AirSlashProjectile, "JustitiaSinAirSlash.prefab");
            AirSlashSinProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Buffs.Sin.SinType);
            ContentAddition.AddProjectile(AirSlashSinProjectile);

            JustitiaShriek = Load<GameObject>("JustitiaScream.prefab");

            On.RoR2.HealthComponent.TakeDamage += ImmuneToDOT;
            On.RoR2.DotController.OnDotStackAddedServer += DoubleDotDuration;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += DoubleDebuffs;
        }

        private static void DoubleDebuffs(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            if (HasTatteredBandages(self) && buffDef.isDebuff) {
                duration *= 2f;
            }

            orig(self, buffDef, duration);
        }

        private static void DoubleDotDuration(On.RoR2.DotController.orig_OnDotStackAddedServer orig, DotController self, object dotStack)
        {
            DotController.DotStack stack = dotStack as DotController.DotStack;

            if (self.victimBody && HasTatteredBandages(self.victimBody)) {
                stack.timer *= 2f;
            }

            orig(self, dotStack);
        }

        private static void ImmuneToDOT(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.damageType.damageType.HasFlag(DamageType.DoT) && HasTatteredBandages(self.body)) {
                damageInfo.rejected = true;
            }

            orig(self, damageInfo);
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_EGO_JUSTITIA_NAME".Add("Justitia");
            "RL_EGO_JUSTITIA_SUB".Add("Sworn Protector");

            "RL_EGO_JUSTITIA_PASSIVE_NAME".Add("Tattered Bandages");
            "RL_EGO_JUSTITIA_PASSIVE_DESC".Add("Receive <style=cIsDamage>no damage</style> from <style=cDeath>damaging debuffs</style>. All <style=cDeath>debuffs</style> last <style=cIsUtility>twice as long</style>.");

            "RL_EGO_JUSTITIA_PRIMARY_NAME".Add("Judgement");
            "RL_EGO_JUSTITIA_PRIMARY_DESC".Add("Slash forward for <style=cIsDamage>220% damage</style>, firing out a cutting wave. Inflicts <style=cIsUtility>Sin</style> while afflicted with any debuff.");

            "RL_EGO_JUSTITIA_SECONDARY_NAME".Add("Piercing Shriek");
            "RL_EGO_JUSTITIA_SECONDARY_DESC".Add("<style=cIsDamage>Stunning.</style> Scream out, dealing <style=cIsDamage>400% damage</style> and inflicting <style=cIsDamage>3</style> <style=cIsUtility>Sin</style>.");

            "RL_EGO_JUSTITIA_UTILITY_NAME".Add("Silence");
            "RL_EGO_JUSTITIA_UTILITY_DESC".Add("Shield yourself from attacks, <style=cIsUtility>reflecting</style> them back to inflict <style=cIsDamage>5</style> <style=cIsUtility>Sin</style>.");

            "RL_EGO_JUSTITIA_SPECIAL_NAME".Add("Ceaseless Judgement");
            "RL_EGO_JUSTITIA_SPECIAL_DESC".Add("<style=cDeath>25% HP.</style> Raise your scale, dealing <style=cIsDamage>500% damage</style> in an area and <style=cDeath>instantly killing</style> targets below <style=cDeath>50%</style> <style=cIsUtility>Sin</style>.");

            "RL_KEYWORD_SIN".Add(
                """
                <style=cKeywordName>Sin</style>Increases the execute threshold by an amount scaling with enemy strength. Executes when struck by Ceaseless Judgement. 66% less effective on bosses.
                """
            );
        }

        public static bool HasTatteredBandages(CharacterBody body) {
            return body.bodyIndex == JustitiaBody;
        }

        public static bool AffectedByAnyDebuff(CharacterBody body) {
            foreach (BuffIndex index in BuffCatalog.debuffBuffIndices) {
                if (body.HasBuff(index)) return true;
            }

            DotController dot = DotController.FindDotController(body.gameObject);

            if (dot) {
                for (DotController.DotIndex index = DotController.DotIndex.Bleed; index < DotController.DotIndex.Count; index++) {
                    if (dot.HasDotActive(index)) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}