using System;

namespace RaindropLobotomy.Buffs {
    public class Enchanted : BuffBase<Enchanted>
    {
        public override BuffDef Buff => Load<BuffDef>("bdEnchanted.asset");

        public override void PostCreation()
        {
            
        }
    }
}