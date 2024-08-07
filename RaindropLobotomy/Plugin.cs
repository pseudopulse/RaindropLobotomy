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
    
    public class Main : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "BALLS";
        public const string PluginName = "RaindropLobotomy";
        public const string PluginVersion = "1.4.3";

        public static Assembly assembly;

        public static AssetBundle MainAssets;
        public static BepInEx.Logging.ManualLogSource ModLogger;
        public static ConfigFile config;

        public static float[] RandomizedPercentages = new float[25];
        private float stopwatch = 0f;
        public static Dictionary<HealthComponent, int> PercentagesMap = new();

        public void Awake() {
            assembly = typeof(Main).Assembly;
            ModLogger = Logger;
            MainAssets = AssetBundle.LoadFromFile(assembly.Location.Replace("RaindropLobotomy.dll", "enkephalin"));

            config = Config;

            OrdealManager.Initialize();
            AbnormalityManager.Initialize();

            ScanTypes<EnemyBase>(x => x.Create());
            ScanTypes<OrdealBase>(x => x.Create());
            ScanTypes<EGOSkillBase>(x => x.Create());
            ScanTypes<BuffBase>(x => x.Create());
            ScanTypes<SurvivorBase>(x => x.Create());
            ScanTypes<CorrosionBase>(x => x.Create());
            ScanTypes<SkillBase>(x => x.Create());
            ScanTypes<EGOGiftBase>(x => x.Initialize());

            StubShaders(MainAssets);

            ForAllAssets<SkillDef>(x => ContentAddition.AddSkillDef(x));
            ForAllAssets<SkillFamily>(x => ContentAddition.AddSkillFamily(x));
            ForAllAssets<GameObject>(x => {
                if (x.GetComponent<NetworkIdentity>()) {
                    // Debug.Log("Adding Networked Object: " + x);
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

            On.RoR2.RoR2Application.Start += (o, s) => {
                o(s);

                string path = typeof(Main).Assembly.Location.Replace("RaindropLobotomy.dll", "");
                AkSoundEngine.AddBasePath(path);

                AkSoundEngine.LoadBank("InitRL", out _);
                AkSoundEngine.LoadBank("RLBank", out _);
            };
        }

        public static T Load<T>(string key) where T : UnityEngine.Object {
            return MainAssets.LoadAsset<T>(key);
        }

        public static void StubShaders(AssetBundle bundle) {
            Material[] mats = MainAssets.LoadAllAssets<Material>();

            foreach (Material mat in mats) {
                mat.shader = mat.shader.name switch {
                    "StubbedShader/deferred/hgstandard" => Assets.Shader.HGStandard,
                    "StubbedShader/fx/hgcloudremap" => Assets.Shader.HGCloudRemap,
                    "Hopoo Games/FX/Cloud Remap" => Assets.Shader.HGCloudRemap,
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