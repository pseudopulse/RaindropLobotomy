using System;

namespace RaindropLobotomy.EGO.Merc {
    public class SwordDraw : CoolerBasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public override float BaseDuration => 0.6f;

        public override float DamageCoefficient => 2.6f;

        public override string HitboxName => "Sword";

        public override GameObject HitEffectPrefab => Assets.GameObject.OmniImpactVFXSlashMerc;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => BLMerc.SlashEffect;

        public override string MuzzleString => step % 2 == 0 ? "GroundLight2" : "GroundLight1";
        private string animation => step % 2 == 0 ? "GroundLight2" : "GroundLight1";
        public override string MechanimHitboxParameter => "Sword.active";

        private int step;

        public void SetStep(int i)
        {
            step = i;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            BLMerc.UpdateSwordplayState(base.characterBody, SwordplayState.Slash);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);

            if (overlapAttack.isCrit) {
                overlapAttack.damageType |= DamageType.BleedOnHit;
            }
        }

        public override void PlayAnimation()
        {
            string animationStateName = animation;
            bool @bool = animator.GetBool("isMoving");
            bool bool2 = animator.GetBool("isGrounded");
            if (!@bool && bool2)
            {
                PlayCrossfade("FullBody, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
                return;
            }
            PlayCrossfade("Gesture, Additive", animationStateName, "GroundLight.playbackRate", duration, 0.05f);
            PlayCrossfade("Gesture, Override", animationStateName, "GroundLight.playbackRate", duration, 0.05f);

            AkSoundEngine.PostEvent(Events.Play_merc_sword_swing, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}