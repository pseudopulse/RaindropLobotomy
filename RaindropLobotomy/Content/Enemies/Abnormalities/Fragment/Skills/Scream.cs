using System;

namespace RaindropLobotomy.Enemies.Fragment {
    public class Scream : BaseSkillState {
        private float duration = 10f;
        private int pulses = 7;
        private float damageCoeff = 4f;
        private float pulseDelay => duration / pulses;
        private Timer pulse;
        
        public override void OnEnter()
        {
            base.OnEnter();

            pulse = new(pulseDelay, true);

            PlayAnimation("Gesture, Override", "Echo", "Echo.playbackRate", duration);
            GetModelAnimator().SetBool("isScreaming", true);

            AkSoundEngine.PostEvent("Play_fragment_scream", base.gameObject);

            base.characterBody.rootMotionInMainState = true;
            base.characterBody.statsDirty = true;
            base.characterDirection.enabled = false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }

            if (pulse.Tick()) {
                pulse.Reset();

                Vector3 pos = FindModelChild("Flower").position;

                EffectManager.SpawnEffect(UniverseFragment.ScreamEffect, new EffectData {
                    origin = pos,
                    scale = 3f
                }, false);

                BlastAttack attack = new();
                attack.position = pos;
                attack.radius = 15f;
                attack.crit = base.RollCrit();
                attack.baseDamage = base.damageStat * damageCoeff;
                attack.attacker = base.gameObject;
                attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                attack.losType = BlastAttack.LoSType.NearestHit;
                attack.falloffModel = BlastAttack.FalloffModel.None;
                attack.procCoefficient = 1f;
                attack.teamIndex = TeamIndex.Neutral;
                attack.damageType = DamageType.SlowOnHit;
                attack.baseForce = 40f;

                attack.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterBody.rootMotionInMainState = false;
            base.characterBody.statsDirty = true;
            base.characterDirection.enabled = true;

            AkSoundEngine.PostEvent("Stop_fragment_scream", base.gameObject);
            GetModelAnimator().SetBool("isScreaming", false);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}