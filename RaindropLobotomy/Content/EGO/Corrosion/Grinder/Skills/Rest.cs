using System;

namespace RaindropLobotomy.EGO.Toolbot {
    public class Rest : GenericCharacterMain {
        public Animator animBlades;
        public float regenPerSecond = 7.5f;
        public float cooldownAcceleration = 1f;
        public float chargeDelay = 0.5f;
        private float[] stopwatches = { 0f, 0f };
        public override void OnEnter()
        {
            base.OnEnter();

            PlayCrossfade("Body", "BoxModeEnter", 0.1f);
		    PlayCrossfade("Stance, Override", "PutAwayGun", 0.1f);

            base.GetModelAnimator().SetFloat("aimWeight", 0f);

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
            }

            animBlades = FindModelChild("Blades").GetComponent<Animator>();
            animBlades.SetBool("boxed", true);
            PlayAnimationOnAnimator(animBlades, "Weapon", "BoxUp", "Generic.playbackRate", 1f);

            AkSoundEngine.PostEvent(Events.Play_MULT_shift_start, base.gameObject);

            base.modelLocator.normalizeToFloor = true;
        }

        public override void ProcessJump()
        {
            return;
        }

        public override bool CanExecuteSkill(GenericSkill skillSlot)
        {
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.isSprinting = false;

            stopwatches[0] += Time.fixedDeltaTime;
            stopwatches[1] += Time.fixedDeltaTime;

            if (stopwatches[0] >= 0.33f) {
                base.healthComponent.Heal(base.healthComponent.health * ((regenPerSecond / 3) * 0.01f), default);
                stopwatches[0] = 0f;
            }

            if (stopwatches[1] >= chargeDelay) {
                Grinder.IncreaseCharge(base.characterBody);
                stopwatches[1] = 0f;
            }

            UpdateSkill(base.skillLocator.primary, Time.fixedDeltaTime);
            UpdateSkill(base.skillLocator.secondary, Time.fixedDeltaTime);
            UpdateSkill(base.skillLocator.special, Time.fixedDeltaTime);

            if (base.fixedAge >= 1f && inputBank.skill3.down) {
                outer.SetNextStateToMain();
            }

            else if (base.fixedAge >= 1f && inputBank.jump.down) {
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = Vector3.up * 30f;
                outer.SetNextStateToMain();
            }
        }

        public void UpdateSkill(GenericSkill slot, float delta) {
            if (slot.stock < slot.maxStock) {
                slot.rechargeStopwatch += delta * cooldownAcceleration;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            animBlades.SetBool("boxed", false);

            PlayAnimation("Body", "BoxModeExit");
			PlayCrossfade("Stance, Override", "Empty", 0.1f);
			
			if (NetworkServer.active)
			{
				base.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
			}

            base.GetModelAnimator().SetFloat("aimWeight", 1f);

            base.modelLocator.normalizeToFloor = false;
            
            AkSoundEngine.PostEvent(Events.Play_MULT_shift_end, base.gameObject);

            EffectManager.SimpleEffect(Paths.GameObject.Bandit2SmokeBombMini, base.transform.position, Quaternion.identity, false);
        }
    }
}