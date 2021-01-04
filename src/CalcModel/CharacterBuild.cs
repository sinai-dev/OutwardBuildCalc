using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.DB.Model;
using OutwardBuildCalc.DB.StaticData;
using SideLoader;

namespace OutwardBuildCalc.CalcModel
{
    public class CharacterBuild
    {
        public WeaponModel MainWeapon;
        public ImbueModel Imbue;
        public EquipmentModel Offhand;

        public EquipmentModel Helmet;
        public EquipmentModel Chest;
        public EquipmentModel Boots;
        public EquipmentModel Backpack;

        public List<StatusModel> Statuses = new List<StatusModel>();
        public List<PassiveModel> Passives = new List<PassiveModel>();

        public CharacterBuild Clone() => (CharacterBuild)this.MemberwiseClone();

        private IEnumerable<DamageType.Types> m_cachedUsedTypes;

        public IEnumerable<DamageType.Types> GetDamageTypes()
        {
            if (m_cachedUsedTypes != null)
                return m_cachedUsedTypes;

            var list = new HashSet<DamageType.Types>();

            list.UnionWith(MainWeapon.BaseDamage.List.Select(it => it.Type));
            list.UnionWith(MainWeapon.EnchantmentDamage.List.Select(it => it.Type));
            list.UnionWith(MainWeapon.EnchantmentBlastDamage.List.Select(it => it.Type));

            return m_cachedUsedTypes = list;
        }

        public DamageList GetWeaponDamage(EnemyModel enemy, Dictionary<string, float[]> naturalHexes, bool useWeaponHex, bool useAxeSpecial = false) 
        {
            float weaponMulti = 1.0f;
            if (useAxeSpecial && (MainWeapon.Type == Weapon.WeaponType.Axe_1H || MainWeapon.Type == Weapon.WeaponType.Axe_2H))
                weaponMulti = 1.3f;

            float[] damages = new float[9];
            for (int i = 0; i < 9; i++)
            {
                var type = (DamageType.Types)i;

                float dmgBonus = GetDamageMultiplier(type);

                if (MainWeapon.BaseDamage.TryGet(type, out DamageType dType))
                    damages[i] += dType.Damage * weaponMulti * dmgBonus;
                
                if (MainWeapon.EnchantmentDamage.TryGet(type, out DamageType eType))
                    damages[i] += eType.Damage * dmgBonus;

                if (MainWeapon.EnchantmentBlastDamage.TryGet(type, out DamageType bType))
                    damages[i] += bType.Damage * dmgBonus;

                if (!string.IsNullOrEmpty(Imbue.Name))
                {
                    if (type != DamageType.Types.Count && Imbue.OverrideType == type)
                    {
                        var totalBase = MainWeapon.BaseDamage.TotalDamage + MainWeapon.EnchantmentDamage.TotalDamage;
                        damages[i] += totalBase * Imbue.DamageMultiplier * dmgBonus;
                    }

                    if (Imbue.BonusDamage.TryGet(type, out DamageType iType))
                        damages[i] += iType.Damage * dmgBonus;
                }
            }

            // calculate total hexes without doubling any

            var totalHex = new float[9];
            
            foreach (var entry in naturalHexes)
                for (int i = 0; i < 9; i++)
                    totalHex[i] += entry.Value[i];
            
            if (useWeaponHex)
            {
                foreach (var entry in MainWeapon.m_alreadyAppliedHexes.Where(it => !naturalHexes.ContainsKey(it.Key)))
                    for (int i = 0; i < 9; i++)
                        totalHex[i] += entry.Value[i];
            }

            // build output damage against enemy resistances

            DamageList dmgList = new DamageList();

            for (int i = 0; i < 9; i++)
            {
                var dmg = (damages[i] - enemy.DamageProtection[i]) * (1 - (enemy.DamageResistance[i] + totalHex[i]));
                if (dmg > 0)
                    dmgList.Add(new DamageType((DamageType.Types)i, dmg));
            }

            return dmgList;
        }

        public float GetDamageMultiplier(DamageType.Types type)
        {
            float multiplier = 1.0f;

            if (MainWeapon.DamageBonus != null)
                multiplier += MainWeapon.DamageBonus[(int)type];

            if (!MainWeapon.TwoHanded && Offhand.DamageBonus != null)
                multiplier += Offhand.DamageBonus[(int)type];

            if (Helmet.DamageBonus != null)
                multiplier += Helmet.DamageBonus[(int)type];

            if (Chest.DamageBonus != null)
                multiplier += Chest.DamageBonus[(int)type];

            if (Boots.DamageBonus != null)
                multiplier += Boots.DamageBonus[(int)type];

            if (Backpack.DamageBonus != null)
                multiplier += Backpack.DamageBonus[(int)type];

            if (Statuses != null)
                foreach (var status in Statuses)
                    multiplier += status.DamageBonus[(int)type] + status.AllDamageBonus;

            if (Passives != null)
                foreach (var passive in Passives)
                    multiplier += passive.DamageBonus[(int)type] + passive.AllDamageBonus;

            return multiplier;
        }
    }
}
