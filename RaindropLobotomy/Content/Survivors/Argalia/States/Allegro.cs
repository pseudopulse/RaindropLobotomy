using System;

namespace RaindropLobotomy.Survivors.Argalia {
    public class Allegro : BaseSkillState {
        public static float DamageCoefficient = 2.2f;
        public static float Duration = 0.4f;
        public static string Muzzle = "MuzzleHorizontalSlash";
        //
        private OverlapAttack attack;
        private GameObject effect;
        public override void OnEnter()
        {
            attack = new();
            attack.attacker = base.gameObject;
            attack.damage = base.damageStat * DamageCoefficient;
            attack.hitBoxGroup = FindHitBoxGroup("HorizontalSlash");
            attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
            attack.procCoefficient = 1f;
            attack.procChainMask = default;
            attack.teamIndex = GetTeam();
            attack.isCrit = base.RollCrit();
            attack.hitEffectPrefab = Assets.GameObject.ImpactMercSwing;
            attack.AddModdedDamageType(Argalia.ResonanceDamageType);

            AkSoundEngine.PostEvent(Events.Play_merc_sword_swing, base.gameObject);

            effect = SpawnMeleeEffect(Assets.GameObject.MercSwordFinisherSlash, Muzzle);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            attack.Fire();

            if (base.fixedAge >= Duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public GameObject SpawnMeleeEffect(GameObject prefab, string muzzle) {
            GameObject swingEffectInstance = null;
            Transform transform = FindModelChild(muzzle);
            if (transform)
            {
                swingEffectInstance = Object.Instantiate(prefab, transform);
                ScaleParticleSystemDuration component = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                if (component)
                {
                    component.newDuration = component.initialDuration;
                }
            }
            return swingEffectInstance;
        }
    }
}