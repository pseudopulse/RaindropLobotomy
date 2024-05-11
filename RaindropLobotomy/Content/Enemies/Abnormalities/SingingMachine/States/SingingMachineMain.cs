using System;

namespace RaindropLobotomy.Enemies.SingingMachine {
    public class SingingMachineMain : GenericCharacterMain {
        public enum SingingMachineLidState {
            Open,
            Closed,
        }

        public SingingMachineLidState lidState = SingingMachineLidState.Closed;
        public Transform hinge;
        public HitBoxGroup shredderGroup;
        public bool disallowLidStateChange = false;
        public OverlapAttack attack;
        private const float xRotOpen = 110;
        private const float hitrate = 16;
        private float stopwatch = 0f;
        private float x = 0f;

        public override void OnEnter()
        {
            base.OnEnter();

            attack = new();
            attack.attacker = base.gameObject;
            attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            attack.teamIndex = TeamIndex.Void;
            attack.damageColorIndex = DamageColorIndex.Bleed;
            attack.damage = base.damageStat * 6f / hitrate;
            attack.procCoefficient = 0.1f;
            attack.hitBoxGroup = FindHitBoxGroup("Shredder");
            attack.isCrit = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!hinge) {
                hinge = GetModelChildLocator().FindChild("Hinge");
                return;
            }

            switch (lidState) {
                case SingingMachineLidState.Open:
                    x += (xRotOpen / 0.2f) * Time.fixedDeltaTime;
                    break;
                case SingingMachineLidState.Closed:
                    x -= (xRotOpen / 0.2f) * Time.fixedDeltaTime;
                    break;
            };

            if (lidState == SingingMachineLidState.Open) {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= (1f / hitrate)) {
                    stopwatch = 0f;
                    attack.ResetIgnoredHealthComponents();
                    attack.Fire();
                }
            }

            x = Mathf.Clamp(x, 0f, xRotOpen);
            hinge.localRotation = Quaternion.Euler(x, 0f, 0f);
        }

        public void UpdateLidState(SingingMachineLidState newState) {
            if (disallowLidStateChange) {
                return;
            }

            this.lidState = newState;
        }
    }
}