using System;
using UnityEngine.SceneManagement;

namespace RaindropLobotomy.EGO.Gifts {
    public class ClockworkSpring : EGOGiftBase<ClockworkSpring>
    {
        public override ItemDef ItemDef => null;

        public override EquipmentDef EquipmentDef => Load<EquipmentDef>("edClockworkSpring.asset");

        public override void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += SpendPast;
        }

        private void SpendPast(CharacterBody sender, StatHookEventArgs args)
        {
            AccumulatedPast past = sender.GetComponent<AccumulatedPast>();

            if (past && past.dumpingPast) {
                args.attackSpeedMultAdd += 1f;
                args.cooldownMultAdd *= 0.5f;
                args.moveSpeedMultAdd += 1f;
            }

            if (sender.inventory && sender.inventory.currentEquipmentIndex == EquipmentDef.equipmentIndex) {
                sender.AddItemBehavior<AccumulatedPast>(1);
            }
            else if (sender.inventory) {
                sender.SetBuffCount(Buffs.AccumulatedPast.Instance.Buff.buffIndex, 0);
                sender.AddItemBehavior<AccumulatedPast>(0);
            }
        }

        public override void SetupLanguage()
        {
            LanguageAPI.Add("RL_GIFT_CLOCKWORKSPRING_NAME", "Clockwork Spring");
            LanguageAPI.Add("RL_GIFT_CLOCKWORKSPRING_DESC", "Passively collect <style=cIsUtility>time</style>, and activate to gain <style=cIsUtility>temporary attack, movement, and cooldown speed</style> based on stored time.");
            LanguageAPI.Add("RL_GIFT_CLOCKWORKSPRING_PICKUP", "Store time and unwind it for a temporary boost to attack, movement, and cooldown speed.");
        }

        public override bool ActivateEquipment(EquipmentSlot slot)
        {
            slot.GetComponent<AccumulatedPast>().DumpPast();
            return true;
        }
    }

    public class AccumulatedPast : CharacterBody.ItemBehavior {
        public float storedPast = 0f;
        public bool dumpingPast = false;
        private bool allowThisStage = true;
        public void Start() {
            allowThisStage = SceneCatalog.mostRecentSceneDef.sceneType != SceneType.Intermission;
        }
        public void FixedUpdate() {
            if (body.GetBuffCount(Buffs.AccumulatedPast.Instance.Buff) != Mathf.RoundToInt(storedPast)) {
                body.SetBuffCount(Buffs.AccumulatedPast.Instance.Buff.buffIndex, Mathf.RoundToInt(storedPast));
            }

            if (!dumpingPast && allowThisStage) {
                storedPast += Time.fixedDeltaTime;
            }
            else {
                storedPast -= Time.fixedDeltaTime * 5f;

                if (storedPast <= 0f) {
                    dumpingPast = false;
                    storedPast = 0f;
                }
            }
        }

        public void DumpPast() {
            dumpingPast = true;
            body.statsDirty = true;
        }
    }
}