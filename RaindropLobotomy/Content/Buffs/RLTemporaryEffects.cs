using System;
using RaindropLobotomy.Buffs;

namespace RaindropLobotomy {
    public class RLTemporaryEffects : MonoBehaviour {
        public TemporaryVisualEffect FeatherShield;
        public void UpdateTemporaryEffects(CharacterBody body) {
            body.UpdateSingleTemporaryVisualEffect(ref FeatherShield, FeatherGuard.FeatherShieldEffect, body.bestFitActualRadius / 2f, body.HasBuff(FeatherGuard.BuffIndex), "Pelvis");
        }

        internal static void ApplyHooks() {
            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += OnUpdateVisualEffects;
        }

        private static void OnUpdateVisualEffects(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self);

            RLTemporaryEffects effects = self.GetComponent<RLTemporaryEffects>();
            if (!effects) effects = self.AddComponent<RLTemporaryEffects>();

            effects.UpdateTemporaryEffects(self);
        }
    }
}