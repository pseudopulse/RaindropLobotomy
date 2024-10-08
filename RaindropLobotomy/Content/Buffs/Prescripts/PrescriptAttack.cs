using System;
using RaindropLobotomy.EGO.Merc;

namespace RaindropLobotomy.Buffs {
    public class PrescriptAttack : BuffBase<PrescriptAttack>
    {
        public override BuffDef Buff => Load<BuffDef>("bdPrescriptAttack.asset");

        public override void PostCreation()
        {
            RecalculateStatsAPI.GetStatCoefficients += IncreaseStats;
            On.RoR2.GlobalEventManager.OnHitEnemy += IncreaseProcCoefficient;
        }

        private void IncreaseProcCoefficient(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>()) {
                if (damageInfo.attacker.GetComponent<CharacterBody>().HasBuff(Buff)) {
                    damageInfo.procCoefficient *= 1.25f;
                }
            }
            
            orig(self, damageInfo, victim);
        }

        private void IncreaseStats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(Buff)) {
                float mult = sender.bodyIndex == IndexMerc.IndexGiantFistBody ? 2f : 1f;
                args.attackSpeedMultAdd += 0.75f * mult;
                args.damageMultAdd += 0.25f * mult;
            }
        }
    }
}