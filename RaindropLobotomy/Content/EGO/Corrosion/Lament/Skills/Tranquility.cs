using System;

namespace RaindropLobotomy.EGO.Commando {
    public class Tranquility : BaseSkillState {
        public float duration = 5f;
        public float minCancelTime = 1.5f;
        public float damageCoefficient = 4f;
        public float radius = 20f;
        public override void OnEnter()
        {
            base.OnEnter();

            GetModelTransform().GetComponent<CharacterModel>().invisibilityCount++;
            GetModelTransform().GetComponent<HurtBoxGroup>().hurtBoxesDeactivatorCounter++;

            GetComponent<ContactDamage>().enabled = true;

            EffectManager.SpawnEffect(Paths.GameObject.Bandit2SmokeBomb, new EffectData {
                origin = base.transform.position,
                scale = 2f
            }, false);

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            if (NetworkServer.active) {
                base.characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed);
            }

            FindModelChild("Swarm").GetComponent<ParticleSystem>().Play();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration || (base.inputBank.skill3.down && base.fixedAge >= minCancelTime)) {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            GetModelTransform().GetComponent<CharacterModel>().invisibilityCount--;
            GetModelTransform().GetComponent<HurtBoxGroup>().hurtBoxesDeactivatorCounter--;
            FindModelChild("Swarm").GetComponent<ParticleSystem>().Stop();

            GetComponent<ContactDamage>().enabled = false;

            BlastAttack attack = new();
            attack.position = base.transform.position;
            attack.radius = radius;
            attack.baseDamage = base.damageStat * damageCoefficient;
            attack.attacker = base.gameObject;
            attack.teamIndex = GetTeam();
            attack.crit = base.RollCrit();
            attack.falloffModel = BlastAttack.FalloffModel.Linear;
            attack.procCoefficient = 1;
            attack.AddModdedDamageType(SolemnLament.Seal);
            
            if (base.isAuthority) attack.Fire();

            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            EffectManager.SpawnEffect(Paths.GameObject.Bandit2SmokeBomb, new EffectData {
                origin = base.transform.position,
                scale = 2f
            }, false);

            if (NetworkServer.active) {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}