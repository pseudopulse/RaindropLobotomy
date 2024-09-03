using System;
using RoR2.ConVar;

namespace RaindropLobotomy.EGO.Viend {
    public class WearShell : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 0.7f;

        public override float DamageCoefficient => 12f;

        public override string HitboxName => "Melee";

        public override GameObject HitEffectPrefab => Paths.GameObject.SpurtImpBlood;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.1f;

        public override GameObject SwingEffectPrefab => EGOMimicry.SlashEffect;

        public override string MuzzleString => "MuzzleVerticalMelee";

        public override void OnEnter()
        {
            base.OnEnter();

            AkSoundEngine.PostEvent("Play_NT_bigslash", base.gameObject);
        }

        public override void PlayAnimation()
        {
            PlayAnimation("RightArm, Override", "FireMegaBlaster", "Melee.playbackRate", duration);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.AddModdedDamageType(EGOMimicry.WearShellType);
            overlapAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            overlapAttack.teamIndex = TeamIndex.Neutral;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}