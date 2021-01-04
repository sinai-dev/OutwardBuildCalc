using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardBuildCalc.DB.Model
{
    public struct PassiveModel
    {
        public string Name;
        public int ItemID;
        public float AllDamageBonus;
        public float[] DamageBonus;
        //public float DPSDamageBonus;
        //public float SkillDamageBonus;

        public PassiveModel(Skill passive)
        {
            Name = passive.Name.Trim();
            ItemID = passive.ItemID;
            DamageBonus = new float[9];
            AllDamageBonus = 0f;

            //DPSDamageBonus = 0f;
            //SkillDamageBonus = 0f;

            //// manual fix for patience cause I havent properly implemented the api for it.
            //// This is (currently) the only case of such a mechanic.
            //if (Name == "Patience")
            //{
            //    DPSDamageBonus = -0.25f;
            //    SkillDamageBonus = 0.25f;
            //    return;
            //}

            foreach (var affect in passive.GetComponentsInChildren<AffectStat>())
            {
                if (Enum.TryParse(affect.AffectedStat?.Tag.TagName, out CharacterStats.StatType statType))
                {
                    float val = affect.Value * 0.01f;

                    switch (statType)
                    {
                        case CharacterStats.StatType.AllDamages:
                            AllDamageBonus += val; break;
                        case CharacterStats.StatType.PhysicalDamage:
                        case CharacterStats.StatType.EtherealDamage:
                        case CharacterStats.StatType.DecayDamage:
                        case CharacterStats.StatType.ElectricDamage:
                        case CharacterStats.StatType.FrostDamage:
                        case CharacterStats.StatType.FireDamage:
                            DamageBonus[(int)statType - 15] += val; break;
                        case CharacterStats.StatType.ElementalDmgModifier:
                            for (int j = 1; j < 6; j++)
                                DamageBonus[j] += val;
                            break;
                    }
                }
            }

            // use lockwell tired status
            //// Lockwell's revelation, unique edge case
            //if (passive.ItemID == 8201030)
            //{
            //    DamageBonus = new[]
            //    {
            //        0f,
            //        0.30f,
            //        0.30f,
            //        0.30f,
            //        0.30f,
            //        0.30f,
            //    };
            //}
            //else
            //{
            //    foreach (var affect in passive.GetComponentsInChildren<AffectStat>())
            //    {
            //        if (Enum.TryParse(affect.AffectedStat?.Tag.TagName, out CharacterStats.StatType statType))
            //        {
            //            float val = affect.Value * 0.01f;

            //            switch (statType)
            //            {
            //                case CharacterStats.StatType.AllDamages:
            //                    AllDamageBonus += val; break;
            //                case CharacterStats.StatType.PhysicalDamage:
            //                case CharacterStats.StatType.EtherealDamage:
            //                case CharacterStats.StatType.DecayDamage:
            //                case CharacterStats.StatType.ElectricDamage:
            //                case CharacterStats.StatType.FrostDamage:
            //                case CharacterStats.StatType.FireDamage:
            //                    DamageBonus[(int)statType - 15] += val; break;
            //            }
            //        }
            //    }
            //}
        }
    }
}
