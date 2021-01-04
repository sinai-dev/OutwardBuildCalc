using SideLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardBuildCalc.DB.Model
{
    public struct StatusModel
    {
        public string Name;
        public string IdentifierName;
        public float AllDamageBonus;
        public float[] DamageBonus;
        public float AttackSpeedModifier;

        public StatusModel(StatusEffect effect)
        {
            Name = effect.StatusName.Trim();
            IdentifierName = effect.IdentifierName;
            AllDamageBonus = 0f;
            DamageBonus = new float[9];
            AttackSpeedModifier = 0f;

            if (IdentifierName == "Craze")
            {
                Name = "Craze (5)";
                AllDamageBonus = 0.25f;
                return;
            }

            var effects = effect.GetComponentsInChildren<Effect>(true);

            for (int i = 0; i < effects.Length && i < effect.StatusData.EffectsData.Length; i++)
            {
                var affectStat = effects[i] as AffectStat;
                if (!affectStat)
                    continue;

                var data = effect.StatusData.EffectsData[i].Data;

                if (Enum.TryParse(affectStat.AffectedStat?.Tag.TagName, out CharacterStats.StatType statType))
                {
                    float val = float.Parse(data[0]) * 0.01f;

                    switch (statType)
                    {
                        case CharacterStats.StatType.AttackSpeed:
                            AttackSpeedModifier = val; break;
                        case CharacterStats.StatType.AllDamages:
                            AllDamageBonus += val; break;
                        case CharacterStats.StatType.PhysicalDamage:
                        case CharacterStats.StatType.EtherealDamage:
                        case CharacterStats.StatType.DecayDamage:
                        case CharacterStats.StatType.ElectricDamage:
                        case CharacterStats.StatType.FrostDamage:
                        case CharacterStats.StatType.FireDamage:
                            DamageBonus[(int)statType - 15] += val;
                            break;
                        case CharacterStats.StatType.ElementalDmgModifier:
                            for (int j = 1; j < 6; j++)
                                DamageBonus[j] += val;
                            break;
                    }
                }
            }

            // end ctor
        }
    }
}
