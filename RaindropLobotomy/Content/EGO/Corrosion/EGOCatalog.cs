using System;

namespace RaindropLobotomy.EGO {
    public class EGOCatalog {
        public static List<EGODef> EGODefs = new();
        public static Dictionary<SurvivorDef, List<EGODef>> EGOMap = new();
        public static Dictionary<SurvivorDef, SurvivorDef> EGOReverseMap = new();

        public static void AddEGOCorrosion(EGODef egoDef) {
            if (EGODefs.Contains(egoDef)) return;

            if (!EGOMap.ContainsKey(egoDef.TargetSurvivor)) {
                EGOMap.Add(egoDef.TargetSurvivor, new List<EGODef>());
            }

            EGODefs.Add(egoDef);
            EGOMap[egoDef.TargetSurvivor].Add(egoDef);
            EGOReverseMap.Add(egoDef.Corrosion, egoDef.TargetSurvivor);
        }
    }
}