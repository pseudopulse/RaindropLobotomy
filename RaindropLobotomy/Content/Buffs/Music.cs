using System;
using RaindropLobotomy.Survivors.Sweeper;

namespace RaindropLobotomy.Buffs {
    public class Music : BuffBase<Music>
    {
        public override BuffDef Buff => Load<BuffDef>("SM_MusicBuff.asset");

        public override void PostCreation()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddMusicBuffs;
        }

        private void AddMusicBuffs(CharacterBody sender, StatHookEventArgs args)
        {
            int MusicCount = sender.GetBuffCount(Buff);

            if (MusicCount > 0) {
                args.attackSpeedMultAdd += 1f;
                args.moveSpeedMultAdd += 1f;
            }
        }
    }
}