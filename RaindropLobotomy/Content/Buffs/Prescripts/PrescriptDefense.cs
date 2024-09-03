using System;
using RaindropLobotomy.EGO.Merc;
using RoR2.Orbs;

namespace RaindropLobotomy.Buffs {
    public class PrescriptDefense : BuffBase<PrescriptDefense>
    {
        public override BuffDef Buff => Load<BuffDef>("bdPrescriptDefense.asset");

        public override void PostCreation()
        {
            RecalculateStatsAPI.GetStatCoefficients += IncreaseStats;
            GlobalEventManager.onServerDamageDealt += OnStruck;
        }

        private void OnStruck(DamageReport report)
        {
            float mult = report.victimBodyIndex == IndexMerc.IndexGiantFistBody ? 2f : 1f;
            if (report.attackerBody && report.victimBody) {
                if (report.victimBody.HasBuff(Buff)) {
                    LightningOrb orb = new();
                    orb.lightningType = LightningOrb.LightningType.RazorWire;
                    orb.damageValue = report.victimBody.baseDamage * 2f * mult;
                    orb.damageColorIndex = DamageColorIndex.Item;
                    orb.attacker = report.victimBody.gameObject;
                    orb.bouncesRemaining = 0;
                    orb.bouncedObjects = null;
                    orb.range = 0f;
                    orb.origin = report.damageInfo.position;
                    orb.teamIndex = report.victimTeamIndex;
                    orb.isCrit = false;
                    orb.damageCoefficientPerBounce = 1f;
                    orb.target = report.attackerBody.mainHurtBox;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }

        private void IncreaseStats(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(Buff) && sender.healthComponent) {
                args.armorAdd += 50;
                args.regenMultAdd += 3.5f;
                args.baseShieldAdd += sender.healthComponent.fullCombinedHealth * 0.25f;
            }
        }
    }
}