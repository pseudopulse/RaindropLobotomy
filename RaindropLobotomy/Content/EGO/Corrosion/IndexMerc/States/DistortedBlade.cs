using System;

namespace RaindropLobotomy.EGO.Merc {
    public class DistortedBlade : AimThrowableBase {
        public bool paladinInstalled => base.characterBody.bodyIndex == IndexMerc.IndexPaladinBody;
        public override void OnEnter()
        {
            base.maxDistance = 120f;
            base.arcVisualizerPrefab = Assets.GameObject.BasicThrowableVisualizer;
            base.endpointVisualizerPrefab = Assets.GameObject.HuntressArrowRainIndicator;
            base.endpointVisualizerRadiusScale = 15f;
            base.baseMinimumDuration = 0.15f;
            base.projectilePrefab = Load<GameObject>("CrashingSwordProjectile.prefab");
            base.useGravity = false;

            base.OnEnter();

            if (paladinInstalled) {
                base.PlayAnimation("Gesture, Underride", "ChargeSpell", "Spell.playbackRate", 0.4f);
            }
        }

        public override void FireProjectile()
        {
            FireProjectileInfo info = new();
            info.projectilePrefab = projectilePrefab;
            info.position = currentTrajectoryInfo.hitPoint;
            info.rotation = Quaternion.identity;
            info.damage = base.damageStat * 14f;
            info.owner = base.gameObject;

            ProjectileManager.instance.FireProjectile(info);
            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();

            if (paladinInstalled) {
                base.PlayAnimation("Gesture, Underride", "BufferEmpty");
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}