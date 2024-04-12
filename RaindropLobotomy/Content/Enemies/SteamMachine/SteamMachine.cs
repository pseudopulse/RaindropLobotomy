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

namespace RaindropLobotomy.Enemies.SteamMachine
{
    public class SteamMachine : EnemyBase<SteamMachine>
    {
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("SteamMachineBody.prefab");
            prefabMaster = Load<GameObject>("SteamMachineMaster.prefab");

            // TODO: swap bulbs for matArtifactGlassOverlay

            ChildLocator loc = prefab.GetComponent<ModelLocator>()._modelTransform.GetComponent<ChildLocator>();
            Transform root = loc.FindChild("BulbRoot");

            root.AddComponent<SteamMachineDigitDisplay>();

            foreach (Transform child in root) {
                Transform inner = null;
                Transform outer = null;

                foreach (Transform child2 in child) {
                    if (child2.name == "InnerBulb") inner = child2;
                    else {
                        outer = child2;
                    }
                }

                outer.GetComponent<MeshRenderer>().material = Assets.Material.matArtifactGlassOverlay;
                inner.Find("Digit").GetComponent<MeshRenderer>().material = Assets.Material.matGrandparentFistGlowing;
            }

            RegisterEnemy(prefab, prefabMaster);

            LanguageAPI.Add("RL_STEAMMACHINE_NAME", "Steam Transport Machine");
            LanguageAPI.Add("RL_STEAMMACHINE_LORE", "");

        }

        public class SteamMachineDigitDisplay : MonoBehaviour {
            public Mesh[] Digits = new Mesh[10];
            public MeshFilter[] Bulbs = new MeshFilter[4];

            public void Start() {
                Bulbs[0] = transform.Find("BulbStem_1").Find("InnerBulb").Find("Digit").GetComponent<MeshFilter>();
                Bulbs[1] = transform.Find("BulbStem_2").Find("InnerBulb").Find("Digit").GetComponent<MeshFilter>();
                Bulbs[2] = transform.Find("BulbStem_3").Find("InnerBulb").Find("Digit").GetComponent<MeshFilter>();
                Bulbs[3] = transform.Find("BulbStem_4").Find("InnerBulb").Find("Digit").GetComponent<MeshFilter>();

                GameObject holder = Load<GameObject>("DigitMeshHolder.prefab");

                for (int i = 0; i < Digits.Length; i++) {
                    Digits[i] = holder.transform.GetChild(i).GetComponent<MeshFilter>().mesh;
                }
            }

            public void Update() {
                if (Run.instance) {
                    TimeSpan time = TimeSpan.FromSeconds(Run.instance.GetRunStopwatch());
                    int hours = Mathf.Clamp(time.Minutes, 0, 99);
                    int minutes = Mathf.Clamp(time.Seconds, 0, 60);

                    string h = hours.ToString();
                    string m = minutes.ToString();

                    int[] display = new int[4];

                    if (h.Length < 2) {
                        display[0] = 0;
                        display[1] = int.Parse(h);
                    }
                    else {
                        display[0] = int.Parse(h[0].ToString());
                        display[1] = int.Parse(h[1].ToString());
                    }

                    if (m.Length < 2) {
                        display[2] = 0;
                        display[3] = int.Parse(m);
                    }
                    else {
                        display[2] = int.Parse(m[0].ToString());
                        display[3] = int.Parse(m[1].ToString());
                    }

                    Debug.Log("Setting to: " + display.ToString());
                    
                    for (int i = 0; i < Bulbs.Length; i++) {
                        Bulbs[i].mesh = Digits[display[i]];
                    }
                }
            }
        }
    }
}