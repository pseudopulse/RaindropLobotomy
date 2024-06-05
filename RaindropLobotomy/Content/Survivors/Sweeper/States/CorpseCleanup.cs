using System;
using RaindropLobotomy.EGO.Viend;

namespace RaindropLobotomy.Survivors.Sweeper {
    public class CorpseCleanup : CoolerBasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public override float BaseDuration => 0.8f;

        public override float DamageCoefficient => 2.2f;

        public override string HitboxName => "Sweep";

        public override GameObject HitEffectPrefab => Assets.GameObject.SpurtGenericBlood;

        public override float ProcCoefficient => 1.0f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => step % 2 == 0 ? Sweeper.SwingEffectL : Sweeper.SwingEffectR;
        public string AnimState => step % 2 == 0 ? "SlashLeft" : "SlashRight";

        public override string MuzzleString => "MuzzleSweep";
        public override string MechanimHitboxParameter => "slashBegun";
        public int step = 0;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", AnimState, "Generic.playbackRate", duration);

            AkSoundEngine.PostEvent(Events.Play_bandit2_m2_slash, base.gameObject);

            characterMotor.velocity = characterMotor.velocity.Nullify(true, false, true);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            overlapAttack.AddModdedDamageType(Sweeper.BigLifesteal);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public void SetStep(int i)
        {
            step = i;
        }
    }
}