using System;
using RaindropLobotomy.Buffs;
using RoR2.Orbs;

namespace RaindropLobotomy.EGO.FalseSon {
    public class CeaselessJudgement : BaseSkillState {
        public float baseDuration = 1.4f;
        public float damageCoeff = 5f;
        public bool hasFired = false;
        public override void OnEnter()
        {
            base.OnEnter();

            baseDuration /= base.attackSpeedStat;
            PlayAnimation("FullBody, Override", "OverheadSwing", "ChargeSwing.playbackRate", baseDuration);
            
            base.StartAimMode(baseDuration * 0.5f);

            AkSoundEngine.PostEvent(Events.Play_boss_falseson_skill1_swing, base.gameObject);

            if (NetworkServer.active) {
                ProcessExecutions();

                DamageInfo damage = new();
                damage.damage = base.healthComponent.fullCombinedHealth * 0.25f;
                damage.position = base.characterBody.corePosition;
                damage.damageType = DamageType.NonLethal | DamageType.BypassBlock | DamageType.BypassArmor;

                base.healthComponent.TakeDamage(damage);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= baseDuration * 0.35f && !hasFired) {
                hasFired = true;

                Vector3 basePos = FindModelChild("ClubExplosionPoint").transform.position;
                AkSoundEngine.PostEvent(Events.Play_boss_falseson_skill1_swing_impact, base.gameObject);
                EffectManager.SpawnEffect(Paths.GameObject.FalseSonMeteorGroundImpact, new EffectData {
                    origin = basePos,
                    scale = 0.8f
                }, false);
            }

            if (base.fixedAge >= baseDuration) {
                outer.SetNextStateToMain();
            }
        }

        public void ProcessExecutions() {

            if (!NetworkServer.active) return;

            SphereSearch search = new();
            search.origin = base.transform.position;
            search.radius = 60f;
            search.mask = LayerIndex.entityPrecise.mask;
            search.RefreshCandidates();
            search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(base.GetTeam()));
            search.FilterCandidatesByDistinctHurtBoxEntities();
            
            HurtBox[] boxes = search.GetHurtBoxes();

            for (int i = 0; i < boxes.Length; i++) {
                HurtBox box = boxes[i];

                if (box.teamIndex == TeamIndex.Player) continue;

                LightningStrikeOrb orb = new();
                orb.target = box;
                orb.damageValue = base.damageStat * damageCoeff;
                orb.procChainMask = default;
                orb.procChainMask.AddProc(ProcType.PlasmaCore);
                orb.isCrit = base.RollCrit();
                orb.scale = 3f;
                orb.procCoefficient = 1f;
                orb.damageType |= DamageType.Stun1s;
                orb.attacker = base.gameObject;

                OrbManager.instance.AddOrb(orb);

                AkSoundEngine.PostEvent(Events.Play_item_use_lighningArm, box.gameObject);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}