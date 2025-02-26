using System;

namespace RaindropLobotomy.Enemies.FHR {
    public class Spray : BaseSkillState {
        public float DamagePerGlob = 3f;
        public float GlobSpread = 25f;
        public int GlobsPerSecond = 30;
        private float globDelay;
        private Animator anim;
        private float stopwatch = 0f;
        private Transform muzzle;
        private float duration = 2f;

        public override void OnEnter()
        {
            base.OnEnter();

            base.StartAimMode(0.2f);

            globDelay = 1f / GlobsPerSecond;
            globDelay /= base.attackSpeedStat;

            PlayAnimation("Fullbody, Override", "Spray", "Generic.playbackRate", duration);

            anim = GetModelAnimator();

            muzzle = FindModelChild("Flower");
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (anim.GetFloat("Hitbox.active") > 0f) {
                stopwatch += Time.fixedDeltaTime;

                if (stopwatch >= globDelay) {
                    stopwatch = 0f;

                    FireGlob();
                }
            }
            else {
                base.StartAimMode(0.2f);
            }

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public void FireGlob() {
            // EffectManager.SimpleMuzzleFlash(FHR.MuzzleFlashBloodShot, base.gameObject, "Flower", false);
            AkSoundEngine.PostEvent(Events.Play_beetle_queen_attack2_shoot, base.gameObject);

            if (!base.isAuthority) return;

            FireProjectileInfo info = new();
            info.projectilePrefab = FHR.BloodShot;
            info.damage = base.damageStat * DamagePerGlob;
            info.crit = base.RollCrit();
            info.position = muzzle.position;
            info.speedOverride = Random.Range(50f, 120f);
            info.rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(muzzle.up, -1f, 1f, GlobSpread, GlobSpread));
            info.owner = base.gameObject;

            ProjectileManager.instance.FireProjectile(info);
        }
    }
}