using System;
using RoR2.UI;

namespace RaindropLobotomy.EGO.Toolbot {
    public class Grinder : CorrosionBase<Grinder>
    {
        public override string EGODisplayName => "Grinder MK. 5-2";

        public override string Description => "Contamination scan complete. Initiating cleaning protocol.";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.Toolbot;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => default;

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdGrinder.asset");

        public override GameObject BodyPrefab => Load<GameObject>("GrinderBody.prefab");

        public override GameObject MasterPrefab => null;
        //
        public static GameObject CleanupSlash;
        public static GameObject MultiSlash;
        //
        public static BuffDef Charge;
        //
        public LazyIndex GrinderIndex = new("GrinderBody");
        public SkillDef Clean;
        public SkillDef NoLimit;

        // TODO:
        // Tweak blood splatter textures
        // Surv Portrait
        // Unlock condition (?)
        // Change saw vfx to be bloody like the slashes
        // Tweak the VFX on Initiate Cleaning

        public override void Modify()
        {
            base.Modify();

            BodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpToolbot;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Paths.ItemDisplayRuleSet.idrsToolbot;
            Load<GameObject>("GrinderDisplay.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animToolbotDisplay;
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Paths.GameObject.MercBody.GetComponent<CharacterBody>().defaultCrosshairPrefab;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animToolbot;

            CleanupSlash = Load<GameObject>("CleanupSlash.prefab");
            MultiSlash = Load<GameObject>("MultiSlashGrinder.prefab");

            Charge = Load<BuffDef>("bdGrinderCharge.asset");
            ContentAddition.AddBuffDef(Charge);

            Clean = Load<SkillDef>("Clean.asset");
            NoLimit = Load<SkillDef>("NoLimit.asset");

            On.RoR2.DotController.AddDot += OnInflictDOT;
            On.RoR2.Skills.SkillDef.IsReady += ChargeBlocker;

            RecalculateStatsAPI.GetStatCoefficients += ChargeStats;
        }

        private void ChargeStats(CharacterBody sender, StatHookEventArgs args)
        {
            int c = sender.GetBuffCount(Charge);
            float mult = c * 0.035f;

            if (c > 0) {
                args.moveSpeedMultAdd += mult;
            }
        }

        private bool ChargeBlocker(On.RoR2.Skills.SkillDef.orig_IsReady orig, RoR2.Skills.SkillDef self, GenericSkill skillSlot)
        {
            if (self == Clean) {
                if (skillSlot.characterBody.GetBuffCount(Charge) < 3) {
                    return false;
                }
            }

            if (self == NoLimit) {
                if (skillSlot.characterBody.GetBuffCount(Charge) < 10) {
                    return false;
                }
            }

            return orig(self, skillSlot);
        }

        private void OnInflictDOT(On.RoR2.DotController.orig_AddDot orig, DotController self, GameObject attackerObject, float duration, DotController.DotIndex dotIndex, float damageMultiplier, uint? maxStacksFromAttacker, float? totalDamage, DotController.DotIndex? preUpgradeDotIndex)
        {
            if (dotIndex == DotController.DotIndex.Bleed && self.victimBody && attackerObject && attackerObject.GetComponent<CharacterBody>().bodyIndex == GrinderIndex) {
                float multiplier = 1f + (self.victimBody.GetBuffCount(RoR2Content.Buffs.Bleeding) * 0.05f);
                damageMultiplier = multiplier;
            }

            orig(self, attackerObject, duration, dotIndex, damageMultiplier, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
        }

        public static void IncreaseCharge(CharacterBody body) {
            int count = body.GetBuffCount(Charge) + 1;
            UpdateCharge(body, count);
        }

        public static void DecreaseCharge(CharacterBody body, int amount) {
            int count = body.GetBuffCount(Charge) - amount;
            UpdateCharge(body, count);
        }

        public static void UpdateCharge(CharacterBody body, int count) {
            count = Mathf.Clamp(count, 0, 10);
            body.SetBuffCount(Charge.buffIndex, count);
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_EGO_GRINDER_BODY_NAME".Add("Grinder MK. 5-2");

            "RL_EGO_GRINDER_PASSIVE_NAME".Add("Repetitive Pattern Recognition");
            "RL_EGO_GRINDER_PASSIVE_DESC".Add("Increase the damage of <style=cDeath>Bleed</style> by <style=cIsDamage>5%</style> when re-inflicting the same target.");

            "RL_EGO_GRINDER_PRIMARY_NAME".Add("Exterminate");
            "RL_EGO_GRINDER_PRIMARY_DESC".Add("Repeatedly shred targets for <style=cIsDamage>550% damage per second</style>. Has a <style=cIsDamage>5%</style> chance to <style=cDeath>bleed</style> per <style=cIsUtility>Charge</style>.");

            "RL_EGO_GRINDER_SECONDARY_NAME".Add("Initiate Cleaning");
            "RL_EGO_GRINDER_SECONDARY_DESC".Add("Slash repeatedly, dealing <style=cIsDamage>4x250% damage</style> and <style=cDeath>bleeding</style>. <style=cIsUtility>Consume 3 Charge.</style>");

            "RL_EGO_GRINDER_UTILITY_NAME".Add("Rest");
            "RL_EGO_GRINDER_UTILITY_DESC".Add("Become temporarily <style=cDeath>dormant</style>, restoring <style=cIsUtility>2 Charge</style> per second and accelerating skill recharge speed.");

            "RL_EGO_GRINDER_SPECIAL_NAME".Add("Disable Limiter");
            "RL_EGO_GRINDER_SPECIAL_DESC".Add("Consume <style=cIsUtility>10 Charge</style>, and rapidly tear through targets for <style=cIsDamage>3x800%</style> damage, <style=cDeath>bleeding</style> on hit.");
        }
    }
}