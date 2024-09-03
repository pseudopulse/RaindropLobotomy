using System;

namespace RaindropLobotomy.EGO.Viend {
    public class Claw : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 0.5f;

        public override float DamageCoefficient => 5f;

        public override string HitboxName => "Melee";

        public override GameObject HitEffectPrefab => Paths.GameObject.SpurtImpBlood;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => EGOMimicry.SlashEffect;

        public override string MuzzleString => "MuzzleMelee";
        public override string MechanimHitboxParameter => "Melee.hitBoxActive";

        public override void OnEnter()
        {
            base.OnEnter();

            AkSoundEngine.PostEvent(Events.Play_bandit2_m2_slash, base.gameObject);

            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity += (base.characterDirection.forward * 16f) + (Vector3.up * 5f);
        }

        public override void PlayAnimation()
        {
            PlayAnimation("LeftArm, Override", "SwingMelee3", "Melee.playbackRate", duration);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.AddModdedDamageType(EGOMimicry.ClawLifestealType);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}