using System;
using RaindropLobotomy.Survivors.Sweeper;

namespace RaindropLobotomy.Buffs {
    public class BloodBoost : BuffBase<BloodBoost>
    {
        public override BuffDef Buff => Load<BuffDef>("bdFHRBoost.asset");

        public override void PostCreation()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddBuffs;
        }

        private void AddBuffs(CharacterBody sender, StatHookEventArgs args)
        {
            int BloodCount = sender.GetBuffCount(Buff);

            if (BloodCount > 0) {
                args.damageMultAdd += 0.005f * BloodCount;
                args.armorAdd -= 0.005f * BloodCount;
            }
        }
    }
}