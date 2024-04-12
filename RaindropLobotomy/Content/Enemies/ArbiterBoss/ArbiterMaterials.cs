using System;

namespace RaindropLobotomy.Enemies.ArbiterBoss {
    public static class ArbiterMaterials {
        public static Material matArbiterSlashMat;
        public static void InitMaterials() {
            // slash
            matArbiterSlashMat = Load<Material>("matArbiterSlash.mat");
            matArbiterSlashMat.SetTexture("_MainTex", Assets.Texture2D.texClayBruiserDeathDecalMask);
            matArbiterSlashMat.SetTexture("_RemapTex", Assets.Texture2D.texRampShadowClone);
            matArbiterSlashMat.SetTexture("_Cloud1Tex", Assets.Texture2D.texCloudDirtyFire);
        }
    }
}