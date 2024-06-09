using System;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json.Serialization;
using RoR2.UI;
using UnityEngine.UI;

#pragma warning disable

namespace RaindropLobotomy.EGO {
    public class EGOManager {
        public static void Initialize() {
            On.RoR2.UI.CharacterSelectController.RebuildLocal += CharacterSelectController_RebuildLocal;
            On.RoR2.UI.CharacterSelectController.Awake += CharacterSelectController_Awake;
            On.RoR2.CharacterSelectBarController.EnforceValidChoice += CharacterSelectBarController_EnforceValidChoice;
            On.RoR2.UI.HGHeaderNavigationController.RebuildHeaders += ThisFuckingSucks;
        }

        private static void ThisFuckingSucks(On.RoR2.UI.HGHeaderNavigationController.orig_RebuildHeaders orig, HGHeaderNavigationController self)
        {
            HGHeaderNavigationController.Header header = self.headers.FirstOrDefault(x => x.headerName == "EGO");

            bool enabled = header.headerName == "EGO" ? header.headerButton.interactable : false;

            orig(self);

            if (header.headerName == "EGO") {
                header.headerButton.interactable = enabled;
            }
        }

        private static void CharacterSelectBarController_EnforceValidChoice(On.RoR2.CharacterSelectBarController.orig_EnforceValidChoice orig, CharacterSelectBarController self)
        {
            SurvivorDef surv = self.GetLocalUserExistingSurvivorPreference();
            if (EGOCatalog.EGOReverseMap.ContainsKey(surv)) {
                return;
            }

            orig(self);
        }

        private static void CharacterSelectController_Awake(On.RoR2.UI.CharacterSelectController.orig_Awake orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);

            EGOUIMenuController controller = self.AddComponent<EGOUIMenuController>();

            GameObject egoButton = GameObject.Instantiate(self.primaryColorImages[0].gameObject, self.primaryColorImages[0].transform.parent);
            egoButton.GetComponent<LanguageTextMeshController>().token = "E.G.O";
            egoButton.GetComponent<HGButton>().interactable = false;
            var navController = egoButton.transform.parent.GetComponent<HGHeaderNavigationController>();
            HGHeaderNavigationController.Header header = new();
            header.headerButton = egoButton.GetComponent<HGButton>();
            header.headerName = "EGO";
            header.tmpHeaderText = navController.headers[0].tmpHeaderText;
            self.primaryColorImages.AddItem(egoButton.GetComponent<Image>());
            controller.EGOButton = egoButton.GetComponent<HGButton>();

            GameObject egoContainer = GameObject.Instantiate(self.skillStripContainer.gameObject, self.skillStripContainer.parent);
            egoContainer.name = "EGOContainer";
            egoContainer.SetActive(false);
            controller.EGOContainer = egoContainer.transform;
            controller.CSS = self;

            header.headerRoot = egoContainer;

            HG.ArrayUtils.ArrayAppend(ref navController.headers, in header);
        }

        private static void CharacterSelectController_RebuildLocal(On.RoR2.UI.CharacterSelectController.orig_RebuildLocal orig, RoR2.UI.CharacterSelectController self)
        {
            orig(self);

            EGOUIMenuController controller = self.GetComponent<EGOUIMenuController>();

            if (!self.currentSurvivorDef) {
                return;
            }

            bool ownsEGO = OwnsValidEGO(self.currentSurvivorDef);

            // Debug.Log("owns ego: " + ownsEGO);

            controller.EGOButton.interactable = ownsEGO;

            SurvivorDef surv = self.currentSurvivorDef;

            controller.EGOButton.GetComponent<Image>().color = surv.bodyPrefab.GetComponent<CharacterBody>().bodyColor;

            if (ownsEGO) {
                controller.PopulateEGOForSurvivor(surv);
            }
            else if (controller.active) {
                controller.HideEGO();
            }
        }

        private static bool OwnsValidEGO(SurvivorDef surv) {
            return EGOCatalog.EGOReverseMap.ContainsKey(surv) || EGOCatalog.EGOMap.ContainsKey(surv);
        }

        public class EGOUIMenuController : MonoBehaviour {
            public HGButton EGOButton;
            public Transform EGOContainer;
            public GameObject SkillPrefab;
            public GameObject SkillFillerPrefab;
            public CharacterSelectController CSS;
            public UIElementAllocator<RectTransform> EGOAllocator;
            public UIElementAllocator<RectTransform> EGOFillerAllocator;
            public bool active = false;

