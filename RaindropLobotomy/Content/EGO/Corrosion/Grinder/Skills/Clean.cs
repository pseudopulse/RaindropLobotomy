using System;
using RaindropLobotomy.EGO.Viend;

namespace RaindropLobotomy.EGO.Toolbot {
    public class Clean : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 0.5f;

        public override float DamageCoefficient => 2.5f;

        public override string HitboxName => "Slashes";

        public override GameObject HitEffectPrefab => Assets.GameObject.SpurtImpBlood;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => Grinder.CleanupSlash;

        public override string MuzzleString => "MuzzleCleanup";
        public override string MechanimHitboxParameter => "Hitbox.active";
        public Animator animBlades;
        private int resetCount = 0;
        private Timer timer = new(0.15f, false, true, false, true);

        public override void OnEnter()
        {
            base.OnEnter();

            AkSoundEngine.PostEvent(Events.Play_bandit2_m2_slash, base.gameObject);

            // base.characterMotor.Motor.ForceUnground();
            // base.characterMotor.velocity += (base.characterDirection.forward * 16f);

            Grinder.DecreaseCharge(base.characterBody, 3);
        }

        public override void FixedUpdate()
        {
            if (animBlades) animator = animBlades;

            if (animBlades && timer.Tick() && resetCount < 4 && animBlades.GetFloat(MechanimHitboxParameter) >= 0.5f) {
                resetCount++;
                overlapAttack.ResetIgnoredHealthComponents();
            }

            base.FixedUpdate();
        }

        public override void PlayAnimation()
        {
            animBlades = FindModelChild("Blades").GetComponent<Animator>();
            PlayAnimationOnAnimator(animBlades, "Weapon", "Grind", "Generic.playbackRate", duration);

            AkSoundEngine.PostEvent(Events.Play_bandit2_m2_slash, base.gameObject);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.damageType |= DamageType.BleedOnHit;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}