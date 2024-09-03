using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.UI;
using R2API;
using HarmonyLib;

namespace RaindropLobotomy.Buffs {
    public class DarkFlame : BuffBase<DarkFlame>
    {
        public override BuffDef Buff => Load<BuffDef>("bdDarkFlame.asset");
        public DamageAPI.ModdedDamageType DarkFlameDamageType = DamageAPI.ReserveDamageType();
        public DotController.DotDef DarkFlameDOT;
        public BurnEffectController.EffectParams DarkFlameEffect;
        public DotController.DotIndex DarkFlameIndex;

        public override void PostCreation()
        {
            DarkFlameDOT = new();
            DarkFlameDOT.associatedBuff = Buff;
            DarkFlameDOT.interval = 0.2f;
            DarkFlameDOT.damageCoefficient = 0.1f;
            DarkFlameDOT.terminalTimedBuff = Buff;
            DarkFlameDOT.terminalTimedBuffDuration = 1f;

            DarkFlameIndex = DotAPI.RegisterDotDef(DarkFlameDOT);

            On.RoR2.CharacterBody.AddBuff_BuffIndex += OnAddDarkFlame;

            DarkFlameEffect = new();
            DarkFlameEffect.fireEffectPrefab = Paths.GameObject.HelfireEffect;
            DarkFlameEffect.overlayMaterial = Paths.Material.matOnHelfire;
            DarkFlameEffect.startSound = "Play_item_proc_igniteOnKill_Loop";
			DarkFlameEffect.stopSound = "Stop_item_proc_igniteOnKill_Loop";

            On.RoR2.GlobalEventManager.OnHitEnemy += InflictDarkFlame;
            RecalculateStatsAPI.GetStatCoefficients += DarkFlameArmorReduction;
        }

        private void DarkFlameArmorReduction(CharacterBody sender, StatHookEventArgs args)
        {
            if (sender.HasBuff(Buff)) {
                args.armorAdd -= 60 * sender.GetBuffCount(Buff);
            }
        }

        private void InflictDarkFlame(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.attacker && damageInfo.HasModdedDamageType(DarkFlameDamageType)) {
                InflictDotInfo info = new();
                info.damageMultiplier = 1f;
                info.totalDamage = damageInfo.attacker.GetComponent<CharacterBody>().damage * 15f;
                info.duration = 12f;
                info.victimObject = victim;
                info.attackerObject = damageInfo.attacker;
                info.preUpgradeDotIndex = DarkFlameIndex;
                info.dotIndex = DarkFlameIndex;

                DotController.InflictDot(ref info);
            }
        }

        private void OnAddDarkFlame(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            orig(self, buffType);

            if (buffType == Buff.buffIndex) {
                CharacterModel model = self.modelLocator?.modelTransform?.GetComponent<CharacterModel>() ?? null;

                if (model) {
                    DarkFlameController controller = model.GetComponent<DarkFlameController>();

                    if (!controller) {
                        controller = model.AddComponent<DarkFlameController>();
                    }

                    controller.effectType = DarkFlameEffect;
                    controller.body = self;
                    controller.target = model.gameObject;
                }
            }
        }

        private class DarkFlameController : BurnEffectController {
            internal CharacterBody body;

            public void FixedUpdate() {
                if (!body.HasBuff(DarkFlame.Instance.Buff)) {
                    Destroy(this);
                }
            }
        }
    }
}