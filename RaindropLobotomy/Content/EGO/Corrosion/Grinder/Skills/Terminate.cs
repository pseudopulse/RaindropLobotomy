using System;

namespace RaindropLobotomy.EGO.Toolbot {
    public class Terminate : BaseSkillState {
        public static float DamageCoeffPerSecond = 5.5f;
        public static int TickRate = 5;
        //
        private OverlapAttack attack;
        private float freq = 1f / TickRate;
        private float stopwatch = 1f / TickRate;
        private GameObject spinEffect;

        public override void OnEnter()
        {
            base.OnEnter();
            
            freq /= base.attackSpeedStat;
            AkSoundEngine.PostEvent(Events.Play_MULT_m1_sawblade_start, base.gameObject);

            PlayAnimation("Gesture, Additive Gun", "SpinBuzzsaw");
			PlayAnimation("Gesture, Additive", "EnterBuzzsaw");

            attack = new();
            attack.attacker = base.gameObject;
            attack.damage = base.damageStat * (DamageCoeffPerSecond / TickRate);
            attack.hitBoxGroup = FindHitBoxGroup("Saw");
            attack.procCoefficient = 1f;
            attack.teamIndex = base.GetTeam();
            attack.isCrit = base.RollCrit();
            UpdateDamageType();

            spinEffect = GameObject.Instantiate(Load<GameObject>("GrinderSaw.prefab"), FindModelChild("MuzzleBuzzsaw"));
            spinEffect.transform.localScale = Vector3.one;
        }

        public void UpdateDamageType() {
            bool res = Util.CheckRoll(5f * base.characterBody.GetBuffCount(Grinder.Charge));

            if (!res) {
                attack.damageType &= ~DamageType.BleedOnHit;
            }
            else {
                attack.damageType |= DamageType.BleedOnHit;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterBody.SetAimTimer(2f);
            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= freq && base.isAuthority) {
                attack.isCrit = base.RollCrit();
                attack.ResetIgnoredHealthComponents();
                UpdateDamageType();

                stopwatch = 0f;

                if (!inputBank.skill1.down) {
                    outer.SetNextStateToMain();
                }
            }

            attack.Fire();
        }

        public override void OnExit()
        {
            base.OnExit();

            AkSoundEngine.PostEvent(Events.Play_MULT_m1_sawblade_stop, base.gameObject);
            
            PlayAnimation("Gesture, Additive Gun", "Empty");
			PlayAnimation("Gesture, Additive", "ExitBuzzsaw");

            if (spinEffect) {
                Destroy(spinEffect);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}