using System;

namespace RaindropLobotomy.Enemies.Fragment {
    public class Penetrate : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 1.2f;

        public override float DamageCoefficient => 4f;

        public override string HitboxName => "StabHitbox";

        public override GameObject HitEffectPrefab => Assets.GameObject.VoidImpactEffect;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.001f;

        public override GameObject SwingEffectPrefab => UniverseFragment.SpearThrust;

        public override string MuzzleString => "SpearMuzzle";
        public override string MechanimHitboxParameter => "piercing";
        private bool spawnedEffect = false;

        public override void OnEnter()
        {
            base.OnEnter();
            AkSoundEngine.PostEvent("Play_fragment_stab", base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterMotor.velocity = Vector3.zero;
        }

        public override void BeginMeleeAttackEffect()
        {
            base.BeginMeleeAttackEffect();
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType |= DamageType.SlowOnHit;
            overlapAttack.teamIndex = TeamIndex.Neutral;
            overlapAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
        }

        public override void PlayAnimation()
        {
            PlayAnimation("Gesture, Override", "Penetrate", "Penetrate.playbackRate", duration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}