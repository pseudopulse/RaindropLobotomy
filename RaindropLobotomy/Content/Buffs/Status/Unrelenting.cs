using System;

namespace RaindropLobotomy.Buffs {
    public class Unrelenting : BuffBase<Unrelenting>
    {
        public override BuffDef Buff => Load<BuffDef>("bdUnrelenting.asset");

        public override void PostCreation()
        {
            On.RoR2.HealthComponent.TakeDamage += TheOscarKeypageIsBalanced;
        }

        private void TheOscarKeypageIsBalanced(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self.body.HasBuff(Buff)) {
                damageInfo.damageType |= DamageType.NonLethal;
            }
            
            orig(self, damageInfo);
        }
    }
}