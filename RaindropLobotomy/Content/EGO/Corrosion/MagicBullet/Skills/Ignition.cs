using System;
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy.EGO.Bandit {
    public class Ignition : CoolerBasicMeleeAttack {
        public override float BaseDuration => 0.4f;

        public override float DamageCoefficient => 5.5f;

        public override string HitboxName => "SlashBlade";

        public override GameObject HitEffectPrefab => Paths.GameObject.VoidImpactEffect;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => EGOMagicBullet.MagicBulletSlash;

        public override string MuzzleString => "MuzzleSlashBlade";

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Additive", "SlashBlade", "SlashBlade.playbackRate", duration);

            // FindModelChild("FlameSlash").gameObject.SetActive(true);

            AkSoundEngine.PostEvent(Events.Play_bandit2_m2_slash, base.gameObject);

            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity = (base.characterDirection.forward * 26f) + (base.transform.up * 3f);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.AddModdedDamageType(DarkFlame.Instance.DarkFlameDamageType);
        }

        public override void OnExit()
        {
            base.OnExit();
            // FindModelChild("FlameSlash").gameObject.SetActive(false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}