using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using OutwardBuildCalc.DB.Model;
using OutwardBuildCalc.DB.StaticData;
using SideLoader;
using SlotID = EquipmentSlot.EquipmentSlotIDs;

namespace OutwardBuildCalc.DB
{
    public static class Database
    {
        public static int[] ITEM_BLACKLIST = new int[] 
        {
            2300250, //Virgin Shield
            2020170, //CalixaMaceGun 
            2400540, //WolfgangRunicSword
            2100221, //Great Bloodsword
            2000221, //Blood Sword
            2130230, //Blood Spear
            2150042, // Elite trog queen staff
            2000141, // Cyrene's sword
            2110021,2110022,2110023, // NPC Marble Greataxes
            2010002, // Etheral axe
            2400532, // grandmother reach
        };

        public static List<WeaponModel> WeaponModels = new List<WeaponModel>();
        public static List<EquipmentModel> OffhandModels = new List<EquipmentModel>();
        public static List<EquipmentModel> HelmetModels = new List<EquipmentModel>();
        public static List<EquipmentModel> ChestModels = new List<EquipmentModel>();
        public static List<EquipmentModel> BootsModels = new List<EquipmentModel>();
        public static List<EquipmentModel> BackpackModels = new List<EquipmentModel>();

        // Finding relevant damage-bonus gear can be slow, so I cache as much as possible into a dictionary.

        private static readonly Dictionary<SlotID, Dictionary<DamageType.Types, List<EquipmentModel>>> s_cachedDmgBonusLists
            = new Dictionary<SlotID, Dictionary<DamageType.Types, List<EquipmentModel>>>();

        public static IEnumerable<EquipmentModel> GetDamageBonusEquipment(SlotID equipType, DamageType.Types dmgType)
        {
            if (!s_cachedDmgBonusLists.ContainsKey(equipType))
            {
                List<EquipmentModel> fullList = null;
                switch (equipType)
                {
                    case SlotID.LeftHand:
                        fullList = OffhandModels; break;
                    case SlotID.Helmet:
                        fullList = HelmetModels; break;
                    case SlotID.Chest:
                        fullList = ChestModels; break;
                    case SlotID.Foot:
                        fullList = BootsModels; break;
                    case SlotID.Back:
                        fullList = BackpackModels; break;
                }

                s_cachedDmgBonusLists.Add(equipType, new Dictionary<DamageType.Types, List<EquipmentModel>>());
                
                foreach (var opt in fullList)
                {
                    if (HasAnyDamageBonus(opt, out List<DamageType.Types> typeList))
                    {
                        foreach (var type in typeList)
                        {
                            if (!s_cachedDmgBonusLists[equipType].ContainsKey(type))
                                s_cachedDmgBonusLists[equipType].Add(type, new List<EquipmentModel>());

                            s_cachedDmgBonusLists[equipType][type].Add(opt);
                        }
                    }
                }
            }

            if (!s_cachedDmgBonusLists[equipType].ContainsKey(dmgType))
                return Enumerable.Empty<EquipmentModel>();

            return s_cachedDmgBonusLists[equipType][dmgType];
        }

        public static Dictionary<int, ImbueModel> ImbueModels = new Dictionary<int, ImbueModel>();
        public static Dictionary<string, StatusModel> StatusModels = new Dictionary<string, StatusModel>();
        public static Dictionary<int, PassiveModel> PassiveModels = new Dictionary<int, PassiveModel>();

        public static List<EnemyModel> EnemyModels;

        public static void BuildDB()
        {
            BuildImbueDB();
            BuildPassivesDB();
            BuildStatusDB();

            BuildWeaponDB();
            BuildOffhandDB();
            BuildEquipmentDB();

            //BuildEnemies();
        }

        //private static void BuildEnemies()
        //{
        //    var list = new List<EnemyModel>();

        //    var enemyFolder = Paths.PluginPath + @"\BuildCalc\Enemies";
        //    if (Directory.Exists(enemyFolder))
        //    {
        //        foreach (var path in Directory.GetFiles(enemyFolder, "*.xml"))
        //        {
        //            using (var file = File.OpenRead(path))
        //            {
        //                var xml = (DM_Enemy)Dataminer.Serializer.GetXmlSerializer(typeof(DM_Enemy)).Deserialize(file);
        //                if (xml != null)
        //                {
        //                    list.Add(new EnemyModel(xml));
        //                }
        //            }
        //        }
        //    }

        //    EnemyModels = list;
        //}

        private static void BuildImbueDB()
        {
            int[] whitelist = new int[] { 203, 205, 207, 208, 209, 211, 217, 218, 222, 223, 219 };

            foreach (var imbue in References.RPM_EFFECT_PRESETS.Values
                                            .Where(it => it is ImbueEffectPreset && whitelist.Contains(it.PresetID))
                                            .Select(it => it as ImbueEffectPreset))
            {
                ImbueModels.Add(imbue.PresetID, new ImbueModel(imbue));
            }
        }

        private static void BuildPassivesDB()
        {
            int[] whitelist = new int[] { 8205080, 8205350, 8202005, 8205240, 8205999, 8202000, 8202001 }; // 8202002 Patience (cant do yet)

            foreach (var passive in References.RPM_ITEM_PREFABS.Values
                .Where(it => whitelist.Contains(it.ItemID) && (it is PassiveSkill || it is NeedPassiveSkill))
                .Select(it => it as Skill))
            {
                PassiveModels.Add(passive.ItemID, new PassiveModel(passive));
            }
        }

