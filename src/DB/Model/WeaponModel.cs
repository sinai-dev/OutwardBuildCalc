using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.CalcModel;
using SideLoader;

namespace OutwardBuildCalc.DB.Model
{
    public struct WeaponModel : IEquipmentModel
    {
        public string Description => $"{Name}{(EnchantID == null ? "" : $" ({Enchant})")}";

        public string Name;
        public int ItemID;
        public string Enchant;
        public int? EnchantID;

        public Weapon.WeaponType Type;
        public bool TwoHanded;
        public DamageList BaseDamage;
        public DamageList EnchantmentDamage;
        public DamageList EnchantmentBlastDamage;
        public float AttackSpeed;
        public float[] DamageBonus;
        public Dictionary<DamageType.Types, float> TotalHexModifier;

        internal readonly Dictionary<string, float[]> m_alreadyAppliedHexes;

        public float GetTotalDamage() => BaseDamage.TotalDamage + EnchantmentDamage.TotalDamage + EnchantmentBlastDamage.TotalDamage;

        public WeaponModel(Weapon weapon, Enchantment enchantment)
        {
            Name = weapon.Name.Trim();
            ItemID = weapon.ItemID;
            TwoHanded = weapon.TwoHanded;
            Type = weapon.Type;
            BaseDamage = weapon.Damage.Clone();
            AttackSpeed = weapon.GetAttackSpeed();

            m_alreadyAppliedHexes = new Dictionary<string, float[]>();

            if (weapon.HasTag(TagSourceManager.NonEnchantable) || weapon.Name.Contains("Runic "))
                enchantment = null;

            Enchant = enchantment?.Name.Trim();
            EnchantID = enchantment?.PresetID;
            EnchantmentDamage = new DamageList();

            DamageBonus = new float[9];
            for (int i = 0; i < 9; i++)
                DamageBonus[i] = weapon.GetDamageAttack((DamageType.Types)i) * 0.01f;

            TotalHexModifier = new Dictionary<DamageType.Types, float>();
            EnchantmentBlastDamage = new DamageList();

            BuildHexAffects(weapon, enchantment);
            BuildAoEBlastEffects(weapon, enchantment);

            if (enchantment)
                BuildEnchantmentDamage(enchantment);
        }

        public void BuildEnchantmentDamage(Enchantment enchantment)
        {
            AttackSpeed += enchantment.StatModifications.GetBonusValue(Enchantment.Stat.AttackSpeed);
            AttackSpeed *= 1 + (enchantment.StatModifications.GetModifierValue(Enchantment.Stat.AttackSpeed) * 0.01f);

            foreach (var additionalDamage in enchantment.AdditionalDamages)
            {
                if (BaseDamage[additionalDamage.SourceDamageType] != null)
                {
                    float damage = BaseDamage[additionalDamage.SourceDamageType].Damage * additionalDamage.ConversionRatio;
                    EnchantmentDamage.Add(new DamageType(additionalDamage.BonusDamageType, damage));
                }
            }

            foreach (DamageType dType in enchantment.DamageBonus.List)
                EnchantmentDamage.Add(dType);

            if (enchantment.DamageModifier != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    if (enchantment.DamageModifier.TryGet((DamageType.Types)i, out DamageType dType))
                        DamageBonus[i] += dType.Damage * 0.01f;
                }
            }
        }

        private void BuildAoEBlastEffects(Weapon weapon, Enchantment enchantment)
        {
            foreach (var blast in weapon.GetComponentsInChildren<ShootEnchantmentBlast>())
            {
                float dmg = BaseDamage.TotalDamage * blast.DamageMultiplier;
                var type = blast.BaseBlast.GetComponentInChildren<WeaponDamage>().OverrideDType;
                EnchantmentBlastDamage.Add(new DamageType(type, dmg));
            }

            if (enchantment && enchantment.Effects != null)
            {
                var shootBlasts = enchantment.Effects.Where(it => it is ShootEnchantmentBlast).Select(it => it as ShootEnchantmentBlast);
                foreach (var shootBlast in shootBlasts)
                {
                    float dmg = BaseDamage.TotalDamage * shootBlast.DamageMultiplier;
                    var type = shootBlast.BaseBlast.GetComponentInChildren<WeaponDamage>().OverrideDType;
                    EnchantmentBlastDamage.Add(new DamageType(type, dmg));
                }
            }
        }

        public void BuildHexAffects(Weapon weapon, Enchantment enchantment)
        {
            m_alreadyAppliedHexes.Clear();

            var addStatusEffects = weapon.GetComponentsInChildren<AddStatusEffectBuildUp>().ToList();
            if (enchantment && enchantment.Effects != null)
                addStatusEffects.AddRange(enchantment.Effects.Where(it => it as AddStatusEffectBuildUp).Select(it => it as AddStatusEffectBuildUp));

            foreach (var addStatus in addStatusEffects)
            {
                if (!addStatus.Status || m_alreadyAppliedHexes.ContainsKey(addStatus.Status.IdentifierName))
                    continue;

                var effects = addStatus.Status.GetComponentsInChildren<Effect>(true);

                float[] totalhex = new float[9];

                for (int i = 0; i < effects.Length && i < addStatus.Status.StatusData.EffectsData.Length; i++)
                {
                    if (effects[i] is AffectStat affectStat)
                    {
                        var data = addStatus.Status.StatusData.EffectsData[i].Data;

                        if (Enum.TryParse(affectStat.AffectedStat?.Tag.TagName, out CharacterStats.StatType statType))
                        {
                            float val = float.Parse(data[0]) * 0.01f;

                            switch (statType)
                            {
                                case CharacterStats.StatType.PhysicalResistance:
                                case CharacterStats.StatType.EtherealResistance:
                                case CharacterStats.StatType.DecayResistance:
                                case CharacterStats.StatType.ElectricResistance:
                                case CharacterStats.StatType.FrostResistance:
                                case CharacterStats.StatType.FireResistance:
                                    var type = (DamageType.Types)((int)statType - 35);

                                    totalhex[(int)type] += val;

                                    if (TotalHexModifier.ContainsKey(type))
                                        TotalHexModifier[type] += val;
                                    else
                                        TotalHexModifier.Add(type, val);
                                    break;
                            }
                        }
                    }
                }

                m_alreadyAppliedHexes.Add(addStatus.Status.IdentifierName, totalhex);
            }
        }
    }
}
