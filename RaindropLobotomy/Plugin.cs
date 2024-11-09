using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Linq;
using RaindropLobotomy.Enemies;
using RaindropLobotomy.Ordeals;
using RaindropLobotomy.EGO;
using RaindropLobotomy.Buffs;
using RaindropLobotomy.Survivors;
using RaindropLobotomy.Skills;
using RaindropLobotomy.EGO.Gifts;
using Survariants;
using System.Collections;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RaindropLobotomy {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Survariants.Survariants.PluginGUID)]
    [BepInDependency(R2API.DotAPI.PluginGUID)]
    [BepInDependency(DamageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(LanguageAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(DirectorAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(PrefabAPI.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rob.Paladin", BepInDependency.DependencyFlags.SoftDependency)]
    
    public class Main : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "BALLS";
        public const string PluginName = "RaindropLobotomy";
        public const string PluginVersion = "1.5.0";

        public static Assembly assembly;

        public static AssetBundle MainAssets;
        public static BepInEx.Logging.ManualLogSource ModLogger;
        public static ConfigFile config;

        public static float[] RandomizedPercentages = new float[25];
        private float stopwatch = 0f;
        public static Dictionary<HealthComponent, int> PercentagesMap = new();
        // compat
        public static bool paladinInstalled;

        public class MainConfiguration : ConfigClass
        {
            public override string Section => "Main Configuration";
            public bool SuppressDuplicateVariants => Option<bool>("Suppress Duplicate Variants", "Prevent a vanilla-targeted variant from loading if another mod survivor has a copy of that variant. (ex: Index Messenger Mercenary wont load if Paladin is present, as he receives Index Messenger Paladin)", true);

            public override void Initialize()
            {
                
            }
        }

        public static MainConfiguration MainConfig = new();

        public void Awake() {
            assembly = typeof(Main).Assembly;
            ModLogger = Logger;
            MainAssets = AssetBundle.LoadFromFile(assembly.Location.Replace("RaindropLobotomy.dll", "enkephalin"));

            paladinInstalled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rob.Paladin");

            config = Config;

            OrdealManager.Initialize();
            AbnormalityManager.Initialize();

            ScanTypes<EnemyBase>(x => x.Create());
            ScanTypes<OrdealBase>(x => x.Create());
            ScanTypes<EGOSkillBase>(x => x.Create());
            ScanTypes<BuffBase>(x => x.Create());
            ScanTypes<SurvivorBase>(x => x.Create());
            ScanTypes<SkillBase>(x => x.Create());
            ScanTypes<EGOGiftBase>(x => x.Initialize());

            StubShaders(MainAssets);

            ForAllAssets<SkillDef>(x => ContentAddition.AddSkillDef(x));
            ForAllAssets<SkillFamily>(x => ContentAddition.AddSkillFamily(x));
            ForAllAssets<GameObject>(x => {
                if (x.GetComponent<NetworkIdentity>()) {
                    
                    if (x.name.Contains("LampBody")) return;

                    PrefabAPI.RegisterNetworkPrefab(x);
                    ContentAddition.AddNetworkedObject(x);
                }

                if (x.GetComponent<EffectComponent>()) {
                    ContentAddition.AddEffect(x);
                }
            });

            ScanTypes<EntityState>((Type x) => {
                ContentAddition.AddEntityState(x, out _);
            });

            On.RoR2.RoR2Content.Init += OnWwiseInit;

            RLTemporaryEffects.ApplyHooks();
        }

        private void OnWwiseInit(On.RoR2.RoR2Content.orig_Init orig)
        {
            orig();
            
            Logger.LogError("RL: Loading Soundbanks");

            string path = typeof(Main).Assembly.Location.Replace("RaindropLobotomy.dll", "");
            AkSoundEngine.AddBasePath(path);

            AkSoundEngine.LoadBank("InitRL", out _);
            AkSoundEngine.LoadBank("RLBank", out _);
        }

        public void Start() { // needs to happen in start to ensure we do this after any other mods we might softdep on have done theirs
            ScanTypes<CorrosionBase>(x => 
                {
                    if (x.AreWeAllowedToLoad()) { // do a check first as some variants are given to different mods when present (ex: index mercenary -> index paladin)
                        x.Create();
                    }
                }
            );
        }

        public static T Load<T>(string key) where T : UnityEngine.Object {
            return MainAssets.LoadAsset<T>(key);
        }

        public static void StubShaders(AssetBundle bundle) {
            Material[] mats = MainAssets.LoadAllAssets<Material>();

            foreach (Material mat in mats) {
                mat.shader = mat.shader.name switch {
                    "StubbedShader/deferred/hgstandard" => Paths.Shader.HGStandard,
                    "StubbedShader/fx/hgcloudremap" => Paths.Shader.HGCloudRemap,
                    "Hopoo Games/FX/Cloud Remap" => Paths.Shader.HGCloudRemap,
                    _ => mat.shader
                };
            }
        }

        public static void ForAllAssets<T>(Action<T> action) where T : UnityEngine.Object {
            T[] ts = MainAssets.LoadAllAssets<T>();
            for (int i = 0; i < ts.Length; i++) {
                action(ts[i]);
            }
        }

        public static float GetPercentage(HealthComponent comp) {
            if (comp == null) {
                return 1f;
            }

            if (!PercentagesMap.ContainsKey(comp)) {
                PercentagesMap[comp] = Random.Range(0, RandomizedPercentages.Length);
            }

            return RandomizedPercentages[PercentagesMap[comp]];
        }

        public void FixedUpdate() {
            stopwatch += Time.fixedDeltaTime;

            if (stopwatch >= 0.8f) {
                stopwatch = 0f;

                for (int i = 0; i < RandomizedPercentages.Length; i++) {
                    RandomizedPercentages[i] = Random.Range(0f, 1f);
                }
            }
        }

        public static void ScanTypes<T>(Action<T> action) {
            IEnumerable<Type> types = assembly.GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (Type type in types) {
                if (typeof(T) == typeof(SurvivorBase) && type.IsSubclassOf(typeof(CorrosionBase))) {
                    continue;
                }

                T instance = (T)Activator.CreateInstance(type);
                action(instance);
            }
        }

        public static void ScanTypes<T>(Action<Type> action) {
            IEnumerable<Type> types = assembly.GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (Type type in types) {
                if (typeof(T) == typeof(SurvivorBase) && type.IsSubclassOf(typeof(CorrosionBase))) {
                    continue;
                }

                action(type);
            }
        }
    }
}