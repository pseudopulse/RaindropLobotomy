using System;
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy.EGO.FalseSon {
    public class Silence : BaseSkillState {
        public float duration = 0.135f;
        public float prepDuration = 0.1f;
        public float buffDuration = 2.5f;
        public Vector3 dashVector;
        public float distance = 20f;
        public Vector3 velocity;
        public bool isDashing = false;

        public override void OnEnter()
        {
            base.OnEnter();

            dashVector = base.characterMotor.moveDirection;
            if (dashVector == Vector3.zero) dashVector = base.inputBank.aimDirection.Nullify(false, true, false);
            velocity = dashVector * (distance / duration);

            PlayAnimation("FullBody, Override", "StepBrothersPrep", "StepBrothersPrep.playbackRate", prepDuration);

            AkSoundEngine.PostEvent(Events.Play_falseson_skill3_dash, base.gameObject);

            if (NetworkServer.active) {
                base.characterBody.AddTimedBuff(FeatherGuard.BuffIndex, buffDuration);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isDashing) {
                base.characterMotor.velocity = velocity;
                base.characterDirection.forward = dashVector;
            }

            if (base.fixedAge >= prepDuration && !isDashing) {
                isDashing = true;
                PlayCrossfade("FullBody, Override", "StepBrothersLoop", 0.1f);

                GameObject.Instantiate(Paths.GameObject.FalseSonDashTrail, FindModelChild("DashCenter"));
            }

            if (base.fixedAge >= duration + prepDuration) {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            PlayAnimation("FullBody, Override", "StepBrothersLoopExit");

            base.characterMotor.velocity = Vector3.zero;
        }
    }
}