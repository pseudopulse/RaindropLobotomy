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
    public class GreenMidnight : OrdealBase<GreenMidnight>
    {
        public override OrdealLevel OrdealLevel => OrdealLevel.MIDNIGHT;

        public override string Name => "Helix of the End";

        public override string Subtitle => "The tower is touched by the sky, and nothing will remain on the ground.";

        public override string RiskTitle => "Ordeal of Green Midnight";

        public override Color32 Color => new Color(0.4117647f, 0.6431373f, 24f / 85f);

        public override void OnSpawnOrdeal(RoR2.Stage stage)
        {
            if (!TeleporterInteraction.instance) {
                return;
            }

            Debug.Log("Spawning the ordeal of green midnight");

            GameObject pref = Load<GameObject>("LastHelixSpawner.prefab");
            GameObject spawner = GameObject.Instantiate(pref);
            ScriptedCombatEncounter sce = spawner.GetComponent<ScriptedCombatEncounter>();
            sce.spawns[0].explicitSpawnPosition = TeleporterInteraction.instance.transform;
            sce.BeginEncounter();
        }
    }
}