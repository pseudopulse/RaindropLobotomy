using System;

namespace RaindropLobotomy.Survivors.Argalia {
    public class Argalia : SurvivorBase<Argalia>
    {
        public override SurvivorDef Survivor => Load<SurvivorDef>("sdArgalia.asset");

        public override GameObject BodyPrefab => Load<GameObject>("ArgaliaBody.prefab");

        public override GameObject MasterPrefab => null;

        public override void Modify()
        {
            base.Modify();
        }
        public override void SetupLanguage()
        {
            "RL_ARGALIA_NAME".Add("Argalia");

            "RL_KEYWORD_RESONANCE".Add("");

            "RL_PASSIVE_REVERB_NAME".Add("The Blue Reverberation");
            "RL_PASSIVE_REVERB_DESC".Add("Reduce <style=cIsUtility>incoming ranged damage</style> by <style=cIsUtility>10%</style> per stack of <style=cIsUtility>resonance</style> you have.");

            "RL_PASSIVE_NUOVO_NAME".Add("Nuovo Fabric");
            "RL_PASSIVE_NUOVO_DESC".Add("<style=cIsUtility>Immune</style> to <style=cIsDamage>movement impairing</style> effects.");

            "RL_PRIMARY_ALLEGRO_NAME".Add("Allegro");
            "RL_PRIMARY_ALLEGRO_DESC".Add("<style=cIsUtility>Resonant</style>. <style=cIsDamage>Slash</style> forward for 220% damage.");

            "RL_PRIMARY_BLUETRAIL_NAME".Add("Trails of Blue");
            "RL_PRIMARY_BLUETRAIL_DESC".Add("<style=cIsUtility>Resonant</style>. <style=cIsDamage>Slash</style> vertically for <style=cIsDamage>440% damage</style>. Targets are <style=cIsUtility>entranced.</style>.");

            "RL_SECONDARY_RESONANCE_NAME".Add("Controlled Resonance");
            "RL_SECONDARY_RESONANCE_DESC".Add("For <style=cIsDamage>9</style> seconds, <style=cIsUtility>Resonance</style> will trigger regardless of count. <style=cDeath>Enemies may trigger Resonance on you for the duration.</style>");

            "RL_SECONDARY_SCYTHE_NAME".Add("Resonant Scythe");
            "RL_SECONDARY_SCYTHE_DESC".Add("<style=cIsUtility>Resonant</style>. <style=cIsDamage>Slam</style> downward, dealing <style=cIsDamage>700% damage</style> in a radius.");

            "RL_UTILITY_DANZA_NAME".Add("Tempestuous Danza");
            "RL_UTILITY_DANZA_DESC".Add("<style=cIsUtility>Blink</style> towards a target and <style=cIsDamage>slash</style> for <style=cIsDamage>390% damage</style>. The target's <style=cIsUtility>resonance</style> is set to <style=cIsDamage>75%</style> of yours.");

            "RL_SPECIAL_CRESCENDO_NAME".Add("Crescendo");
            "RL_SPECIAL_CRESCENDO_DESC".Add("<style=cDeath>Unstable</style>. <style=cIsDamage>Slash</style> upward for <style=cIsDamage>350% damage</style>, then strike <style=cIsUtility>all resonant enemies</style> for <style=cIsDamage>2000% damage</style>.");

            "RL_SPECIAL_FINALE_NAME".Add("Grand Finale");
            "RL_SPECIAL_FINALE_DESC".Add("");
        }
    }
}