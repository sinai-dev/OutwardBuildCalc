using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.CalcModel;

namespace OutwardBuildCalc.DB.Model
{
    public struct ImbueModel
    {
        public string Description => Name;

        public string Name;
        public int PresetID;
        public float DamageMultiplier;
        public DamageType.Types OverrideType;
        public DamageList BonusDamage;
        public float AttackSpeedModifier;

        public ImbueModel(ImbueEffectPreset preset)
        {
            Name = preset.Name.Trim();
            PresetID = preset.PresetID;
            DamageMultiplier = 0f;
            OverrideType = DamageType.Types.Count;
            BonusDamage = new DamageList();
            AttackSpeedModifier = 0;

            if (preset.PresetID == 209)
                AttackSpeedModifier = 0.2f;
            else
            {
                if (preset.GetComponentInChildren<WeaponDamage>() is WeaponDamage weaponDamage)
                {
                    OverrideType = weaponDamage.OverrideDType;
                    DamageMultiplier = weaponDamage.WeaponDamageMult - 1;
                    BonusDamage = new DamageList(weaponDamage.Damages);
                }
            }
        }
    }
}
