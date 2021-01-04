using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.DB;
using OutwardBuildCalc.DB.Model;
using OutwardBuildCalc.DB.StaticData;
using SideLoader;
using SlotID = EquipmentSlot.EquipmentSlotIDs;

namespace OutwardBuildCalc.CalcModel
{
    public class BuildProfile
    {
        public BuildProfile()
        {
            m_customEnemy = new EnemyModel
            {
                Name = "Custom",
                DamageResistance = new float[9],
                DamageProtection = new float[9],
                Health = 1000f,
                ColliderRadius = 0.4f
            };
        }

        internal const float MIN_NAIVE_DPS = 30f;

        public bool AxesOnlyUseSpecial = true;
        public bool LimitOneEachWeapon = true;
        public bool ConsiderWeaponHex = true;

        public bool UseCustomEnemy = true;
        internal EnemyModel m_customEnemy;
        private EnemyModel m_selectedEnemy;
        public EnemyModel Enemy => UseCustomEnemy ? m_customEnemy : m_selectedEnemy;

        public void SetSelectedEnemy(EnemyModel enemy) => m_selectedEnemy = enemy;

        public Dictionary<string, float[]> NaturalHexes = new Dictionary<string, float[]>();

        public List<Weapon.WeaponType> WeaponBlacklist { get; internal set; }

        public List<ImbueModel> AllowedImbues = new List<ImbueModel>();
        public List<PassiveModel> ChosenPassives = new List<PassiveModel>();
        public List<StatusModel> ChosenStatuses = new List<StatusModel>();

        public WeaponModel? ChosenWeapon;
        public EquipmentModel? ChosenOffhand;
        public EquipmentModel? ChosenHelmet;
        public EquipmentModel? ChosenChest;
        public EquipmentModel? ChosenBoots;
        public EquipmentModel? ChosenBackpack;

        // API that the UI uses to gauge progress in calc
        internal static bool IsGenerating;
        internal static bool IsPickingGear;
        internal static bool IsCalculating;
        public static int GeneratedBuildCount { get; private set; }
        public static int GearPickProgress { get; private set; }

        public void Calculate(int resultLimit, out List<CalcResult> highestDmg, out List<CalcResult> highestDPS)
        {
            GearPickProgress = 0;
            GeneratedBuildCount = 0;
            IsCalculating = true;

            highestDmg = new List<CalcResult>();
            highestDPS = new List<CalcResult>();

            BuildCalcMod.Instance.StartCoroutine(CalculateCoroutine(resultLimit, highestDPS, highestDmg));
        }

