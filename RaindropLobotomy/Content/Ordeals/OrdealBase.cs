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
using RoR2.Navigation;

namespace RaindropLobotomy.Ordeals
{
    public abstract class OrdealBase<T> : OrdealBase where T : OrdealBase<T>
    {
        public static T Instance { get; private set; }

        public OrdealBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class OrdealBase
    {
        public abstract OrdealLevel OrdealLevel { get; }
        public abstract string Name { get; }
        public abstract string Subtitle { get; }
        public abstract string RiskTitle { get; }
        public abstract Color32 Color { get; }

        public abstract void OnSpawnOrdeal(RoR2.Stage stage);

        public void Create() {
            OrdealManager.ordeals[OrdealLevel].Add(this);
        }

        private Vector3[] PickValidPositions(Vector3 origin, float min, float max, NodeGraph.Node[] nodes)
            {
                List<Vector3> validPositions = new();

                foreach (NodeGraph.Node node in nodes)
                {
                    float distance = Vector3.Distance(node.position, origin);
                    if (distance > min && distance < max)
                    {
                        validPositions.Add(node.position);
                    }
                }

                if (validPositions.Count < 1)
                {
                    return new Vector3[] { origin };
                }

                return validPositions.ToArray();
            }

            public Vector3 PickSpawnPosition(Vector3 origin, float min, float max)
            {
                if (!SceneInfo.instance || !SceneInfo.instance.groundNodes)
                {
                    return origin;
                }

                NodeGraph.Node[] nodes = SceneInfo.instance.groundNodes.nodes;
                Vector3[] validPositions;
                validPositions = PickValidPositions(origin, min, max, nodes);
                return validPositions.GetRandom(Run.instance.spawnRng);
            }
    }

    public enum OrdealLevel {
        DAWN,
        NOON,
        DUSK,
        MIDNIGHT
    }
}