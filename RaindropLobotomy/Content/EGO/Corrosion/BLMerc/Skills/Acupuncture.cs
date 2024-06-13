using System;
using System.Text;

namespace RaindropLobotomy.EGO.Merc {
    public class Acupuncture : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 0.5f;

        public override float DamageCoefficient => 4f;

        public override string HitboxName => "WhirlwindGround";

        public override GameObject HitEffectPrefab => Assets.GameObject.ImpactMercSwing;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.05f;

        public override GameObject SwingEffectPrefab => BLMerc.Spear;

        public override string MuzzleString => "Thrust";
        private Vector3 vector;

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterMotor.Motor.ForceUnground();
            base.characterDirection.forward = base.inputBank.aimDirection;
            vector = base.inputBank.aimDirection;

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            BLMerc.UpdateSwordplayState(base.characterBody, SwordplayState.Thrust);
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
            
            if (overlapAttack.isCrit) {
                overlapAttack.AddModdedDamageType(BLMerc.PoiseDamageBonus);
            }
        }

        public override void PlayAnimation()
        {
            string animationStateName = "GroundLight1";
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

        public override void OnExit()
        {
            base.OnExit();

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.isSprinting = true;

            base.characterDirection.forward = vector;
            base.characterMotor.velocity = vector * 32f;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}