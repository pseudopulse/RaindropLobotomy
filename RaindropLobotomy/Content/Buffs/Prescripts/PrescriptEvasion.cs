using System;

namespace RaindropLobotomy.Buffs {
    public class PrescriptEvasion : BuffBase<PrescriptEvasion>
    {
        public override BuffDef Buff => Load<BuffDef>("bdPrescriptEvasion.asset");

        public override void PostCreation()
        {
            On.RoR2.CharacterBody.RecalculateStats += IncreaseStats;
        }

        private void IncreaseStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self.HasBuff(Buff)) {
                self.moveSpeed *= 1.5f;
                self.maxJumpCount += 3;
                self.acceleration *= 2f;
            }
        }
    }
}