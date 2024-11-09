using System;
using RoR2.UI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Mono.Cecil;

namespace RaindropLobotomy.Buffs {
    public class Sin : BuffBase<Sin>
    {
        public override BuffDef Buff => Load<BuffDef>("bdSin.asset");

        public static DamageAPI.ModdedDamageType SinReflectionType = DamageAPI.ReserveDamageType();
        public static DamageAPI.ModdedDamageType SinType = DamageAPI.ReserveDamageType();

        public static float baseHealthPerStack = 30f;
        public static float championPenalty = 0.33f;

        public override void PostCreation()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += HandleExecute;
            IL.RoR2.UI.HealthBar.ApplyBars += UpdateExecutePreview;
        }

        private void UpdateExecutePreview(ILContext il)
        {
            ILCursor c = new(il);

            MethodReference handleBar = null;
            VariableDefinition allocator = null;
            int allocIndex = -1;

            c.TryGotoNext(x => x.MatchCallOrCallvirt(out handleBar) && handleBar.Name.StartsWith("<ApplyBars>g__HandleBar|"));
            Debug.Log(handleBar);
            c.TryGotoPrev(x => x.MatchLdloca(out allocIndex));
            Debug.Log(allocIndex);
            allocator = il.Method.Body.Variables[allocIndex];

            VariableDefinition sinExecuteInfo = new(il.Import(typeof(HealthBar.BarInfo)));
            il.Method.Body.Variables.Add(sinExecuteInfo);

            c.Index = 0;

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<HealthBar, HealthBar.BarInfo>>((bar) => {

                HealthBar.BarInfo info = new() {
                    enabled = Executable(bar.source),
                    color = bar.style.curseBarStyle.baseColor,
                    sprite = bar.style.curseBarStyle.sprite,
                    imageType = bar.style.curseBarStyle.imageType,
                    sizeDelta = bar.style.curseBarStyle.sizeDelta,
                    normalizedXMax = 0f,
                    normalizedXMin = 0f
                };

                if (info.enabled) {
                    float hp = GetExecuteFraction(bar.source);
                    float max = bar.source != null ? bar.source.fullCombinedHealth : 0f;

                    info.normalizedXMin = 0f;
                    info.normalizedXMax = hp;
                }

                return info;
            });
            c.Emit(OpCodes.Stloc, sinExecuteInfo);

            c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<HealthBar.BarInfoCollection>(nameof(HealthBar.BarInfoCollection.GetActiveCount)));
            c.Emit(OpCodes.Ldloca, sinExecuteInfo);
            c.EmitDelegate((int count, in HealthBar.BarInfo info) => {
                if (info.enabled) {
                    count++;
                }

                return count;
            });

            c.TryGotoNext(MoveType.Before, x => x.MatchRet());
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloca, sinExecuteInfo);
            c.Emit(OpCodes.Ldloca, allocator);
            c.Emit(OpCodes.Call, handleBar);
        }

        private void HandleExecute(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);

            if (damageInfo.HasModdedDamageType(SinReflectionType) && !self.body.isPlayerControlled) {
                int count = self.body.GetBuffCount(BuffIndex);
                self.body.SetBuffCount(BuffIndex, count + 5);
            }

            if (damageInfo.HasModdedDamageType(SinType) && !self.body.isPlayerControlled) {
                int count = self.body.GetBuffCount(BuffIndex);
                self.body.SetBuffCount(BuffIndex, count + 1);
            }

            if (Executable(self) && damageInfo.procChainMask.HasProc(ProcType.PlasmaCore)) {
                float hp = GetExecuteHealth(self);

                if (self.combinedHealth < hp) {
                    self.Suicide(damageInfo.attacker);

                    EffectManager.SpawnEffect(Paths.GameObject.OmniImpactExecuteBandit, new EffectData {
                        origin = damageInfo.position,
                        scale = self.body.bestFitActualRadius,
                    }, true);

                    AkSoundEngine.PostEvent(Events.Play_lunar_reroller_activate, self.gameObject);
                }
            }
        }

        private static bool Executable(HealthComponent hc) {
            if (!hc) return false;
            return hc.body.GetBuffCount(BuffIndex) > 0;
        }

        private static float GetExecuteHealth(HealthComponent hc) {
            if (!hc) return float.MaxValue;

            int stacks = hc.body.GetBuffCount(BuffIndex);
            
            float percentagePer = baseHealthPerStack * (hc.body.isChampion ? championPenalty : 1f) / hc.body.baseMaxHealth;

            float percentage = stacks * percentagePer;

            return hc.fullCombinedHealth * percentage;
        }

        private static float GetExecuteFraction(HealthComponent hc) {
            if (!hc) return 0f;

            int stacks = hc.body.GetBuffCount(BuffIndex);
            
            float percentagePer = baseHealthPerStack * (hc.body.isChampion ? championPenalty : 1f) / hc.body.baseMaxHealth;

            return stacks * percentagePer;
        }
    }
}