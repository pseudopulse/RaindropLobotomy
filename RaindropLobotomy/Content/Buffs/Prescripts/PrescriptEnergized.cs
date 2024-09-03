using System;
using RaindropLobotomy.EGO.Merc;

namespace RaindropLobotomy.Buffs {
    public class PrescriptEnergized : BuffBase<PrescriptEnergized>
    {
        public override BuffDef Buff => Load<BuffDef>("bdPrescriptEnergized.asset");

        public override void PostCreation()
        {
            RecalculateStatsAPI.GetStatCoefficients += IncreaseStats;
            // On.RoR2.GenericSkill.SetBonusStockFromBody += SetBonusStock;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += OnGrantPrescript;
        }

        private void OnGrantPrescript(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            orig(self, buffDef, duration);

            if (buffDef == Buff && self.skillLocator) {
                foreach (GenericSkill slot in self.skillLocator.allSkills) {
                    if (slot == self.skillLocator.special && (self.bodyIndex == IndexMerc.IndexMercBody || self.bodyIndex == IndexMerc.IndexPaladinBody)) {
                        continue;
                    }

                    if (!slot.CanApplyAmmoPack()) {
                        continue;
                    }

                    slot.ApplyAmmoPack();
                }
            }
        }

        private void SetBonusStock(On.RoR2.GenericSkill.orig_SetBonusStockFromBody orig, GenericSkill self, int newBonusStockFromBody)
        {
            if (self.characterBody.HasBuff(Buff)) {
                newBonusStockFromBody += 1;
            }

            orig(self, newBonusStockFromBody);
        }

        private void IncreaseStats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(Buff)) {
                float mult = sender.bodyIndex == IndexMerc.IndexGiantFistBody ? 2f : 1f;
                args.cooldownMultAdd -= 0.50f * mult;
            }
        }
    }
}