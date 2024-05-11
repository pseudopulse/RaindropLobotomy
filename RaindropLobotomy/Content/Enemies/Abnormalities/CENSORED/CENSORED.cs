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

namespace RaindropLobotomy.Enemies.CENSORED
{
    public class CENSORED : EnemyBase<CENSORED>
    {
        public override void LoadPrefabs()
        {
            prefab = Load<GameObject>("CENSOREDBody.prefab");
            prefabMaster = Load<GameObject>("CENSOREDMaster.prefab");

            RegisterEnemy(prefab, prefabMaster);

            LanguageAPI.Add("RL_CENSORED_NAME", "[CENSORED]");
            LanguageAPI.Add("RL_CENSORED_LORE", "");
        }
    }
}