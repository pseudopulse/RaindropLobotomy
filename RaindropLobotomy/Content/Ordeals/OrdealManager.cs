using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using System;
using RoR2.UI;
using System.Collections.Generic;
using EntityStates;
using RoR2.ExpansionManagement;
using Unity;
using HarmonyLib;
using RoR2.CharacterAI;
using UnityEngine.UI;
using TMPro;

namespace RaindropLobotomy.Ordeals
{
    public class OrdealManager {
        public static Dictionary<OrdealLevel, List<OrdealBase>> ordeals = new() {
            {OrdealLevel.DAWN, new() {}},
            {OrdealLevel.NOON, new() {}},
            {OrdealLevel.DUSK, new() {}},
            {OrdealLevel.MIDNIGHT, new() {}},
        };

        private static GameObject ordealUI;
        private static GameObject ordealPopupUI;

        public static void Initialize() {
            CreateOrdealPopupUI();
            CreateOrdealTimerUI();
            On.RoR2.UI.HUD.Awake += PickOrdeal;
        }

        private static void PickOrdeal(On.RoR2.UI.HUD.orig_Awake orig, RoR2.UI.HUD self)
        {
            orig(self);
            SetupOrdeal(self);
        }

        private static void CreateOrdealTimerUI() {
            ordealUI = PrefabAPI.InstantiateClone(Assets.GameObject.HudCountdownPanel, "Ordeal UI");
            ChildLocator loc = ordealUI.AddComponent<ChildLocator>();
            LayoutElement element = ordealUI.AddComponent<LayoutElement>();
            element.ignoreLayout = true;
            ordealUI.transform.Find("Juice").Find("Container").Find("Border").GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            ordealUI.transform.Find("Juice").Find("Container").Find("Border").GetComponent<RectTransform>().sizeDelta = new(260, 90);
            ordealUI.transform.Find("Juice").Find("Container").Find("Backdrop").GetComponent<RectTransform>().sizeDelta = new(260, 90);
            Transform countdown = ordealUI.transform.Find("Juice").Find("Container").Find("CountdownTitleLabel");
            countdown.GetComponent<RectTransform>().localScale = new(1.7f, 1.7f, 1f);
            countdown.GetComponent<RectTransform>().localPosition = new(70f, 0f, 0f);
            Transform countdown2 = ordealUI.transform.Find("Juice").Find("Container").Find("CountdownLabel");
            countdown2.GetComponent<RectTransform>().position = new(-50f, 0f, 0f);
            countdown2.transform.parent.RemoveComponent<ContentSizeFitter>();
            countdown2.transform.parent.RemoveComponent<VerticalLayoutGroup>();
            GameObject.DestroyImmediate(countdown.GetComponent<HGTextMeshProUGUI>());
            Image image = countdown.AddComponent<Image>();
            countdown.GetComponent<RectTransform>().sizeDelta = new(50, 50);

            loc.transformPairs = new ChildLocator.NameTransformPair[] {
                new() {
                    transform = countdown,
                    name = "Icon"
                },
            };
        }

        private static void CreateOrdealPopupUI() { 
            ordealPopupUI = PrefabAPI.InstantiateClone(Assets.GameObject.AchievementNotificationPanel, "OrdealPopupUI");
            ordealPopupUI.GetComponent<RectTransform>().position = new(0, 90, 0);
            ordealPopupUI.transform.Find("Backdrop").GetComponent<Image>().color = new Color32(95, 95, 95, 255);
            ordealPopupUI.transform.Find("Backdrop").GetComponent<RectTransform>().localScale = new(0.5f, 1f, 1f);
            ordealPopupUI.transform.Find("Blur").GetComponent<RectTransform>().localScale = new(0.5f, 1f, 1f);
            ordealPopupUI.transform.Find("DisableDuringSwiping").Find("DisplayArea").Find("UnlockedImagePanel").gameObject.SetActive(false);
            Transform textRoot = ordealPopupUI.transform.Find("DisableDuringSwiping").Find("DisplayArea").Find("StackedTextPanel");
            textRoot.GetComponent<VerticalLayoutGroup>().spacing = 30;
            textRoot.Find("Title").GetComponent<HGTextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            textRoot.Find("UnlockDescription").GetComponent<HGTextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            GameObject sub = GameObject.Instantiate(textRoot.Find("UnlockDescription").gameObject, textRoot);
            sub.GetComponent<HGTextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            sub.transform.SetSiblingIndex(0);
            ChildLocator loc = ordealPopupUI.AddComponent<ChildLocator>();
            loc.transformPairs = new ChildLocator.NameTransformPair[] {
                new() {
                    transform = sub.transform,
                    name = "OrdealLevel"
                },
                new() {
                    transform = textRoot.Find("Title"),
                    name = "Name"
                },
                new() {
                    transform = textRoot.Find("UnlockDescription"),
                    name = "Subtitle"
                },
            };
        }

