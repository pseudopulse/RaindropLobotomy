using System;

namespace RaindropLobotomy.Ordeals.Midnight.Green {
    public class SpawnState : BaseState {
        private Transform leftShell;
        private Transform rightShell;
        private float targetY = -1.45f;
        private float Y = 0f;
        private bool begun = false;

        public override void OnEnter()
        {
            base.OnEnter();
            leftShell = GetModelChildLocator().FindChild("Rod1");
            rightShell = GetModelChildLocator().FindChild("Rod2");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= 6.4f && !begun) {
                begun = true;
                base.fixedAge = 0f;
            }

            if (begun && Y > targetY) {
                Y += (targetY / 2f) * Time.fixedDeltaTime;
                leftShell.transform.localPosition = new(0f, Y, 0f);
                rightShell.transform.localPosition = new(0f, Y * -1f, 0f);
            }
    
            if (base.fixedAge >= 3f && begun) {
                outer.SetNextState(new BeamState());
                EntityStateMachine.FindByCustomName(base.gameObject, "Weapon").SetNextState(new SpinState());
                return;
            }
        }

        private float Lerp(float v, float target, float time) {
            return Mathf.Lerp(v, target, time);
        }
    }
}