        private IEnumerator CalculateCoroutine(int resultLimit, List<CalcResult> highestDPS, List<CalcResult> highestDmg)
        {
            // Generate build mutations
            IsGenerating = true;

            var builds = new List<CharacterBuild>
            {
                new CharacterBuild()
                {
                    Statuses = this.ChosenStatuses.ToList(),
                    Passives = this.ChosenPassives.ToList()
                }
            };

            if (AllowedImbues.Count > 0)
                builds = MutateImbues(builds, AllowedImbues);

            // generate weapons shortlist (no yield)

            var shortList = new List<WeaponModel>();
            SL.Log("checking weapon shortlist. Not allowed count: " + WeaponBlacklist.Count);
            foreach (var weapon in Database.WeaponModels)
            {
                if (WeaponBlacklist.Contains(weapon.Type))
                {
                    SL.Log("Skipping weapon type " + weapon.Type + " (" + weapon.Name + ")");
                    continue;
                }

                if (ChosenWeapon != null && weapon.ItemID != ChosenWeapon?.ItemID)
                    continue;
                if (ChosenWeapon?.EnchantID != null && weapon.EnchantID != ChosenWeapon?.EnchantID)
                    continue;
                if (ChosenOffhand != null && weapon.TwoHanded)
                    continue;

                // other basic weapon filtering would go here (eg Weapon Type filtering)

                shortList.Add(weapon);
            }

            var mutations = new List<CharacterBuild>();

            int itersThisYield = 0;
            foreach (var weapon in shortList)
            {

                //// filter out low damage results if weapon was not chosen
                //if (ChosenWeapon == null)
                //{
                //    var naiveDPS = weapon.GetTotalDamage() * (1 / AttackSpeedData.GetTimePerAttack(weapon.Type, weapon.AttackSpeed, AxesOnlyUseSpecial));
                //    if (naiveDPS < MIN_NAIVE_DPS)
                //        continue;
                //}

                foreach (var entry in builds)
                {
                    var clone = entry.Clone();
                    clone.MainWeapon = weapon;
                    mutations.Add(clone);

                    if (weapon.Type == Weapon.WeaponType.Bow)
                        clone.Imbue = default;

                    itersThisYield++;
                    GeneratedBuildCount++;
                }

                if (itersThisYield > 500)
                {
                    itersThisYield = 0;
                    yield return null;
                }
            }

            // pick best gear
            IsGenerating = false;
            IsPickingGear = true;

            itersThisYield = 0;

            foreach (var build in mutations)
            {
                if (ChosenOffhand != null)
                    build.Offhand = ChosenOffhand.Value;
                else if (!build.MainWeapon.TwoHanded)
                    PickBestEquipment(build, SlotID.LeftHand, (_build, model) => { _build.Offhand = model; });

                if (ChosenHelmet != null)
                    build.Helmet = ChosenHelmet.Value;
                else
                    PickBestEquipment(build, SlotID.Helmet, (_build, model) => { _build.Helmet = model; });

                if (ChosenChest != null)
                    build.Chest = ChosenChest.Value;
                else
                    PickBestEquipment(build, SlotID.Chest, (_build, model) => { _build.Chest = model; });

                if (ChosenBoots != null)
                    build.Boots = ChosenBoots.Value;
                else
                    PickBestEquipment(build, SlotID.Foot, (_build, model) => { _build.Boots = model; });

                if (ChosenBackpack != null)
                    build.Backpack = ChosenBackpack.Value;
                else
                    PickBestEquipment(build, SlotID.Back, (_build, model) => { _build.Backpack = model; });

                GearPickProgress++;
                itersThisYield++;
                if (itersThisYield > 500)
                {
                    itersThisYield = 0;
                    yield return null;
                }
            }

            // Calculate damage and dps against enemy, sort
            IsPickingGear = false;

            var results = new List<CalcResult>();
            itersThisYield = 0;
            foreach (var build in mutations)
            {
                var dmg = build.GetWeaponDamage(Enemy, NaturalHexes, ConsiderWeaponHex);

                float aps = (float)(1 / (decimal)AttackSpeedData.GetTimePerAttack(build, Enemy, AxesOnlyUseSpecial));
                var dps = aps * build.GetWeaponDamage(Enemy, NaturalHexes, ConsiderWeaponHex, AxesOnlyUseSpecial).TotalDamage;

                var result = new CalcResult { DPS = dps, Damage = dmg, RefBuild = build, }; // Enemy = enemyIsChanging ? Enemy.Name : null };
                results.Add(result);

                GeneratedBuildCount--;
                itersThisYield++;
                if (itersThisYield > 500)
                {
                    itersThisYield = 0;
                    yield return null;
                }
            }

            var highestDmgALL = results.OrderByDescending(it => it.Damage.TotalDamage).ToArray();
            var highestDPSALL = results.OrderByDescending(it => it.DPS).ToArray();

            int checkedResults = 0;
            while (checkedResults < results.Count && (highestDmg.Count < resultLimit || highestDPS.Count < resultLimit))
            {
                var highDmg = highestDmgALL[checkedResults];

                if (highestDmg.Count < resultLimit && (!LimitOneEachWeapon || !highestDmg.Where(it => it.RefBuild.MainWeapon.Name == highDmg.RefBuild.MainWeapon.Name).Any()))
                    highestDmg.Add(highDmg);

                var highDps = highestDPSALL[checkedResults];

                if (highestDPS.Count < resultLimit && (!LimitOneEachWeapon || !highestDPS.Where(it => it.RefBuild.MainWeapon.Name == highDps.RefBuild.MainWeapon.Name).Any()))
                    highestDPS.Add(highDps);

                checkedResults++;
            }

            IsCalculating = false;
        }

        private List<CharacterBuild> MutateImbues(List<CharacterBuild> list, List<ImbueModel> options)
        {
            var newList = new List<CharacterBuild>();

            foreach (var imbue in options)
            {
                foreach (var entry in list)
                {
                    var clone = entry.Clone();
                    clone.Imbue = imbue;
                    newList.Add(clone);
                }
            }

            return newList;
        }

        private void PickBestEquipment(CharacterBuild build, SlotID equipType, Action<CharacterBuild, EquipmentModel> setEquipment)
        {
            DamageList highest = null;
            EquipmentModel highestEquipment = default;
            var usedTypes = build.GetDamageTypes();

            var relevantGear = new List<EquipmentModel>();
            foreach (var type in usedTypes)
                relevantGear.AddRange(Database.GetDamageBonusEquipment(equipType, type));

            foreach (var option in relevantGear)
            {
                var clone = build.Clone();
                setEquipment.Invoke(clone, option);
                var newDmg = clone.GetWeaponDamage(Enemy, NaturalHexes, ConsiderWeaponHex);

                if (highest == null || newDmg.TotalDamage > highest.TotalDamage)
                {
                    highestEquipment = option;
                    highest = newDmg;
                }
            }

            setEquipment.Invoke(build, highestEquipment);
        }
    }
}
