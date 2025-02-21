using System;

namespace RaindropLobotomy.EGO.Mage {
    public class Darkness : BaseSkillState {
        private bool playedAnim = false;
        public override void OnEnter()
        {
            base.OnEnter();

            PlayAnimation("Gesture, Additive", "ChargeNovaBomb", "ChargeNovaBomb.playbackRate", 2f);

            EGOLamp.DC.TriggerDarkness();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= 15f) {
                outer.SetNextStateToMain();
            }

            if (base.fixedAge >= 2f && !playedAnim) {
                playedAnim = true;
                PlayAnimation("Gesture, Additive", "FireNovaBomb", "ChargeNovaBomb.playbackRate", 1f);
            }
        }
    }
}