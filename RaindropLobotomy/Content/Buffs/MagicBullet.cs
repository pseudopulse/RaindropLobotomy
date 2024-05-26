using System;

namespace RaindropLobotomy.Buffs {
    public class MagicBullet : BuffBase<MagicBullet>
    {
        public override BuffDef Buff => Load<BuffDef>("bdMagicBullet");

        public override void PostCreation()
        {
            
        }
    }
}