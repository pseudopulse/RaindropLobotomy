using System;
using System.Runtime.CompilerServices;
using Survariants;

namespace RaindropLobotomy.EGO.Merc {
    public class IndexPaladin : CorrosionBase<IndexPaladin>
    {
        public override string EGODisplayName => "Index Messenger Paladin";

        public override string Description => "I have the talent to walk the path in front of me, if nothing else...";

        public override SurvivorDef TargetSurvivorDef => Paths.SurvivorDef.Merc;

        public override UnlockableDef RequiredUnlock => null;

        public override Color Color => new Color32(159, 166, 100, 255);

        public override SurvivorDef Survivor => null;

        public override GameObject BodyPrefab => null;

        public override GameObject MasterPrefab => null;

        public override string RequiresMod => "com.rob.Paladin"; // if paladin is present, index variant goes to him instead

        private GameObject _bodyPrefab;
        private GameObject _displayPrefab;
        private SurvivorDef _survivorDef;

        public override void Create()
        {
            // SetupPaladinVariant();
            // SetupLanguage();
            // Modify();
            // IndexMerc.SharedSetup();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void SetupPaladinVariant() {

            _bodyPrefab = PrefabAPI.InstantiateClone(PaladinMod.PaladinPlugin.characterPrefab, "IndexPaladinBody");
            _displayPrefab = PrefabAPI.InstantiateClone(PaladinMod.Modules.Prefabs.paladinDisplayPrefab, "IndexPaladinDisplay");
            _survivorDef = CreateSurvivorDef(_bodyPrefab, _displayPrefab, "RL_INDEXPALADIN_NAME", "RL_INDEXMERC_DESC");

            _bodyPrefab.GetComponent<CharacterBody>().bodyColor = Color;

            _bodyPrefab.AddComponent<IndexMerc.IndexPrescriptTargeter>();
            _bodyPrefab.AddComponent<IndexMerc.IndexLockTargeter>();
            
            EGO = ScriptableObject.CreateInstance<SurvivorVariantDef>();
            (EGO as ScriptableObject).name = EGODisplayName;
            EGO.DisplayName = EGODisplayName;
            EGO.VariantSurvivor = _survivorDef;
            List<SurvivorDef> guhWhy = typeof(PaladinMod.Modules.Prefabs).GetField("survivorDefinitions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null) as List<SurvivorDef>;
            EGO.TargetSurvivor = guhWhy[0];
            EGO.RequiredUnlock = RequiredUnlock;
            EGO.Color = Color;
            EGO.Description = Description;

            SurvivorVariantCatalog.AddSurvivorVariant(EGO);

            GameObject dummyHolder = Load<GameObject>("YanadinDummy.prefab");
            Mesh paladinMesh = dummyHolder.transform.Find("Mesh1").GetComponent<MeshFilter>().sharedMesh;
            Mesh paladinCape = dummyHolder.transform.Find("Mesh2").GetComponent<MeshFilter>().sharedMesh;
            Mesh paladinSword = dummyHolder.transform.Find("Mesh3").GetComponent<MeshFilter>().sharedMesh;
            Material matYanadin = Load<Material>("matYanPaladin.mat");

            GameObject stupidBall = Load<GameObject>("YanPaladinOrb.prefab");

            ModifyModel(_bodyPrefab.GetComponent<ModelLocator>()._modelTransform.gameObject);
            ModifyModel(_displayPrefab);

            SkillLocator locator = _bodyPrefab.GetComponent<SkillLocator>();
            ReplaceSkill(locator.primary, Load<SkillFamily>("IndexPR.asset"));
            ReplaceSkill(locator.secondary, Load<SkillFamily>("IndexSC.asset"));
            ReplaceSkill(locator.utility, Load<SkillFamily>("IndexUT.asset"));
            ReplaceSkill(locator.special, Load<SkillFamily>("IndexSP.asset"));

            locator.passiveSkill.skillNameToken = "RL_INDEXMERC_PASSIVE_NAME";
            locator.passiveSkill.skillDescriptionToken = "RL_INDEXMERC_PASSIVE_DESC";
            locator.passiveSkill.icon = Load<Sprite>("OmPower.png");

            // TODO:
            // replace passive vfx
            // remove eye trail
            // actually spawn in the orb
            // replace spawn vfx
            // fix the sword using the emissive at some parts

            void ModifyModel(GameObject target) {
                CharacterModel model = target.GetComponentInChildren<CharacterModel>();
                model.RemoveComponent<ModelSkinController>();
                model.baseRendererInfos = new CharacterModel.RendererInfo[] {
                    /*new CharacterModel.RendererInfo {
                        renderer = model.transform.Find("meshCape").GetComponent<SkinnedMeshRenderer>(),
                        defaultMaterial = matYanadin,
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On
                    },*/
                    new CharacterModel.RendererInfo {
                        renderer = model.transform.Find("Armature/meshSword").GetComponent<SkinnedMeshRenderer>(),
                        defaultMaterial = Load<Material>("matIndexMerc.mat"),
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On
                    },
                    new CharacterModel.RendererInfo {
                        renderer = model.transform.Find("Armature/meshPaladin").GetComponent<SkinnedMeshRenderer>(),
                        defaultMaterial = matYanadin,
                        defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On
                    },
                };

                // (model.baseRendererInfos[0].renderer as SkinnedMeshRenderer).sharedMesh = paladinCape;
                (model.baseRendererInfos[0].renderer as SkinnedMeshRenderer).sharedMesh = paladinSword;
                (model.baseRendererInfos[1].renderer as SkinnedMeshRenderer).sharedMesh = paladinMesh;

                Transform spine2 = model.transform.Find("Armature/spine/spine.001/spine.002");
                GameObject ballCopy = GameObject.Instantiate(stupidBall, spine2);

                ChildLocator loc = model.GetComponent<ChildLocator>();
                loc.FindChild("Sword").Find("SwordStuff").gameObject.SetActive(false);
                SkinnedMeshRenderer capeRend = loc.FindChild("Chest").parent.Find("cape.01").Find("Cape/meshCape").GetComponent<SkinnedMeshRenderer>();
                capeRend.material = matYanadin;
                capeRend.sharedMesh = paladinCape;
                capeRend.transform.parent.gameObject.SetActive(true);
                capeRend.transform.parent.parent.gameObject.SetActive(true);
            }

            ContentAddition.AddSurvivorDef(_survivorDef);
            ContentAddition.AddBody(_bodyPrefab);
        }

        public override void Modify()
        {
            base.Modify();
        }

        public override void SetupLanguage()
        {
            base.SetupLanguage();

            "RL_INDEXPALADIN_NAME".Add("Index Messenger Paladin");
        }
    }
}