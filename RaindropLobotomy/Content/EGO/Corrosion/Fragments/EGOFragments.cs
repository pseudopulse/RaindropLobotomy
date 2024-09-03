/*using System;

namespace RaindropLobotomy.EGO.Arti {
    public class EGOFragments : CorrosionBase<EGOFragments>
    {
        public override string EGODisplayName => "Fragments from Somewhere";

        public override string Description => "You see a song in front of you. It's approaching, becoming more colorful by the second.";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.Mage;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => Color.magenta;

        public override SurvivorDef Survivor => Load<SurvivorDef>("sdFragmentsEGO.asset");

        public override GameObject BodyPrefab => Load<GameObject>("FragmentsEGOBody.prefab");

        public override GameObject MasterPrefab => null;

        public override void Modify()
        {
            base.Modify();
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_EGO_FRAGMENTS_NAME".Add("Artificer :: Fragments from Somewhere");

            "RL_KEYWORD_ENLIGHTENED".Add("");

            "RL_EGO_FRAGMENTS_PASSIVE_NAME".Add("Incomprehensible");
            "RL_EGO_FRAGMENTS_PASSIVE_DESC".Add("Melee attacks inflict <style=cDeath>Enlightenment</style>. Targets who are <style=cDeath>Enlightened</style> will have <style=cIsDamage>increased stats</style>, <style=cIsUtility>reduced stagger threshold</style>, and <style=cDeath>lash out indiscriminately</style>.");

            "RL_EGO_FRAGMENTS_PRIMARY_NAME".Add("Enlighten");
            "RL_EGO_FRAGMENTS_PRIMARY_DESC".Add("Slash forward, dealing <style=cIsDamage>160% damage</style>.");

            "RL_EGO_FRAGMENTS_SECONDARY_NAME".Add("Penetration of the Boundary");
            "RL_EGO_FRAGMENTS_SECONDARY_DESC".Add("Spear forward, dealing <style=cIsDamage>580% damage</style>. If the target is <style=cDeath>Enlightened</style>, stun them and <style=cIsUtility>reset their targeting</style>.");

            "RL_EGO_FRAGMENTS_UTILITY_NAME".Add("Dim Sound of Ringing");
            "RL_EGO_FRAGMENTS_UTILITY_DESC".Add("Perform a short-ranged scream, <style=cIsDamage>destroying</style> projectiles and <style=cIsUtility>stunning</style> enemies.");

            "RL_EGO_FRAGMENTS_SPECIAL_NAME".Add("Echoes from Beyond");
            "RL_EGO_FRAGMENTS_SPECIAL_DESC".Add("Sing, dealing <style=cIsDamage>4x400%</style> damage and <style=cIsUtility>slowing</style> targets. Targets that are <style=cDeath>Enlightened</style> will not be hit.");
        }
    }
}*/