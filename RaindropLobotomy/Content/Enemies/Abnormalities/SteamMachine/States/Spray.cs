using System;

namespace RaindropLobotomy.Enemies.SteamMachine {
    public class Spray : BaseState {
        private GameObject sprayInstance;
        private Transform muzzle;
        private Timer sprayTimer = new(0.2f, false, true, false, true);
        private bool started = false;
        private Animator animator;
        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Gesture, Override", "Spray", "Spray.playbackRate", 8f);
            muzzle = FindModelChild("Nozzle");
            animator = GetModelAnimator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterBody.SetAimTimer(1f);

            if (!started && animator.GetFloat("sprayBegun") >= 0.5f) {
                FindModelChild("Spray").GetComponent<ParticleSystem>().Play();
                started = true;
            }

            if (animator.GetFloat("sprayBegun") <= 0.2f && started) {
                outer.SetNextStateToMain();
                return;
            }


            if (sprayTimer.Tick() && started && base.isAuthority) {
                BulletAttack attack = new();
                attack.damage = base.damageStat * 4f * sprayTimer.duration;
                attack.aimVector = -muzzle.transform.forward;
                attack.weapon = muzzle.gameObject;
                attack.owner = base.gameObject;
                attack.falloffModel = BulletAttack.FalloffModel.None;
                attack.isCrit = base.RollCrit();
                attack.stopperMask = LayerIndex.world.mask;
                attack.origin = muzzle.transform.position;
                attack.procCoefficient = 1f;
                attack.radius = 1f;
                attack.smartCollision = true;
                attack.maxDistance = 35f;
                attack.muzzleName = "Nozzle";
                
                attack.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            FindModelChild("Spray").GetComponent<ParticleSystem>().Stop();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}