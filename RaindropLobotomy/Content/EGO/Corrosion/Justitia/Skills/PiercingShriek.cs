using System;
using System.Collections;

namespace RaindropLobotomy.EGO.FalseSon {
    public class PiercingShriek : BaseSkillState {
        public float damageCoeff = 4f;
        public float range = 40f;
        public float angle = 70f;
        public float duration = 0.5f;

        public override void OnEnter()
        {
            base.OnEnter();

            PlayCrossfade("Gesture, Head, Override", "FireLaserLoop", 0.25f);
		    PlayCrossfade("Gesture, Head, Additive", "FireLaserLoop", 0.25f);

            GameObject.Instantiate(EGOJustitia.JustitiaShriek, base.inputBank.aimOrigin + (base.inputBank.aimDirection), Quaternion.LookRotation(base.inputBank.aimDirection));

            AkSoundEngine.PostEvent(Events.Play_beetle_worker_death, base.gameObject);
            AkSoundEngine.PostEvent(Events.Play_beetle_worker_death, base.gameObject);

            duration /= base.attackSpeedStat;

            base.StartAimMode(duration);

            if (NetworkServer.active) {
                BullseyeSearch search = new();
                search.maxDistanceFilter = range;
                search.maxAngleFilter = angle;
                search.filterByDistinctEntity = true;
                search.searchOrigin = base.GetAimRay().origin;
                search.searchDirection = base.GetAimRay().direction;
                search.teamMaskFilter = TeamMask.GetUnprotectedTeams(base.GetTeam());
                search.filterByLoS = true;
                search.RefreshCandidates();

                IEnumerable<HurtBox> hurtBoxes = search.GetResults();
                
                foreach (HurtBox box in hurtBoxes) {
                    if (box.healthComponent) {
                        CharacterBody body = box.healthComponent.body;
                        int sin = body.GetBuffCount(Buffs.Sin.BuffIndex);
                        body.SetBuffCount(Buffs.Sin.BuffIndex, sin + 3);

                        DamageInfo damage = new();
                        damage.attacker = base.gameObject;
                        damage.crit = base.RollCrit();
                        damage.position = box.transform.position;
                        damage.damage = base.damageStat * damageCoeff;
                        damage.procCoefficient = 1;
                        
                        box.healthComponent.TakeDamage(damage);
                        GlobalEventManager.instance.OnHitAll(damage, body.gameObject);
                        GlobalEventManager.instance.OnHitEnemy(damage, body.gameObject);
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnExit()
        {
            base.OnExit();

            PlayAnimation("Gesture, Head, Override", "FireLaserLoopEnd");
		    PlayAnimation("Gesture, Head, Additive", "FireLaserLoopEnd");
        }
    }
}