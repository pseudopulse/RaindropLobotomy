using System;
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy.EGO.Bandit {
    public class SilentAdvance : AimThrowableBase {
        public override void OnEnter()
        {
            base.maxDistance = 40f;
            base.arcVisualizerPrefab = Assets.GameObject.BasicThrowableVisualizer;
            base.endpointVisualizerPrefab = Assets.GameObject.HuntressArrowRainIndicator;
            base.endpointVisualizerRadiusScale = 2f;
            base.baseMinimumDuration = 0.15f;
            base.projectilePrefab = Assets.GameObject.Fireball; // the game expects this to be set
            AkSoundEngine.PostEvent(Events.Play_bandit2_shift_enter, base.gameObject);
            base.OnEnter();
        }

        public override void FireProjectile()
        {
            if (!isAuthority) {
                return;
            }

            EffectManager.SimpleEffect(EGOMagicBullet.TeleportEffect, base.transform.position, Quaternion.identity, true);
            TeleportHelper.TeleportBody(base.characterBody, base.currentTrajectoryInfo.hitPoint);
            EffectManager.SimpleEffect(EGOMagicBullet.TeleportEffect, base.transform.position, Quaternion.identity, true);

            AkSoundEngine.PostEvent(Events.Play_bandit2_shift_exit, base.gameObject);

            BlastAttack attack = new();
            attack.attacker = base.gameObject;
            attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            attack.baseDamage = base.damageStat * 3f;
            attack.crit = base.RollCrit();
            attack.AddModdedDamageType(DarkFlame.Instance.DarkFlameDamageType);
            attack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
            attack.radius = 5f;
            attack.position = base.currentTrajectoryInfo.hitPoint;
            attack.teamIndex = base.GetTeam();
            attack.procCoefficient = 1f;

            attack.Fire();
        }
    }
}