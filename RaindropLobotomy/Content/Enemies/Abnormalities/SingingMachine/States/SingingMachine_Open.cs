using System;

namespace RaindropLobotomy.Enemies.SingingMachine {
    public class SingingMachine_Open : BaseState {
        public override void OnEnter()
        {
            base.OnEnter();
            // Debug.Log("opening");
            AkSoundEngine.PostEvent(Events.Play_MULT_m1_sawblade_start, base.gameObject);
            (EntityStateMachine.FindByCustomName(gameObject, "Body").state as SingingMachineMain).UpdateLidState(SingingMachineMain.SingingMachineLidState.Open);
            base.skillLocator.primary.UnsetSkillOverride(this, Load<SkillDef>("SM_Open.asset"), GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.primary.SetSkillOverride(this, Load<SkillDef>("SM_Close.asset"), GenericSkill.SkillOverridePriority.Contextual);
            outer.SetNextStateToMain();
        }
    }
}