using System;
using RaindropLobotomy.EGO.Commando;

namespace RaindropLobotomy.Buffs {
    public class StackSeal : BuffBase<StackSeal>
    {
        public override BuffDef Buff => Load<BuffDef>("bdLamentStackSeal.asset");

        public override void PostCreation()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEnemy;
        }

        private void OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.HasModdedDamageType(SolemnLament.StackingLament) && victim) {
                CharacterBody body = victim.GetComponent<CharacterBody>();

                if (body) {
                    int count = body.GetBuffCount(Buff);
                    body.SetBuffCount(Buff.buffIndex, count + 1);

                    if (count + 1 >= 5) {
                        body.AddTimedBuff(Buffs.Sealed.Instance.Buff, 10f, 4);
                        body.SetBuffCount(Buff.buffIndex, 0);
                    }
                }
            }
        }
    }
}