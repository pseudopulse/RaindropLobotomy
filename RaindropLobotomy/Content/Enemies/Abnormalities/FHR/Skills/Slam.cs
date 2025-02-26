using System;

namespace RaindropLobotomy.Enemies.FHR {
    public class Slam : CoolerBasicMeleeAttack
    {
        public override float BaseDuration => 2f;

        public override float DamageCoefficient => 5f;

        public override string HitboxName => "Slam";

        public override GameObject HitEffectPrefab => null;

        public override float ProcCoefficient => 1f;

        public override float HitPauseDuration => 0.001f;

        public override GameObject SwingEffectPrefab => null;

        public override string MuzzleString => "";
        public override string MechanimHitboxParameter => "Hitbox.active";
        private bool didSlamEffect = false;
        private static float vineArc = 170f;
        private static int totalVines = 6;

        public override void OnEnter()
        {
            base.OnEnter();

            base.StartAimMode(0.2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (!didSlamEffect && animator.GetFloat(MechanimHitboxParameter) > 0.5f) {
                didSlamEffect = true;

                ProcessSlamAuthority();
            }
        }

        public void ProcessSlamAuthority() {
            if (!base.isAuthority) return;

            Vector3 point = FindModelChild("SlamPoint").transform.position;

            EffectManager.SpawnEffect(Paths.GameObject.ExplosionGolemDeath, new EffectData {
                scale = 9f,
                origin = point
            }, true);

            float num = vineArc / totalVines;
            Vector3 vec = Vector3.ProjectOnPlane(base.characterDirection.forward, Vector3.up);
            Vector3 pos = point;

            for (int i = 0; i < totalVines; i++) {
                Vector3 forward = Quaternion.AngleAxis(num * ((float)i - ((float)totalVines / 2f)), Vector3.up) * vec;

                FireProjectileInfo info = new();
                info.damage = base.damageStat * base.damageCoefficient;
                info.position = pos;
                info.rotation = Util.QuaternionSafeLookRotation(forward);
                info.crit = base.RollCrit();
                info.owner = base.gameObject;
                info.projectilePrefab = FHR.VineProjectile;

                ProjectileManager.instance.FireProjectile(info);
            }
        }

        public override void BeginMeleeAttackEffect()
        {
            base.BeginMeleeAttackEffect();
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            base.AuthorityModifyOverlapAttack(overlapAttack);
        }

        public override void PlayAnimation()
        {
            PlayAnimation("Fullbody, Override", "Slam", "Generic.playbackRate", duration);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}