using System;

namespace RaindropLobotomy.Ordeals.Noon.Green {
    public class DefensiveStance : BaseSkillState {
        public float duration = 20f;
        private HurtBox[] boxes;

        public override void OnEnter()
        {
            base.OnEnter();

            GetModelAnimator().SetBool("isDefensive", true);
            GetModelAnimator().SetBool("isFiring", false);
            GetModelAnimator().SetBool("isSawing", true);

            EntityStateMachine.FindByCustomName(base.gameObject, "Weapon").SetNextStateToMain();

            skillLocator.primary.Reset();

            PlayAnimation("Body", "EnterDefensive", "Standard.playbackRate", 1.3f);

            boxes = modelLocator.modelTransform.GetComponent<HurtBoxGroup>().hurtBoxes;

            for (int i = 0; i < boxes.Length; i++) {
                if (i == 6) continue;
                boxes[i].damageModifier = HurtBox.DamageModifier.Barrier;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= duration) {
                outer.SetNextStateToMain();
            }

            PerformInputs();
        }

        public override void OnExit()
        {
            base.OnExit();

            GetModelAnimator().SetBool("isDefensive", false);

            EntityStateMachine.FindByCustomName(base.gameObject, "Weapon").SetNextStateToMain();

            for (int i = 0; i < boxes.Length; i++) {
                if (i == 6) continue;
                boxes[i].damageModifier = HurtBox.DamageModifier.Normal;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        protected void PerformInputs()
        {
            if (base.isAuthority)
            {
                if (skillLocator)
                {
                    HandleSkill(base.skillLocator.primary, ref base.inputBank.skill1);
                    HandleSkill(base.skillLocator.secondary, ref base.inputBank.skill2);
                    HandleSkill(base.skillLocator.utility, ref base.inputBank.skill3);
                }
                
            }
            void HandleSkill(GenericSkill skillSlot, ref InputBankTest.ButtonState buttonState)
            {
                if (buttonState.down && (bool)skillSlot && (!skillSlot.mustKeyPress || !buttonState.hasPressBeenClaimed) && skillSlot.ExecuteIfReady())
                {
                    buttonState.hasPressBeenClaimed = true;
                }
            }
        }
    }
}