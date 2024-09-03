using System;

namespace RaindropLobotomy.EGO.Merc {
    public class DistortedBlade : AimThrowableBase {
        public bool paladinInstalled => base.characterBody.bodyIndex == IndexMerc.IndexPaladinBody;
        public override void OnEnter()
        {
            base.maxDistance = 120f;
            base.arcVisualizerPrefab = Paths.GameObject.BasicThrowableVisualizer;
            base.endpointVisualizerPrefab = Paths.GameObject.TreebotMortarAreaIndicator;
            base.endpointVisualizerRadiusScale = 15f;
            base.baseMinimumDuration = 0.15f;
            base.projectilePrefab = Load<GameObject>("CrashingSwordProjectile.prefab");
            base.useGravity = true;

            base.OnEnter();

            if (paladinInstalled) {
                base.PlayAnimation("Gesture, Override", "ChargeSpell", "Spell.playbackRate", 0.4f);
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

            if (paladinInstalled) {
                base.PlayAnimation("Gesture, Override", "CastSpell", "Spell.playbackRate", 0.5f);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}