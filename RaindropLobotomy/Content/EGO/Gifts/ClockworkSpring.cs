using System;

namespace RaindropLobotomy.EGO.Gifts {
    public class ClockworkSpring : EGOGiftBase<ClockworkSpring>
    {
        public override ItemDef ItemDef => null;

        public override EquipmentDef EquipmentDef => Load<EquipmentDef>("edClockworkSpring.asset");

        public override void Hooks()
        {
            
        }

        public override void SetupLanguage()
        {
            LanguageAPI.Add("RL_GIFT_CLOCKWORKSPRING_NAME", "Clockwork Spring");
            LanguageAPI.Add("RL_GIFT_CLOCKWORKSPRING_DESC", "Passively collect <style=cIsUtility>time</style>, and activate to gain <style=cIsUtility>temporary attack, movement, and cooldown speed</style> based on stored time.");
            LanguageAPI.Add("RL_GIFT_CLOCKWORKSPRING_PICKUP", "Store time and unwind it for a temporary boost to attack, movement, and cooldown speed.");
        }

        public override bool ActivateEquipment(EquipmentSlot slot)
        {
            return base.ActivateEquipment(slot);
        }
    }
}