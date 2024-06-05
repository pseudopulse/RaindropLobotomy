using System;

namespace RaindropLobotomy.Survivors.Sweeper {
    public class RallyAllies : BaseSkillState {
        public float duration = 1.45f;
        public float radius = 45f;
        public bool rallied = false;
        public Animator animator;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Override", "Rally", "Generic.playbackRate", duration);
            animator = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterMotor.velocity = base.characterMotor.velocity.Nullify(true, false, true);

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }

            if (!rallied && animator.GetFloat("rallyBegun") > 0.5f) {
                rallied = true;
                Rally();
            }
        }

        public void Rally() {
            List<CharacterBody> bodies = Sweeper.GetSweepersInRange(radius, base.transform.position);

            for (int i = 0; i < bodies.Count; i++) {
                for (int j = 0; j < bodies.Count; j++) {
                    bodies[i].AddTimedBuff(Buffs.Persistence.Instance.Buff, 10f);
                }
            }

            AkSoundEngine.PostEvent(Events.Play_item_use_gainArmor, base.gameObject);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}