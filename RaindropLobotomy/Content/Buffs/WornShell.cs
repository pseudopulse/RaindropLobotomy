using System;

namespace RaindropLobotomy.Buffs {
    public class WornShell : BuffBase<WornShell>
    {
        public override BuffDef Buff => Load<BuffDef>("bdWornShell.asset");

        public override void PostCreation()
        {
            
        }
    }
}