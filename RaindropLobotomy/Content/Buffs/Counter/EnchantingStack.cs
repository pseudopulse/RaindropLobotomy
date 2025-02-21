using System;
using RaindropLobotomy.EGO.Commando;
using RaindropLobotomy.EGO.Mage;

namespace RaindropLobotomy.Buffs {
    public class EnchantingStack : BuffBase<EnchantingStack>
    {
        public override BuffDef Buff => Load<BuffDef>("bdEnchantStack.asset");

        public override void PostCreation()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += OnHitEnemy;
        }

        private void OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.HasModdedDamageType(EGOLamp.Enchanting) && victim) {
                CharacterBody body = victim.GetComponent<CharacterBody>();

                if (body && !body.HasBuff(Buffs.Enchanted.BuffIndex)) {
                    int count = body.GetBuffCount(Buff);
                    if (DarknessController.DarknessActive) {
                        count++;
                    }
                    
                    body.SetBuffCount(Buff.buffIndex, count + 1);

                    int neededCount = body.isBoss ? 15 : 5;

                    if (count + 1 >= neededCount) {
                        body.AddTimedBuff(Buffs.Enchanted.Instance.Buff, 7.5f, 1);
                        body.SetBuffCount(Buff.buffIndex, 0);
                    }
                }
            }
        }
    }
}