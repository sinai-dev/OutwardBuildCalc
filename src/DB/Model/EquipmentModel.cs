using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.CalcModel;
using SideLoader;

namespace OutwardBuildCalc.DB.Model
{
    public struct EquipmentModel : IEquipmentModel
    {
        public string Description => $"{Name}{(string.IsNullOrEmpty(Enchant) ? "" : $" ({Enchant})")}";

        public string Name;
        public int ItemID;
        public string Enchant;
        public int? EnchantID;
        public float[] DamageBonus;

        public EquipmentModel(Equipment equipment, Enchantment enchantment)
        {
            Name = equipment.Name.Trim();
            ItemID = equipment.ItemID;
            Enchant = enchantment?.Name.Trim();
            EnchantID = enchantment?.PresetID;
            DamageBonus = new float[9];

            for (int i = 0; i < 9; i++)
            {
                float bonus = equipment.GetDamageAttack((DamageType.Types)i);
                if (enchantment && enchantment.DamageModifier.TryGet((DamageType.Types)i, out DamageType dType))
                    bonus += dType.Damage;

                DamageBonus[i] = bonus * 0.01f;
            }
        }

        public float GetTotalDamageBonus()
        {
            float ret = 0f;
            foreach (float bonus in this.DamageBonus)
                ret += bonus;
            return ret;
        }
    }
}
