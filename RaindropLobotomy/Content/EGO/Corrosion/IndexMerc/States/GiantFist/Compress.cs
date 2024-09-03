using System;

namespace RaindropLobotomy.EGO.Merc {
    public class Compress : BaseSkillState {
        public Animator animator;
        public bool didStrike = false;
        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();
            PlayAnimation("Body", "Compress", "Generic.playbackRate", 2f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator && !didStrike && animator.GetFloat("Hitbox.active") > 0.65f) {
                didStrike = true;
                Explode();
            }

            if (base.fixedAge >= 2f) {
                outer.SetNextStateToMain();
            }
        }

        public void Explode() {
            BlastAttack attack = new();
            attack.position = base.transform.position;
            attack.radius = 7f;
            attack.crit = base.RollCrit();
            attack.baseDamage = base.damageStat * 8f;
            attack.attacker = base.gameObject;
            attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            attack.losType = BlastAttack.LoSType.NearestHit;
            attack.falloffModel = BlastAttack.FalloffModel.None;
            attack.procCoefficient = 1f;
            attack.teamIndex = base.GetTeam();
            attack.baseForce = 400f;

            attack.Fire();

            EffectManager.SpawnEffect(Paths.GameObject.HermitCrabBombExplosion, new EffectData {
                            origin = base.transform.position,
                            scale = 5f
                        }, false);
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}