        private static void SetupOrdeal(HUD hudInst) {
            OrdealBase ordeal = GetNextOrdealType();
            if (ordeal == null) {
                Debug.Log("returning because no ordeal");
                return;
            }

            Transform parent = hudInst.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("UpperLeftCluster");
            GameObject hud = GameObject.Instantiate(ordealUI, parent);
            hud.transform.localPosition = new(100, -160, 0);
            OrdealController cont = hud.AddComponent<OrdealController>();
            cont.ordeal = ordeal;
        }

        public static void SpawnOrdealPopupUI(OrdealBase ordeal) {
            GameObject ui = GameObject.Instantiate(ordealPopupUI, HUD.instancesList[0].mainContainer.transform);
            ui.transform.localPosition = new(0, 90, 0);
            ChildLocator loc = ui.GetComponent<ChildLocator>();
            ChangeText(loc.FindChild("Name"), ordeal.Name, ordeal.Color);
            ChangeText(loc.FindChild("OrdealLevel"), ordeal.RiskTitle, ordeal.Color);
            ChangeText(loc.FindChild("Subtitle"), ordeal.Subtitle, ordeal.Color);
        }

        private static void ChangeText(Transform transform, string text, Color32 col) {
            TextMeshProUGUI gui = transform.GetComponent<HGTextMeshProUGUI>();
            gui.text = text;
            gui.color = col;
            gui.horizontalAlignment = HorizontalAlignmentOptions.Center;
            gui.alignment = TextAlignmentOptions.Center;
        }

        private static OrdealBase GetNextOrdealType() {
            int i = Run.instance.stageClearCount;
            OrdealLevel ordeal = OrdealLevel.MIDNIGHT;

            if (i > 2) ordeal = OrdealLevel.NOON;
            if (i > 4) ordeal = OrdealLevel.DUSK;
            if (i > 6) ordeal = OrdealLevel.DUSK;
            if (i > 8) ordeal = OrdealLevel.MIDNIGHT;

            OrdealBase[] options = ordeals[ordeal].ToArray();

            if (options.Length == 0) {
                return null;
            }

            return options[Run.instance.stageRng.RangeInt(0, options.Length)];
        }


        public class OrdealController : MonoBehaviour {
            private float totalDuration;
            private float duration;
            public OrdealBase ordeal;
            public TimerText timer;
            private bool startedOrdeal = false;

            public void Start() {
                totalDuration = ordeal.OrdealLevel switch {
                    OrdealLevel.DAWN => 60 * 5f,
                    OrdealLevel.NOON => 60 * 7f,
                    OrdealLevel.DUSK => 60 * 9f,
                    OrdealLevel.MIDNIGHT => 60 * 7f,
                    _ => 60 * 5f
                };
                duration = totalDuration;

                ChildLocator loc = GetComponent<ChildLocator>();
                timer = GetComponent<TimerText>();
                loc.FindChild("Icon").GetComponent<Image>().sprite = ordeal.OrdealLevel switch {
                    OrdealLevel.DAWN => Load<Sprite>("Dawn.png"),
                    OrdealLevel.NOON => Load<Sprite>("Noon.png"),
                    OrdealLevel.DUSK => Load<Sprite>("Dawn.png"),
                    OrdealLevel.MIDNIGHT => Load<Sprite>("Midnight.png"),
                };
                loc.FindChild("Icon").GetComponent<Image>().color = ordeal.Color;
            }

            public void FixedUpdate() {
                if (duration >= 0f) {
                    duration -= Time.fixedDeltaTime;
                    timer.seconds = duration;
                }

                if (duration <= 0f) {
                    timer.enabled = false;
                    timer.targetLabel.text = "XX:XX:XX";
                }

                if (duration <= 0f && !startedOrdeal) {
                    startedOrdeal = true;
                    OrdealManager.SpawnOrdealPopupUI(ordeal);
                    ordeal.OnSpawnOrdeal(RoR2.Stage.instance);
                }
            }
        }
    }
}