        private static void BuildStatusDB()
        {
            string[] whitelist = new string[]
            {
                "Discipline", "Mist", "Bless", "Possessed", "Cool", "Warm",
                "Discipline Amplified", "Mist Amplified",  "Bless Amplified",
                "Possessed Amplified", "Cool Amplified", "Warm Amplified",
                "Shimmer", "Attack Up", "Calygrey Sleep", "Immolate",
                "Gift of Blood", "Defiled Positive", "Defiled Negative",
                "Lockwell's Revelation Very Tired", "Craze", "KillStreak",
                "Fire Totem Sleep", "Ice Totem Sleep", "Light Totem Sleep",
                "Decay Totem Sleep", "Ethereal Totem Sleep"
            };

            foreach (var status in References.RPM_STATUS_EFFECTS.Where(it => whitelist.Contains(it.Value.IdentifierName)))
            {
                StatusModels.Add(status.Key, new StatusModel(status.Value));
            }
        }

        public static List<int> GetCompatibleEnchantments(Equipment equipment)
        {
            var list = new List<int>();

            foreach (var enchantment in References.ENCHANTMENT_RECIPES.Values)
            {
                var result = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantment.RecipeID);

                if (IsRelevant(equipment, result) && enchantment.CompatibleEquipments.Match(equipment)) 
                    list.Add(enchantment.ResultID);
            }

            return list;
        }

        public static bool IsRelevant(Equipment equipment, Enchantment enchantment)
        {
            bool ret = (enchantment.AdditionalDamages != null && enchantment.AdditionalDamages.Length > 0)
                || (enchantment.DamageBonus != null && enchantment.DamageBonus.Count > 0)
                || (enchantment.DamageModifier != null && enchantment.DamageModifier.Count > 0);

            if (!ret)
            {
                if (equipment.HasTag(TagSourceManager.MainWeapon))
                {
                    ret = (bool)enchantment.Effects?.Where(it => it is ShootEnchantmentBlast).Any();

                    if (!ret)
                        ret = (bool)enchantment.Effects?.Where(it => it is AddStatusEffectBuildUp).Any();
                }
            }

            return ret;
        }

        public static bool ShouldSkip(Equipment equipment)
        {
            return !equipment.Stats
                    || equipment.ItemID < 2000000
                    || ITEM_BLACKLIST.Contains(equipment.ItemID)
                    || equipment.HasTag(TagSourceManager.Instance.GetTag("196")) // MonsterWeapon
                    || (int)equipment.RequiredPType == 2
                    || equipment.Name.Trim() == "-"
                    || equipment.Name.Contains("Stat Boost")
                    || equipment.Name.ToLower().Contains("removed");
        }

        private static void BuildWeaponDB()
        {
            foreach (var weapon in References.RPM_ITEM_PREFABS.Values.Where(it => it is Weapon).Select(it => it as Weapon))
            {
                if (ShouldSkip(weapon) || !weapon.HasTag(TagSourceManager.MainWeapon))
                    continue;

                WeaponModels.Add(new WeaponModel(weapon, null));

                foreach (var enchantID in GetCompatibleEnchantments(weapon))
                {
                    var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantID);

                    WeaponModels.Add(new WeaponModel(weapon, enchantment));
                }
            }
        }

        private static void BuildOffhandDB()
        {
            foreach (var offhand in References.RPM_ITEM_PREFABS.Values.Where(it => it is Equipment).Select(it => it as Equipment))
            {
                if (offhand.EquipSlot != SlotID.LeftHand || offhand.HasTag(TagSourceManager.MainWeapon))
                    continue;

                if (ShouldSkip(offhand))
                    continue;

                OffhandModels.Add(new EquipmentModel(offhand, null));

                foreach (var enchantID in GetCompatibleEnchantments(offhand))
                {
                    var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantID);
                    OffhandModels.Add(new EquipmentModel(offhand, enchantment));
                }
            }
        }

        private static void BuildEquipmentDB()
        {
            foreach (var equipment in References.RPM_ITEM_PREFABS.Values.Where(it => it is Equipment).Select(it => it as Equipment))
            {
                if (ShouldSkip(equipment))// || !HasAnyDamageBonus(equipment))
                    continue;

                switch (equipment.EquipSlot)
                {
                    case SlotID.Helmet:
                        AddEquipment(HelmetModels); break;
                    case SlotID.Chest:
                        AddEquipment(ChestModels); break;
                    case SlotID.Foot:
                        AddEquipment(BootsModels); break;
                    case SlotID.Back:
                        AddEquipment(BackpackModels); break;

                    default: continue;
                }

                void AddEquipment(List<EquipmentModel> list)
                {
                    list.Add(new EquipmentModel(equipment, null));

                    var enchants = GetCompatibleEnchantments(equipment);
                    if (enchants != null && enchants.Count > 0)
                    {
                        foreach (var enchantID in enchants)
                        {
                            var enchantment = ResourcesPrefabManager.Instance.GetEnchantmentPrefab(enchantID);
                            list.Add(new EquipmentModel(equipment, enchantment));
                        }
                    }
                }
            }
        }

        internal static bool HasAnyDamageBonus(EquipmentModel model) => HasAnyDamageBonus(model, out _);

        internal static bool HasAnyDamageBonus(EquipmentModel model, out List<DamageType.Types> types)
        {
            types = new List<DamageType.Types>();

            if (model.DamageBonus == null)
                return false;

            bool ret = false;
            for (int i = 0; i < 9; i++)
            {
                if (model.DamageBonus[i] > 0)
                {
                    ret = true;
                    types.Add((DamageType.Types)i);
                }
            }

            return ret;
        }

        internal static bool HasAnyDamageBonus(Equipment equipment)
        {
            for (int i = 0; i < 9; i++)
                if (equipment.GetDamageAttack((DamageType.Types)i) > 0)
                    return true;

            return false;
        }
    }
}
