using System;

namespace RaindropLobotomy.EGO {
    public class EGODef : ScriptableObject {
        public SurvivorDef TargetSurvivor;
        public string Description;
        public SurvivorDef Corrosion;
        public UnlockableDef RequiredUnlock;
        public string DisplayName;
        public Sprite Icon;
        public Color Color;
    }
}