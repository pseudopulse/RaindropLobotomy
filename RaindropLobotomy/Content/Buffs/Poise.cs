using System;

namespace RaindropLobotomy.Buffs {
    public class Poise : BuffBase<Poise>
    {
        public override BuffDef Buff => Load<BuffDef>("bdPoise");
        public DamageAPI.ModdedDamageType GivePoise = DamageAPI.ReserveDamageType();

        public override void PostCreation()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddPoiseBuffs;
            On.RoR2.GlobalEventManager.OnHitEnemy += ReducePoise;
        }

        private void ReducePoise(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.crit && damageInfo.attacker) {
                int count = damageInfo.attacker.GetComponent<CharacterBody>().GetBuffCount(Buff);
                
                if (count > 0) {
                    damageInfo.attacker.GetComponent<CharacterBody>().SetBuffCount(Buff.buffIndex, Mathf.Clamp(count - 1, 0, 20));
                }
            }

            if (damageInfo.attacker && damageInfo.HasModdedDamageType(GivePoise)) {
                int count = damageInfo.attacker.GetComponent<CharacterBody>().GetBuffCount(Buff);
                damageInfo.attacker.GetComponent<CharacterBody>().SetBuffCount(Buff.buffIndex, Mathf.Clamp(count + 1, 0, 20));
            }
        }

        private void AddPoiseBuffs(CharacterBody sender, StatHookEventArgs args)
        {
            int PoiseCount = sender.GetBuffCount(Buff);

            if (PoiseCount > 0) {
                args.critAdd += PoiseCount * 5;
            }
        }
    }
}