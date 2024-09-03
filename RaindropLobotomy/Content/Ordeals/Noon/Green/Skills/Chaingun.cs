using System;

namespace RaindropLobotomy.Ordeals.Noon.Green {
    public class Chaingun : BaseSkillState {
        public float duration = 6f;
        public float startDelay = 1f;
        private float stopwatch = 0f;
        private int hitRate = 5;
        private float damageCoefficient = 1f;
        private float procCoeffPerHit;
        private float procCoeffPerSecond = 1f;
        private float delay;
        private bool defensive;

        public override void OnEnter()
        {
            base.OnEnter();

            defensive = GetModelAnimator().GetBool("isDefensive");

            GetModelAnimator().SetBool("isFiring", true);
            PlayAnimation("Gesture, Override", defensive ? "Defensive Fire" : "Fire", "Standard.playbackRate", startDelay);

            hitRate = Mathf.CeilToInt(hitRate * base.attackSpeedStat);

            if (defensive) hitRate *= 5;

            procCoeffPerHit = procCoeffPerSecond / hitRate;
            delay = 1f / hitRate;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!defensive) {
                base.StartAimMode();
            }
            else {
                base.StartAimMode(10f);
            }

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
                return;
            }
 

            if (base.fixedAge >= startDelay) {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= delay) {
                    stopwatch = 0f;

                    FireBulletAuthority();
                }
            }
        }

        public void FireBulletAuthority() {
            if (!isAuthority) return;

            Transform muzzle = FindModelChild("MuzzleCannon");

            BulletAttack bulletAttack = new();
            bulletAttack.owner = base.gameObject;
            bulletAttack.weapon = base.gameObject;
            bulletAttack.origin = muzzle.position;
            bulletAttack.aimVector = -muzzle.forward;
            bulletAttack.minSpread = 4f;
            bulletAttack.maxSpread = 9f;
            bulletAttack.damage = base.damageStat;
            bulletAttack.force = 40f;
            bulletAttack.tracerEffectPrefab = Paths.GameObject.TracerCommandoBoost;
            bulletAttack.muzzleName = "MuzzleCannon";
            bulletAttack.hitEffectPrefab = Paths.GameObject.OmniImpactVFX;
            bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
            bulletAttack.radius = 0.2f;
            bulletAttack.smartCollision = true;

            EffectManager.SimpleMuzzleFlash(Paths.GameObject.MuzzleflashBarrage, base.gameObject, "MuzzleCannon", true);

            AkSoundEngine.PostEvent(Events.Play_commando_R, base.gameObject);

            bulletAttack.Fire();
        }

        public override void OnExit()
        {
            base.OnExit();
            GetModelAnimator().SetBool("isFiring", false);
            base.StartAimMode(2f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}