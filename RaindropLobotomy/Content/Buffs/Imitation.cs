using System;

namespace RaindropLobotomy.Buffs {
    public class Imitation : BuffBase<Imitation>
    {
        public override BuffDef Buff => Load<BuffDef>("bdImitation.asset");

        public override void PostCreation()
        {
            
        }
    }
}