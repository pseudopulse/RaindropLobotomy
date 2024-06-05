using System;
using System.Linq;

namespace RaindropLobotomy.Enemies {
    public interface Abnormality {
        RiskLevel ThreatLevel { get;}
        SpawnCard SpawnCard { get; }
        bool IsTool { get; }
    }

    public enum RiskLevel {
        Zayin = 0,
        Teth = 1,
        He = 2,
        Waw = 3,
        Aleph = 4
    }

    public static class AbnormalityManager {
        public static List<Abnormality> Abnormalities = new();
        //
        private static List<Abnormality> SpawnedLastStage = new();
        //
        private static int[] CreditMap = { 220, 420, 650, 1000, 2500 };
        private static DirectorCardCategorySelection AbnoDCCS;
        private static GameObject AbnoDirector;
        public static void Initialize() {
            AbnoDCCS = Load<DirectorCardCategorySelection>("AbnoDCCS.asset");
            AbnoDirector = Load<GameObject>("AbnormalityDirector.prefab");

            On.RoR2.CombatDirector.Awake += OnDirectorStart;
        }

        private static void OnDirectorStart(On.RoR2.CombatDirector.orig_Awake orig, CombatDirector self)
        {
            orig(self);

            if (!GameObject.Find("AbnormalityDirector(Clone)")) {
                GameObject dir = GameObject.Instantiate(AbnoDirector);
            }
        }

        public static void AddAbnormality(Abnormality abno) {
            Debug.Log("Adding abnormality: " + abno.SpawnCard);

            if (abno.IsTool) {
                HandleAbno_Tool(abno);
                return;
            }

            switch (abno.ThreatLevel) {
                case RiskLevel.Zayin:
                    break; // all of these are tools
                case RiskLevel.Teth:
                case RiskLevel.He:
                case RiskLevel.Waw:
                    HandleAbno_Standard(abno);
                    break;
                case RiskLevel.Aleph:
                    HandleAbno_Aleph(abno);
                    break;
            }
            
            abno.SpawnCard.directorCreditCost = CreditMap[(int)abno.ThreatLevel];
        }

        private static void HandleAbno_Aleph(Abnormality abno) {

        }

        private static void HandleAbno_Tool(Abnormality abno) {

        }

        private static void HandleAbno_Standard(Abnormality abno) {
            int index = (int)abno.ThreatLevel - 1;

            DirectorCard card = new();
            card.selectionWeight = 1;
            card.minimumStageCompletions = index;
            card.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;
            card.spawnCard = abno.SpawnCard;

            AbnoDCCS.AddCard(index, card);
        }
    }
}