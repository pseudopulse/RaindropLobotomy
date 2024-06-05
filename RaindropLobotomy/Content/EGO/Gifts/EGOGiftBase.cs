using System;
using System.Reflection;

namespace RaindropLobotomy.EGO.Gifts {
    public abstract class EGOGiftBase<T> : EGOGiftBase where T : EGOGiftBase<T>
    {
        public static T Instance { get; private set; }

        public EGOGiftBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class EGOGiftBase {
        public abstract ItemDef ItemDef { get; }
        public abstract EquipmentDef EquipmentDef { get; }
        public static ItemTierDef EGOTier;

        public void Initialize() {
            if (!EGOTier) {
                EGOTier = Load<ItemTierDef>("ITD_Gift.asset");
                EGOTier.bgIconTexture = Assets.ItemTierDef.BossTierDef.bgIconTexture;
                EGOTier.dropletDisplayPrefab = Assets.ItemTierDef.BossTierDef.dropletDisplayPrefab;
                EGOTier.highlightPrefab = Assets.ItemTierDef.BossTierDef.highlightPrefab;

                ContentAddition.AddItemTierDef(EGOTier);
            }
            
            if (ItemDef) {
                ItemDef._itemTierDef = EGOTier;
                ItemDef.tier = ItemTier.AssignedAtRuntime;
                ContentAddition.AddItemDef(ItemDef);
            }

            if (EquipmentDef) {
                ContentAddition.AddEquipmentDef(EquipmentDef);
                EquipmentDef.isBoss = true;
                /// EquipmentDef.GetType().GetProperty("bgIconTexture", (BindingFlags)(-1)).SetValue(EquipmentDef, Assets.Texture2D.texBossBGIcon);
                On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformEquipmentAction;
            }

            Hooks();
            SetupLanguage();
        }

        public abstract void Hooks();
        public abstract void SetupLanguage();

        private bool PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, RoR2.EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (equipmentDef == EquipmentDef)
            {
                return ActivateEquipment(self);
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }

        public virtual bool ActivateEquipment(EquipmentSlot slot) {
            return false;
        }
    }

}