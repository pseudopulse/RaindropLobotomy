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
using RoR2.EntityLogic;

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

        public class OrdealConfig : ConfigClass
        {
            public override string Section => "Mechanics :: Ordeals";
            public bool Enabled => Option<bool>("Enabled", "Should ordeals appear during runs?", true);
            public bool ShowPopup => Option<bool>("Show Ordeal Popup", "Should the ordeal popup appear when an ordeal spawns?", true);
            public float DawnTimer => Option<float>("Activation Time - DAWN", "The time it takes (in seconds) for a Dawn ordeal to appear.", 60f * 5f);
            public float NoonTimer => Option<float>("Activation Time - NOON", "The time it takes (in seconds) for a Noon ordeal to appear.", 60f * 5f);
            public float DuskTimer => Option<float>("Activation Time - DUSK", "The time it takes (in seconds) for a Dusk ordeal to appear.", 60f * 6f);
            public float MidnightTimer => Option<float>("Activation Time - MIDNIGHT", "The time it takes (in seconds) for a Midnight ordeal to appear.", 60f * 7f);
        }

        public static OrdealConfig Config = new();
        public static OrdealTimer Timer;

        public static void Initialize() {
            if (!Config.Enabled) {
                return;
            }

            CreateOrdealPopupUI();
            CreateOrdealTimerUI();
            On.RoR2.Stage.Start += PickOrdeal;
            On.RoR2.Stage.OnDisable += ClearOrdeal;
        }

        private static void ClearOrdeal(On.RoR2.Stage.orig_OnDisable orig, RoR2.Stage self)
        {
            orig(self);

            ObjectivePanelController.collectObjectiveSources -= CollectSources;
        }

        private static void PickOrdeal(On.RoR2.Stage.orig_Start orig, RoR2.Stage self)
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
            GameObject imagePanel = ordealPopupUI.transform.Find("DisableDuringSwiping").Find("DisplayArea").Find("UnlockedImagePanel").gameObject;
            imagePanel.transform.Find("BG").gameObject.SetActive(false);
            imagePanel.transform.localPosition = new(-374, 0, 0);

            GameObject imagePanel2 = GameObject.Instantiate(imagePanel, imagePanel.transform.parent);
            imagePanel2.transform.localPosition = new(374, 0, 0);

            Transform textRoot = ordealPopupUI.transform.Find("DisableDuringSwiping").Find("DisplayArea").Find("StackedTextPanel");
            textRoot.GetComponent<VerticalLayoutGroup>().spacing = 30;
            textRoot.Find("Title").GetComponent<HGTextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            textRoot.Find("Title").transform.localPosition = new(0, 5, 0);
            textRoot.Find("UnlockDescription").GetComponent<HGTextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            GameObject sub = GameObject.Instantiate(textRoot.Find("UnlockDescription").gameObject, textRoot);
            sub.GetComponent<HGTextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            sub.transform.SetSiblingIndex(0);
            textRoot.Find("UnlockDescription").localScale *= 0.7f;
            textRoot.Find("UnlockDescription").localPosition = new(0, -45, 0);
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
                new() {
                    transform = imagePanel.transform,
                    name = "LeftIcon"
                },
                new() {
                    transform = imagePanel2.transform,
                    name = "RightIcon"
                }
            };

        }

        private static void SetupOrdeal(RoR2.Stage stage) {
            OrdealBase ordeal = GetNextOrdealType(SceneCatalog.mostRecentSceneDef);
            if (ordeal == null) {
                // Debug.Log("returning because no ordeal");
                return;
            }
            
            OrdealTimer timer = stage.AddComponent<OrdealTimer>();
            timer.ordeal = ordeal;

            Timer = timer;

            ObjectivePanelController.collectObjectiveSources += CollectSources;
        }

        private static void CollectSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> sources) {
            sources.Add(new ObjectivePanelController.ObjectiveSourceDescriptor() {
                master = master,
                objectiveType = typeof(OrdealObjective),
                source = Timer});
        }

        public static void SpawnOrdealPopupUI(OrdealBase ordeal) {
            if (!Config.ShowPopup) {
                return;
            }

            GameObject ui = GameObject.Instantiate(ordealPopupUI, HUD.instancesList[0].mainContainer.transform);
            ui.transform.localPosition = new(0, 90, 0);
            ChildLocator loc = ui.GetComponent<ChildLocator>();
            ChangeText(loc.FindChild("Name"), ordeal.Name, ordeal.Color);
            ChangeText(loc.FindChild("OrdealLevel"), $"<sprite=1> {ordeal.RiskTitle} <sprite=2>", ordeal.Color);
            ChangeText(loc.FindChild("Subtitle"), ordeal.Subtitle, ordeal.Color);
            ChangeIcon(loc.FindChild("LeftIcon"), ordeal);
            ChangeIcon(loc.FindChild("RightIcon"), ordeal);
        }

        private static void ChangeText(Transform transform, string text, Color32 col) {
            TextMeshProUGUI gui = transform.GetComponent<HGTextMeshProUGUI>();
            gui.text = text;
            gui.color = col;
            gui.horizontalAlignment = HorizontalAlignmentOptions.Center;
            gui.alignment = TextAlignmentOptions.Center;
        }

        private static void ChangeIcon(Transform transform, OrdealBase ordeal) {
            Image image = transform.Find("Icon").GetComponent<Image>();
            image.sprite = ordeal.OrdealLevel switch {
                OrdealLevel.DAWN => Load<Sprite>("Dawn.png"),
                 OrdealLevel.NOON => Load<Sprite>("Noon.png"),
                OrdealLevel.DUSK => Load<Sprite>("Dawn.png"),
                OrdealLevel.MIDNIGHT => Load<Sprite>("Midnight.png"),
            };
            image.color = ordeal.Color;
        }

        private static OrdealBase GetNextOrdealType(SceneDef scene) {
            if (scene.sceneType != SceneType.Stage) return null;
            if (scene.cachedName == "moon" || scene.cachedName == "moon2") return null;

            int i = (Run.instance.stageClearCount + 1) % 5;
            Debug.Log(i);
            OrdealLevel ordeal = OrdealLevel.DAWN;

            ordeal = i switch {
                1 => OrdealLevel.DAWN,
                2 => OrdealLevel.DAWN,
                3 => OrdealLevel.NOON,
                4 => OrdealLevel.NOON,
                0 => OrdealLevel.MIDNIGHT
            };

            // ordeal = OrdealLevel.MIDNIGHT;

            OrdealBase[] options = ordeals[ordeal].ToArray();

            // Debug.Log("Setting ordeal to: " + ordeal.ToString());

            if (options.Length == 0) {
                // Debug.Log("no options for that ordea, returning.");
                return null;
            }

            return options[Run.instance.stageRng.RangeInt(0, options.Length)];
        }

        public class OrdealTimer : MonoBehaviour {
            private float totalDuration;
            public float duration;
            public OrdealBase ordeal;
            private bool startedOrdeal = false;

            public void Start() {
                totalDuration = ordeal.OrdealLevel switch {
                    OrdealLevel.DAWN => Config.DawnTimer,
                    OrdealLevel.NOON => Config.NoonTimer,
                    OrdealLevel.DUSK => Config.DuskTimer,
                    OrdealLevel.MIDNIGHT => Config.MidnightTimer,
                    _ => 60 * 5f
                };
                
                duration = totalDuration;
                // duration = 10f;
            }

            public void FixedUpdate() {
                duration -= Time.fixedDeltaTime;

                if (duration <= 0f && !startedOrdeal) {
                    startedOrdeal = true;

                    SpawnOrdealPopupUI(ordeal);
                    
                    if (NetworkServer.active) {
                        ordeal.OnSpawnOrdeal(RoR2.Stage.instance);
                    }
                }
            }
        }

        public class OrdealObjective : ObjectivePanelController.ObjectiveTracker {
            public OrdealTimer ordealTimer = null;
            public override string GenerateString()
            {
                if (ordealTimer == null) ordealTimer = sourceDescriptor.source as OrdealTimer;
                string col = $"#{ColorUtility.ToHtmlStringRGBA(ordealTimer.ordeal.Color)}";

                return 
                $"Prepare for the <color={col}>{ordealTimer.ordeal.RiskTitle}</color> - <style=cStack>({(ordealTimer.duration >= 0f ? TimeSpan.FromSeconds(ordealTimer.duration).ToString(@"mm\:ss") : "XX:XX")})</style>";
            }
            public override bool IsDirty()
            {
                return true;
            }
        }
    }
}