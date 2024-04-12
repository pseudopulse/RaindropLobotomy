using System;

namespace RaindropLobotomy.Enemies.SteamMachine {
    public class Slam : BaseState {
        public Transform clawMuzzle;
        public bool performedSlam = false;
        private Vector3 forward;
        private OverlapAttack attack;
        public override void OnEnter()
        {
            base.OnEnter();
            clawMuzzle = FindModelChild("Claw");
            PlayAnimation("Gesture, Override", "Slash", "Slam.playbackRate", 1f);
            attack = new();
            attack.damage = base.damageStat * 5f;
            attack.attacker = base.gameObject;
            attack.hitBoxGroup = FindHitBoxGroup("SlashHitbox");
            attack.teamIndex = GetTeam();
            attack.isCrit = RollCrit();
            attack.procCoefficient = 1f;
            attack.pushAwayForce = 2000f;
            attack.forceVector = Vector3.up;

            base.characterBody.SetAimTimer(0.3f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= 0.4f) {
                attack.Fire();
            }

            if (base.fixedAge >= 2f) {
                outer.SetNextStateToMain();
            }
        }
    }
}