using System;

namespace RaindropLobotomy.EGO.Mage {
    public class Illuminate : AimThrowableBase {
        public override void OnEnter()
        {
            base.maxDistance = 70f;
            base.arcVisualizerPrefab = Paths.GameObject.BasicThrowableVisualizer;
            base.endpointVisualizerPrefab = Paths.GameObject.TreebotMortarAreaIndicator;
            base.endpointVisualizerRadiusScale = 4f;
            base.baseMinimumDuration = 0.15f;
            base.projectilePrefab = EGOLamp.LampAreaProjectile;
            base.useGravity = true;

            base.OnEnter();

            PlayAnimation("Gesture, Additive", "ChargeNovaBomb", "ChargeNovaBomb.playbackRate", 2f);
        }

        public override void FireProjectile()
        {
            FireProjectileInfo info = new();
            info.projectilePrefab = projectilePrefab;
            info.position = currentTrajectoryInfo.hitPoint;
            info.rotation = Quaternion.identity;
            info.damage = base.damageStat * 2f;
            info.owner = base.gameObject;
            
            EffectManager.SimpleEffect(Paths.GameObject.ExplosionSolarFlare, info.position, Quaternion.identity, true);

            ProjectileManager.instance.FireProjectile(info);
            outer.SetNextStateToMain();
        }

        public override void UpdateTrajectoryInfo(out TrajectoryInfo dest)
        {
            base.UpdateTrajectoryInfo(out dest);

            if (Physics.Raycast(dest.hitPoint + Vector3.up, Vector3.down, out RaycastHit info, 4000f, LayerIndex.world.mask)) {
                dest.hitPoint = info.point;
                dest.hitNormal = info.normal;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            
            PlayAnimation("Gesture, Additive", "FireNovaBomb", "ChargeNovaBomb.playbackRate", 2f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}