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

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace RaindropLobotomy {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class Main : BaseUnityPlugin {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "pseudopulse";
        public const string PluginName = "RaindropLobotomy";
        public const string PluginVersion = "1.0.0";

        public static Assembly assembly;

        public static AssetBundle MainAssets;
        public static BepInEx.Logging.ManualLogSource ModLogger;

        public void Awake() {
            assembly = typeof(Main).Assembly;
            ModLogger = Logger;
            MainAssets = AssetBundle.LoadFromFile(assembly.Location.Replace("RaindropLobotomy.dll", "enkephalin"));

            OrdealManager.Initialize();

            ScanTypes<EnemyBase>(x => x.Create());
            ScanTypes<OrdealBase>(x => x.Create());
            ScanTypes<EGOSkillBase>(x => x.Create());
            ScanTypes<BuffBase>(x => x.Create());
            ScanTypes<SurvivorBase>(x => x.Create());

            StubShaders(MainAssets);
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

        public static void ScanTypes<T>(Action<T> action) {
            IEnumerable<Type> types = assembly.GetTypes().Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(T)));

            foreach (Type type in types) {
                T instance = (T)Activator.CreateInstance(type);
                action(instance);
            }
        }
    }
}