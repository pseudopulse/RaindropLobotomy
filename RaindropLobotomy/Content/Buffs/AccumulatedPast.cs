using System;

namespace RaindropLobotomy.Buffs {
    public class AccumulatedPast : BuffBase<AccumulatedPast>
    {
        public override BuffDef Buff => Load<BuffDef>("bdAccPast.asset");

        public override void PostCreation()
        {
            
        }
    }
}