using System;

namespace RaindropLobotomy.EGO.Viend {
    public class Goodbye : BaseState {
        public override void OnEnter()
        {
            base.OnEnter();
            FindModelChild("ScytheScaleBone").GetComponent<EGOMimicry.GoodbyeArmStretcher>().BeginGoodbye(0);
            base.StartAimMode();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= 0.2f) {
                outer.SetNextState(new GoodbyeSlash());
            }
        }
    }
    public class GoodbyeSlash : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 0.5f;

        public override float DamageCoefficient => 22f;

        public override string HitboxName => "Goodbye";

        public override GameObject HitEffectPrefab => Assets.GameObject.SpurtImpBlood;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => EGOMimicry.SlashEffect;

        public override string MuzzleString => "MuzzleGoodbye";

        public override void OnEnter()
        {
            base.OnEnter();
            
            base.characterMotor.Motor.ForceUnground();
            base.characterMotor.velocity += (base.characterDirection.forward * 26f) + (base.transform.up * 9f);

            AkSoundEngine.PostEvent(Events.Play_bandit2_m2_slash, base.gameObject);
        }

        public override void AuthorityOnFinish()
        {
            outer.SetNextState(new GoodbyeEnd());
        }

        public override void PlayAnimation()
        {
            PlayAnimation("RightArm, Override", "FireMegaBlaster", "Melee.playbackRate", duration);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.AddModdedDamageType(EGOMimicry.WearShellType);
            overlapAttack.AddModdedDamageType(EGOMimicry.MistType);
            overlapAttack.teamIndex = TeamIndex.Neutral;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void BeginMeleeAttackEffect()
        {
            base.BeginMeleeAttackEffect();
            ParticleSystem system = swingEffectInstance.transform.Find("Rotator").Find("SwingTrail").GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            ParticleSystem.MinMaxCurve curve = main.startSize;
            curve.constant *= 2f;
            main.startLifetimeMultiplier *= 2f;
        }
    }

    public class GoodbyeEnd : BaseState {
        public override void OnEnter()
        {
            base.OnEnter();
            FindModelChild("ScytheScaleBone").GetComponent<EGOMimicry.GoodbyeArmStretcher>().BeginGoodbye(1);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= 0.2f) {
                outer.SetNextStateToMain();
            }
        }
    }
}