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
using RaindropLobotomy.Enemies;

namespace RaindropLobotomy.Ordeals.Midnight.Green
{
    public class LastHelix : EnemyBase<LastHelix>
    {
        public static GameObject LaserPrefab;
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("LastHelixBody.prefab");
            prefabMaster = Load<GameObject>("LastHelixMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            LanguageAPI.Add("RL_LASTHELIX_NAME", "Helix of the End");
            LanguageAPI.Add("RL_LASTHELIX_SUB", "Ordeal of Green Midnight");


            // EFFECTS
            LaserPrefab = PrefabAPI.InstantiateClone(Assets.GameObject.EyeBeamRoboBallMini, "HelixLaser");
            LaserPrefab.transform.Find("TechyLine").GetComponent<LineRenderer>().material = Assets.Material.matMoonElevatorBeam;
            LaserPrefab.transform.Find("TechyLine").GetComponent<LineRenderer>().startWidth = 3f;
            LaserPrefab.transform.Find("TechyLine").GetComponent<LineRenderer>().endWidth = 3f;
            LaserPrefab.transform.Find("TechyLine").GetComponent<LineRenderer>().widthMultiplier = 1f;
            LaserPrefab.transform.Find("LaserFront").GetComponent<LineRenderer>().enabled = false;
            LaserPrefab.transform.Find("LaserFront").GetComponent<LineRenderer>().startWidth = 2.5f;
            LaserPrefab.transform.Find("LaserFront").GetComponent<LineRenderer>().endWidth = 2.5f;
            LaserPrefab.transform.Find("LaserFront").GetComponent<LineRenderer>().widthMultiplier = 1f;
            LaserPrefab.transform.Find("LaserFront").Find("Ring").gameObject.SetActive(false);
            LaserPrefab.transform.Find("LaserFront").Find("Ring, Bright").gameObject.SetActive(false);
            LaserPrefab.transform.Find("LaserFront").Find("SquareFlare").gameObject.SetActive(false);
            LaserPrefab.transform.Find("LaserFront").Find("HitFlash").gameObject.SetActive(false);
            LaserPrefab.transform.Find("LaserEnd").Find("Fire").GetComponent<ParticleSystemRenderer>().material = Assets.Material.matVoidSurvivorBlasterFireCorrupted;
            LaserPrefab.transform.Find("LaserEnd").Find("SquareFlare").gameObject.SetActive(false);
            LaserPrefab.transform.Find("LaserEnd").Find("HitFlash").gameObject.SetActive(false);
        }
    }
}