            public void Start() {

                SkillPrefab = EGOContainer.transform.Find("SkillStripPrefab").gameObject;
                SkillFillerPrefab = EGOContainer.transform.Find("SkillStripFillerPrefab").gameObject;

                SkillFillerPrefab.GetComponent<Image>().enabled = false;

                EGOButton.onClick.RemoveAllListeners();
                EGOButton.onClick.AddListener(DisplayEGO);

                GameObject highlight = SkillPrefab.transform.Find("Inner/HoverHighlight").gameObject;
                highlight.GetComponent<Image>().overrideSprite = Assets.Texture2D.texUIHighlightHeader.MakeSprite();

                GameObject newHighlight = GameObject.Instantiate(highlight, highlight.transform.parent);
                newHighlight.name = "SelectedHighlight";

                EGOButton.imageOnHover = newHighlight.GetComponent<Image>();

                EGOAllocator = new(EGOContainer.GetComponent<RectTransform>(), SkillPrefab);
                EGOFillerAllocator = new(EGOContainer.GetComponent<RectTransform>(), SkillFillerPrefab.gameObject);
            }

            public void DisplayEGO() {
                active = true;
                EGOButton.transform.parent.GetComponent<HGHeaderNavigationController>().ChooseHeaderByButton(EGOButton.GetComponent<MPButton>());
            }

            public void HideEGO() {
                active = false;
                EGOButton.transform.parent.GetComponent<HGHeaderNavigationController>().ChooseHeaderByButton(CSS.primaryColorImages[1].GetComponent<MPButton>());
            }

            public void PopulateEGOForSurvivor(SurvivorDef surv) {
                if (EGOCatalog.EGOReverseMap.ContainsKey(surv)) {
                    surv = EGOCatalog.EGOReverseMap[surv];
                }

                List<EGODef> EGO = EGOCatalog.EGOMap[surv];

                int amount = EGO.Count + 1;

                EGOFillerAllocator.AllocateElements(0);
                EGOAllocator.AllocateElements(amount);

                for (int i = 0; i < amount; i++) {
                    EGODef currentEGO = i == 0 ? null : EGO[i - 1];
                    AllocateEGO(i, currentEGO ? currentEGO.TargetSurvivor : surv, currentEGO);
                }

                EGOFillerAllocator.AllocateElements(Mathf.Max(0, 4 - amount));
            }

            public void AllocateEGO(int element, SurvivorDef surv, EGODef ego) {
                GameObject tmp = ego ? ego.Corrosion.bodyPrefab : surv.bodyPrefab;
                Texture2D rizz = tmp.GetComponent<CharacterBody>().portraitIcon as Texture2D;
                Sprite icon = Sprite.Create(rizz, new Rect(0f, 0f, rizz.width, rizz.height), new Vector2(0.5f, 0.5f), 100f);
                string Display = $"No E.G.O";
                string Description = "The default survivor, untainted by E.G.O.";

                if (ego) {
                    Display = $"{Language.GetString(surv.displayNameToken)} :: {ego.DisplayName}";
                    Description = $"\"{ego.Description}\"";
                }

                Transform skillStrip = EGOAllocator.elements[element];

                Image image = skillStrip.Find("Inner/Icon").GetComponent<Image>();
                HGTextMeshProUGUI name = skillStrip.Find("Inner/SkillDescriptionPanel/SkillName").GetComponent<HGTextMeshProUGUI>();
                HGTextMeshProUGUI description = skillStrip.Find("Inner/SkillDescriptionPanel/SkillDescription").GetComponent<HGTextMeshProUGUI>();
                HGButton button = skillStrip.gameObject.GetComponent<HGButton>();
                Image selectedHighlight = skillStrip.Find("Inner/SelectedHighlight").GetComponent<Image>();

                image.sprite = icon;
                name.text = Display;
                name.color = ego ? ego.Color : surv.primaryColor;
                description.text = Description;

                button.hoverToken = "";
                button.showImageOnHover = false;
                button.imageOnHover.enabled = false;
                button.disablePointerClick = false;

                if ((ego ? ego.Corrosion : surv) == CSS.currentSurvivorDef) {
                    selectedHighlight.color = new Color(selectedHighlight.color.r, selectedHighlight.color.g, selectedHighlight.color.b, 1f);
                }
                else {
                    selectedHighlight.color = new Color32(62, 62, 62, 255);
                }

                EGOSlotBehaviour behaviour = button.AddComponent<EGOSlotBehaviour>();
                behaviour.surv = ego ? ego.Corrosion : surv;
                behaviour.user = CSS.localUser;

                button.onClick.AddListener(behaviour.OnClick);
            }
        }

        public class EGOSlotBehaviour : MonoBehaviour {
            public SurvivorDef surv;
            public LocalUser user;

            public void OnClick() {
                // Debug.Log("clicked buttonm");
                user.userProfile.SetSurvivorPreference(surv);
            }
        }
    }
}