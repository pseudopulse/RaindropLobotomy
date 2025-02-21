/* using System;
using System.Collections;
using System.Linq;
using RoR2.CharacterAI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

namespace RaindropLobotomy.EGO.Mage {
    public class Arbitificer : CorrosionBase<Arbitificer>
    {
        public override string EGODisplayName => "Arbitificer";

        public override string Description => "A star can only truly be seen in the darkness.";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.Mage;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => Color.yellow;

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdArbitificer.asset");

        public override GameObject BodyPrefab => Load<GameObject>("ArbitificerBody.prefab");

        public override GameObject MasterPrefab => null;
        public const string STYLE_FAIRY = "<color=#FA9907>"; // make this be a proper color later
        //
        public static GameObject FairyWave;
        public static GameObject FairySpearEffect;

        // TODO:
        // Implement:
        // - fairy status
        // - m2
        // - util
        // - special
        // - passive
        // Improve:
        // - m1 vfx (fade out, make streaks move way faster)
        // Fix:
        // - weights (cloak should have some fallback towards the back and be weighted closer to the arms; fix arms dragging cloak through body)
        // - texture (needs hexagons)

        public override void Create()
        {
            base.Create();
        }

        public override void Modify()
        {
            base.Modify();

            BodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandard;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animMage;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Paths.ItemDisplayRuleSet.idrsMage;
            Load<GameObject>("DisplayArbitificer.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animMageDisplay;
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Paths.GameObject.CommandoBody.GetComponent<CharacterBody>().defaultCrosshairPrefab;

            FairyWave = Load<GameObject>("FairySlashProjectile.prefab");
            ContentAddition.AddProjectile(FairyWave);

            Load<GameObject>("FairySlashGhost.prefab").AddComponent<VFXAttributes>().DoNotPool = true;

            FairySpearEffect = Load<GameObject>("FairyExplosion.prefab");
            ContentAddition.AddEffect(FairySpearEffect);
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_ARBITIFICER_NAME".Add("House Arbiter");

            "RL_ARBITIFICER_PASSIVE_NAME".Add("Looming Prescence");
            "RL_ARBITIFICER_PASSIVE_DESC".Add($"Passively inflict <style=cIsDamage>Dark Fog</style> on nearby targets, <style=cIsUtility>slowing their movement and attacks</style>. <style=cIsUtility>Immunity to fall damage.</style>");

            "RL_EGO_ARBITIFICER_PRIMARY_NAME".Add("Severing Wave");
            "RL_EGO_ARBITIFICER_PRIMARY_DESC".Add($"Slash forward for <style=cIsDamage>220% damage</style>, piercing in a short range and inflicting {STYLE_FAIRY}Fairy</color>. <style=cIsUtility>Hits reduce the recharge time of Prismatic Meltdown.</style>");

            "RL_EGO_ARBITIFICER_SECONDARY_NAME".Add("Fairy Spear");
            "RL_EGO_ARBITIFICER_SECONDARY_DESC".Add($"<style=cIsDamage>Stunning.</style> Spear the target, dealing <style=cIsDamage>600% damage</style> and inflicting additional {STYLE_FAIRY}Fairy</color> equal to <style=cIsDamage>40%</style> of the target's stacks.");

            "RL_EGO_ARBITIFICER_UTILITY_NAME".Add("Rupturing Chain");
            "RL_EGO_ARBITIFICER_UTILITY_DESC".Add($"<style=cIsDamage>Stunning.</style> Send forth burrowing chains, which spring up and <style=cIsUtility>root</style> a target for <style=cIsDamage>1200% damage</style>.");

            "RL_EGO_ARBITIFICER_SPECIAL_NAME".Add("Prismatic Meltdown");
            "RL_EGO_ARBITIFICER_SPECIAL_DESC".Add($"Charge up a radial spread of <style=cIsDamage>4</style> pillars for <style=cIsDamage>250% damage</style> each. Inflicts <style=cIsUtility>Pale Damage</style> equal to <style=cIsDamage>25%</style> of the target's {STYLE_FAIRY}Fairy</color> stacks.");

            "KEYWORD_FAIRY".Add(
                "<style=cKeywordName>Fairy</style>."
            );

            "KEYWORD_PALE".Add(
                "<style=cKeywordName>Pale Damage</style>"
            );
        }
    }
}*/