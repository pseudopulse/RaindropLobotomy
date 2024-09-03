using System;

namespace RaindropLobotomy.EGO.Commando {
    public class SolemnLament : CorrosionBase<SolemnLament>
    {
        public override string EGODisplayName => "Solemn Lament";

        public override string Description => "Where does one go when they die?";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.Commando;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => default;

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdLament.asset");

        public override GameObject BodyPrefab => Load<GameObject>("LamentBody.prefab");

        public override GameObject MasterPrefab => null;
        //
        public static DamageAPI.ModdedDamageType StackingLament = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType Seal = DamageAPI.ReserveDamageType();
        //
        public static GameObject LamentMuzzleFlashWhite;
        public static GameObject LamentMuzzleFlashBlack;
        public static GameObject LamentTracerWhite;
        public static GameObject LamentTracerBlack;
        //
        public static GameObject GuidingHandWhite;
        public static GameObject GuidingHandBlack;

        public class LamentConfig : ConfigClass
        {
            public override string Section => "EGO Corrosions :: Solemn Lament";
            public bool ReplaceSoundEffects => Option<bool>("Vanilla Sounds", "Use the crit sound effect instead of the ruina ding.", false);

            public override void Initialize()
            {
                _ = ReplaceSoundEffects;
            }
        }

        public static LamentConfig config = new();

        public override void Modify()
        {
            base.Modify();

            BodyPrefab.GetComponent<CameraTargetParams>().cameraParams = Paths.CharacterCameraParams.ccpStandard;

            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animCommando;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet = Paths.ItemDisplayRuleSet.idrsCommando;
            Load<GameObject>("LamentDisplay.prefab").GetComponentInChildren<Animator>().runtimeAnimatorController = Paths.RuntimeAnimatorController.animCommandoDisplay;
            BodyPrefab.GetComponent<CharacterBody>()._defaultCrosshairPrefab = Paths.GameObject.CommandoBody.GetComponent<CharacterBody>().defaultCrosshairPrefab;
            BodyPrefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Paths.GameObject.GenericFootstepDust;

            LamentMuzzleFlashBlack = Load<GameObject>("LamentMuzzleFlashBlack.prefab");
            LamentMuzzleFlashWhite = Load<GameObject>("LamentMuzzleFlashWhite.prefab");
            LamentTracerBlack = Load<GameObject>("LamentTracerBlack.prefab");
            LamentTracerWhite = Load<GameObject>("LamentTracerWhite.prefab");

            GuidingHandWhite = Load<GameObject>("GuidingHandProjectileWhite.prefab");
            GuidingHandBlack = Load<GameObject>("GuidingHandProjectileBlack.prefab");

            GuidingHandBlack.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Seal);
            GuidingHandWhite.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(Seal);

            ContentAddition.AddProjectile(GuidingHandBlack);
            ContentAddition.AddProjectile(GuidingHandWhite);
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_EGO_LAMENT_NAME".Add("Solemn Lament");


            "RL_EGO_LAMENT_PASSIVE_NAME".Add("Hand of Salvation");
            "RL_EGO_LAMENT_PASSIVE_DESC".Add("Upon <style=cIsUtility>sealing</style> all skill slots of an enemy, <style=cIsDamage>stun</style> them for <style=cIsDamage>10 seconds</style>. <style=cStack>This effect is halved against bosses.</style>");

            "RL_EGO_LAMENT_PRIMARY_NAME".Add("Solemn Lament");
            "RL_EGO_LAMENT_PRIMARY_DESC".Add("Fire slow, heavy shots for <style=cIsDamage>250% damage</style>. <style=cIsUtility>Seal</style> a skill slot after hitting the same target <style=cIsDamage>5</style> times.");

            "RL_EGO_LAMENT_SECONDARY_NAME".Add("Guiding Hand");
            "RL_EGO_LAMENT_SECONDARY_DESC".Add("Send out a pair of seeking butterflies to <style=cIsUtility>seal</style> two skill slots on an enemy for <style=cIsDamage>2x250% damage</style>");

            "RL_EGO_LAMENT_UTILITY_NAME".Add("Tranquility");
            "RL_EGO_LAMENT_UTILITY_DESC".Add("Phase into a swarm of butterflies, becoming <style=cIsUtility>intangible</style> and dealing <style=cIsDamage>400% damage per second</style> to enemies you walk through, then explode into a burst of butterflies that <style=cIsUtility>seals</style> for <style=cIsDamage>400% damage</style>.");

            "RL_EGO_LAMENT_SPECIAL_NAME".Add("Eternal Rest");
            "RL_EGO_LAMENT_SPECIAL_DESC".Add("Fire rapidly at all <style=cIsDamage>sealed</style> targets in sight, dealing <style=cIsDamage>16x200% damage</style>");
        }
    }
}