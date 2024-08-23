using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RaindropLobotomy.EGO.Viend;
using RoR2.UI;

namespace RaindropLobotomy.Buffs {
    public class Pale : BuffBase
    {
        public override BuffDef Buff => Load<BuffDef>("bdPale.asset");
        public static DamageAPI.ModdedDamageType PaleDamage = DamageAPI.ReserveDamageType();
        private static float PaleMaxDuration = 10f;
        private static float PaleMinDuration = 3f;
        public static DamageColorIndex PaleColorIndex = (DamageColorIndex)194;
        private static Color32 PaleColor = new(0, 255, 255, 255);
        private static BodyIndex MimicryViendIndex => EGOMimicry.MimicryViendIndex;

        public override void PostCreation()
        {
            On.RoR2.DamageColor.FindColor += GetPaleColor;
            On.RoR2.HealthComponent.TakeDamage += ReceivedPaleDamage;
            IL.RoR2.CharacterBody.RecalculateStats += PaleCurse;
            IL.RoR2.UI.HealthBar.UpdateBarInfos += PaleCurseColor;
        }

        private void PaleCurseColor(ILContext il)
        {
            /* IL CODE TO MATCH

            IL_0269: dup
            IL_026a: ldloc.1
            IL_026b: ldfld float32 RoR2.HealthComponent/HealthBarValues::curseFraction
            IL_0270: ldc.r4 0.0
            IL_0275: cgt
            IL_0277: stfld bool RoR2.UI.HealthBar/BarInfo::enabled

            */

            ILCursor c = new(il);

            bool found = c.TryGotoNext(MoveType.After,
                x => x.MatchDup(),
                x => x.MatchLdloc(1),
                x => x.MatchLdfld(typeof(HealthComponent.HealthBarValues), "curseFraction"),
                x => x.MatchLdcR4(0f),
                x => x.MatchCgt(),
                x => x.MatchStfld(typeof(HealthBar.BarInfo), "enabled")
            );

            if (found) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<HealthBar>>((bar) => {
                    ref HealthBar.BarInfo curseBar = ref bar.barInfoCollection.curseBarInfo;
                    if (bar.source && bar.source.body && bar.source.body.GetBuffCount(Buff) > 0) {
                        curseBar.color = PaleColor;
                    }
                });
            }
            else {
                // Debug.LogError("Failed to apply Pale Damage Color IL Hook");
            }
        }

        private void PaleCurse(ILContext il)
        {
            /* IL CODE TO MATCH

            // if (buffCount3 > 0)
            IL_12ad: ldloc.s 93
            IL_12af: ldc.i4.0
            IL_12b0: ble.s IL_12c9

            // cursePenalty += (float)buffCount3 * 0.01f;
            IL_12b2: ldarg.0
            IL_12b3: ldarg.0
            IL_12b4: call instance float32 RoR2.CharacterBody::get_cursePenalty()
            IL_12b9: ldloc.s 93
            IL_12bb: conv.r4
            IL_12bc: ldc.r4 0.01
            IL_12c1: mul
            IL_12c2: conv.r4
            IL_12c3: add

            */
            ILCursor c = new(il);

            bool found = c.TryGotoNext(MoveType.Before, 
                x => x.MatchLdloc(93),
                x => x.MatchLdcI4(0),
                x => x.MatchBle(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt(typeof(CharacterBody), "get_cursePenalty"),
                x => x.MatchLdloc(93),
                x => x.MatchConvR4(),
                x => x.MatchLdcR4(0.01f),
                x => x.MatchMul(),
                x => x.MatchConvR4(),
                x => x.MatchAdd()
            );

            if (found) {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<CharacterBody>>((x) => {
                    int paleCount = x.GetBuffCount(Buff);
                    // // Debug.Log("pale count is: " + paleCount);
                    if (paleCount <= 0) return;
                    paleCount = Mathf.Clamp(paleCount, 0, 99);

                    float penaltyAdd = paleCount * 0.01f;

                    x.cursePenalty += penaltyAdd;
                    // // Debug.Log("curse penalty: " + x.cursePenalty);
                });
            }
            else {
                // Debug.LogError("Failed to apply Pale Damage IL Hook");
            }
        }

        private void ReceivedPaleDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(PaleDamage)) {                
                float targetMaxHP = self.fullHealth;
                float totalPale = damageInfo.damage;
                float mult = totalPale * 0.01f;
                float damage = targetMaxHP * mult;

                if (self.body.bodyIndex == MimicryViendIndex) {
                    damage *= 0.8f;
                }

                int paleToInflict = Mathf.FloorToInt(totalPale / 2);
                float paleDuration = Util.Remap(totalPale, 0, 100, PaleMinDuration, PaleMaxDuration);

                for (int i = 0; i < paleToInflict; i++) {
                    self.body.AddTimedBuff(Buff, paleDuration);
                }
                
                damageInfo.damage = damage;
                damageInfo.RemoveModdedDamageType(PaleDamage);
            }

            orig(self, damageInfo);
        }

        private Color GetPaleColor(On.RoR2.DamageColor.orig_FindColor orig, DamageColorIndex colorIndex)
        {
            if (colorIndex == PaleColorIndex) return PaleColor;
            return orig(colorIndex);
        }
    }
}