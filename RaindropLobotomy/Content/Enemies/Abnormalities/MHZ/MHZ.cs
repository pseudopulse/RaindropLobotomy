using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using System;
using System.Collections.Generic;
using EntityStates;
using RoR2.ExpansionManagement;
using Unity;
using HarmonyLib;
using RoR2.CharacterAI;
using System.Linq;
using RoR2.UI;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Security.Cryptography;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace RaindropLobotomy.Enemies.MHZ {
    public class MHZ : EnemyBase<MHZ>, Abnormality
    {
        public RiskLevel ThreatLevel => RiskLevel.Teth;

        public SpawnCard SpawnCard => Load<CharacterSpawnCard>("cscMHZ.asset");

        public bool IsTool => false;

        public override string ConfigName => "176 MHz";

        public static GameObject FogEffect;
        public static BuffDef bdNoise;

        public static List<HUDStaticMarker> instancesList = new();
        public static GameObject HUDOverlay;
        //
        private delegate void orig_EquipmentIcon_SetDisplayData(EquipmentIcon self, EquipmentIcon.DisplayData data);

        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("MHZBody.prefab");
            prefabMaster = Load<GameObject>("MHZMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            FogEffect = Load<GameObject>("MHZStaticFog.prefab");
            bdNoise = Load<BuffDef>("bdMHZFog.asset");

            ContentAddition.AddBuffDef(bdNoise);

            LanguageAPI.Add("RL_MHZ_NAME", "1.76 MHz");
            LanguageAPI.Add("RL_MHZ_LORE", "");

            On.RoR2.CharacterBody.RecalculateStats += UpdateMHZFog;
            On.RoR2.UI.MoneyText.Update += MHZStatic_MoneyText;
            // On.RoR2.UI.LevelText.Update += MHZStatic_LevelText; // doesnt work
            On.RoR2.UI.ExpBar.Update += MHZStatic_ExpBar;
            IL.RoR2.UI.SkillIcon.Update += MHZStatic_SkillIcon;
            On.RoR2.HealthComponent.GetHealthBarValues += MHZStatic_HealthBar;

            Hook hook = new(
                typeof(EquipmentIcon).GetMethod(nameof(EquipmentIcon.SetDisplayData), (BindingFlags)(-1)),
                typeof(MHZ).GetMethod(nameof(MHZStatic_EquipmentIcon), (BindingFlags)(-1))
            );
        }

        private HealthComponent.HealthBarValues MHZStatic_HealthBar(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, HealthComponent self)
        {
            HealthComponent.HealthBarValues values = orig(self);

            if (self.body.HasBuff(bdNoise)) {
                values.healthDisplayValue = Random.Range(0, 90000);
                values.maxHealthDisplayValue = Random.Range(0, 90000);
                values.healthFraction = Main.GetPercentage(self);
            }

            return values;
        }

        private static void MHZStatic_EquipmentIcon(orig_EquipmentIcon_SetDisplayData orig, EquipmentIcon self, EquipmentIcon.DisplayData data)
        {
            if (self.GetComponent<HUDStaticMarker>() && data.equipmentDef) {
                data.cooldownValue = (int)(data.equipmentDef.cooldown * Main.RandomizedPercentages[8]);
            }

            orig(self, data);
        }

        private void MHZStatic_SkillIcon(ILContext il)
        {
            ILCursor c = new(il);

            bool found = c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld(typeof(SkillIcon), nameof(SkillIcon.targetSkill)),
                x => x.MatchCallOrCallvirt(typeof(GenericSkill), "get_cooldownRemaining")
            );

            c.Emit(OpCodes.Ldarg_0);

            c.EmitDelegate<Func<float, SkillIcon, float>>((num, icon) => {
                if (icon.GetComponent<HUDStaticMarker>()) {
                    float percentage = Main.RandomizedPercentages[(int)(icon.targetSkillSlot) + 1];
                    return icon.targetSkill.baseRechargeInterval * percentage;
                }
                else {
                    return num;
                }
            });
        }

        private void MHZStatic_ExpBar(On.RoR2.UI.ExpBar.orig_Update orig, ExpBar self)
        {
            if (self.GetComponent<HUDStaticMarker>()) {
                float x = Main.RandomizedPercentages[0];

                _ = self.rectTransform.rect;
                _ = self.fillRectTransform.rect;
                self.fillRectTransform.anchorMin = new Vector2(0f, 0f);
                self.fillRectTransform.anchorMax = new Vector2(x, 1f);
                self.fillRectTransform.sizeDelta = new Vector2(1f, 1f);

                return;
            }

            orig(self);
        }

        private void MHZStatic_LevelText(On.RoR2.UI.LevelText.orig_Update orig, LevelText self)
        {
            if (self.GetComponent<HUDStaticMarker>()) {
                self.targetText.text = RandomString(2);
                return;
            }

            orig(self);
        }

        private void MHZStatic_MoneyText(On.RoR2.UI.MoneyText.orig_Update orig, MoneyText self)
        {
            if (self.GetComponent<HUDStaticMarker>()) {
                self.targetText.text = RandomString(5);
                return;
            }

            orig(self);
        }

        private void UpdateMHZFog(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self.HasBuff(bdNoise)) {
                UpdateHUDMarkers(self);
            }
        }

        private void UpdateHUDMarkers(CharacterBody body) {
            if (instancesList.Where(x => x.target == body).Count() > 0) {
                return;
            }

            HUD hud = HUD.instancesList.FirstOrDefault(x => x.localUserViewer != null && x.localUserViewer.cachedBody == body);

            if (!hud) return;

            AddStaticMarkers(body, hud.moneyText, hud.lunarCoinText, hud.levelText, hud.expBar, hud.equipmentIcons[0],
                hud.skillIcons[0], hud.skillIcons[1], hud.skillIcons[2], hud.skillIcons[3], hud.healthBar
            );

            if (body.hasAuthority && !HUDOverlay) {
                HUDOverlay = GameObject.Instantiate(Load<GameObject>("MHZStaticOverlay.prefab"));
                var marker = HUDOverlay.AddComponent<HUDStaticMarker>();
                marker.killOurselves = true;
                marker.target = body;
            }
        }

        private void AddStaticMarkers(CharacterBody body, params Component[] components) {
            for (int i = 0; i < components.Length; i++) {
                if (components[i].GetComponent<HUDStaticMarker>()) continue;
                components[i].AddComponent<HUDStaticMarker>().target = body;
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789&%_-+*$#@!";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Range(0, s.Length)]).ToArray());
        }

        public class HUDStaticMarker : MonoBehaviour {
            public CharacterBody target;
            public bool killOurselves = false;

            public void Start() {
                instancesList.Add(this);
            }

            public void OnDestroy() {
                instancesList.Remove(this);
            }

            public void Update() {
                if (target && !target.HasBuff(bdNoise)) {
                    if (killOurselves) {
                        Destroy(base.gameObject);
                    }
                    else {
                        Destroy(this);
                    }
                }
            }
        }
    }
}