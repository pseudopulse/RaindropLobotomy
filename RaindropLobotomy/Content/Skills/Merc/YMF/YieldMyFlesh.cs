using System;

namespace RaindropLobotomy.Skills.Merc {
    public class YieldMyFlesh : SkillBase<YieldMyFlesh>
    {
        public override SkillDef SkillDef => Load<SkillDef>("sdYMF.asset");

        public override SurvivorDef Survivor => Assets.SurvivorDef.Merc;

        public override SkillSlot Slot => SkillSlot.Special;

        public override UnlockableDef Unlock => null;
        public static SkillDef ToClaimTheirBones;

        public override void CreateLanguage()
        {
            base.CreateLanguage();

            "RL_KEYWORD_UNRELENTING".Add("<style=cKeywordName>Unrelenting</style>While in an <style=cDeath>Unrelenting</style> state, your health cannot drop below 1. Gain brief invulnerability when leaving an <style=cDeath>Unrelenting</style> state.");
        
            "RL_SKILL_YMF_NAME".Add("Yield My Flesh");
            "RL_SKILL_YMF_DESC".Add(
                "<style=cDeath>Unrelenting.</style> Slash forward, dealing <style=cIsDamage>400% damage</style>. If you took damage during this skill, replace it with <style=cIsDamage>To Claim Their Bones</style>."
            );

            "RL_KEYWORD_TCTB".Add(
                "<style=cKeywordName>To Claim Their Bones</style>Perform a wide slash for <style=cIsDamage>400%-4600%</style> damage. Heal <style=cIsUtility>0.5%-5%</style> health for every target hit. <style=cIsDamage>Radius, damage, and healing scales with the damage taken to use this skill</style>."
            );

            "RL_SKILL_TCTB_NAME".Add("To Claim Their Bones");
            "RL_SKILL_TCTB_DESC".Add("<style=cDeath>Slowing.</style> <style=cIsUtility>Crippling.</style> <style=cIsDamage>Slayer.</style> Perform a wide slash for <style=cIsDamage>400%-4600%</style> damage. Heal <style=cIsUtility>0.5%-5%</style> health for every target hit. <style=cIsDamage>Radius, damage, and healing scales with the damage taken to use this skill</style>.");

            ToClaimTheirBones = Load<SkillDef>("sdTCTB.asset");
            ContentAddition.AddSkillDef(ToClaimTheirBones);
        }
    }
}