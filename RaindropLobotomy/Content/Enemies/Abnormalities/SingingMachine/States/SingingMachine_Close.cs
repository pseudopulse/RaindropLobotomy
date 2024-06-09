using System;

namespace RaindropLobotomy.Enemies.SingingMachine {
    public class SingingMachine_Close : BaseState {
        public override void OnEnter()
        {
            base.OnEnter();
            // Debug.Log("closing");
            AkSoundEngine.PostEvent(Events.Play_MULT_m1_sawblade_stop, base.gameObject);
            (EntityStateMachine.FindByCustomName(gameObject, "Body").state as SingingMachineMain).UpdateLidState(SingingMachineMain.SingingMachineLidState.Closed);
            base.skillLocator.primary.UnsetSkillOverride(this, Load<SkillDef>("SM_Close.asset"), GenericSkill.SkillOverridePriority.Contextual);
            base.skillLocator.primary.SetSkillOverride(this, Load<SkillDef>("SM_Open.asset"), GenericSkill.SkillOverridePriority.Contextual);
            outer.SetNextStateToMain();
        }
    }
}