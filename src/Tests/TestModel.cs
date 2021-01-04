//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using OutwardBuildCalc.DB;
//using OutwardBuildCalc.DB.Model;
//using OutwardBuildCalc.DB.StaticData;
//using OutwardBuildCalc.Profile;
//using SideLoader;

//namespace OutwardBuildCalc.Tests
//{
//    public class TestModel
//    {
//        public string Name;
//        public List<string> Statuses;
//        public List<int> Passives;
//        public List<int> ImbueOptions;

//        public bool LimitOneEachWeapon = true;
//        public bool ConsiderHex = true;

//        private List<CharacterBuild> m_cachedBuilds;

//        private void CacheBuilds(EnemyModel enemy)
//        {
//            var statusModels = new List<StatusModel>();
//            foreach (var statusID in Statuses)
//            {
//                if (ResourcesPrefabManager.Instance.GetStatusEffectPrefab(statusID) is StatusEffect)
//                    statusModels.Add(Database.StatusModels[statusID]);
//            }
//            var passiveModels = new List<PassiveModel>();
//            foreach (var passiveID in this.Passives)
//            {
//                if (ResourcesPrefabManager.Instance.GetItemPrefab(passiveID) is PassiveSkill)
//                    passiveModels.Add(Database.PassiveModels[passiveID]);
//            }
//            var imbueModels = new List<ImbueModel>();
//            foreach (var imbueID in this.ImbueOptions)
//            {
//                if (ResourcesPrefabManager.Instance.GetEffectPreset(imbueID) is ImbueEffectPreset)
//                    imbueModels.Add(Database.ImbueModels[imbueID]);
//            }

//            // create build mutations

//            var builds = new List<CharacterBuild>()
//            {
//                new CharacterBuild
//                {
//                    Statuses = statusModels,
//                    Passives = passiveModels,
//                }
//            };

//            // SL.Log($"Mutating {this.ImbueOptions.Count} imbues, Builds count: {builds.Count}");
//            builds = MutateImbues(builds, imbueModels);

//            // SL.Log($"Mutating weapon setups (db count: {Database.WeaponModels.Count}), Builds count: {builds.Count}");
//            builds = MutateWeapons(builds);

//            // SL.Log($"Picking best gear, Builds count: {builds.Count}");
//            GetBestDamageBonusGear(builds, enemy);

//            m_cachedBuilds = builds;
//        }

//        public void RunSingleTest(EnemyModel enemy, bool forceRecache = false)
//        {
//            if (m_cachedBuilds == null || forceRecache)
//                CacheBuilds(enemy);

//            GetBestDamageBonusGear(m_cachedBuilds, enemy);

//            // check damage and dps against enemy model
//            SL.LogWarning("Checking " + m_cachedBuilds.Count + " builds...");

//            var results = new List<TestResult>();
//            foreach (var build in m_cachedBuilds)
//            {
//                var dmg = build.GetWeaponDamage(enemy);

//                var dps = build.GetWeaponDamage(enemy, true) * (float)(1 / (decimal)AttackSpeedData.GetTimePerAttack(build.MainWeapon, build.Imbue, enemy));

//                var result = new TestResult(build, this, dmg, dps);

//                results.Add(result);
//            }

//            var highestDmgALL = results.OrderByDescending(it => it.TotalDamage).ToArray();
//            var highestDPSALL = results.OrderByDescending(it => it.DamagePerSecond).ToArray();

//            var highestDmg = new List<TestResult>();
//            var highestDPS = new List<TestResult>();
//            int checkedResults = 0;
//            while (checkedResults < results.Count && highestDmg.Count < 20 && highestDPS.Count < 20)
//            {
//                var highDmg = highestDmgALL[checkedResults];

//                if (!LimitOneEachWeapon || !highestDmg.Where(it => it.RefBuild.MainWeapon.Name == highDmg.RefBuild.MainWeapon.Name).Any())
//                    highestDmg.Add(highDmg);

//                var highDps = highestDPSALL[checkedResults];

//                if (!LimitOneEachWeapon || !highestDPS.Where(it => it.RefBuild.MainWeapon.Name == highDps.RefBuild.MainWeapon.Name).Any())
//                    highestDPS.Add(highDps);

//                checkedResults++;
//            }

//            // Build log for results

//            string statuses = "";
//            for (int i = 0; i < Statuses.Count; i++)
//            {
//                if (i > 0) statuses += ", ";
//                statuses += Statuses[i];
//            }
//            string passives = "";
//            for (int i = 0; i < Passives.Count; i++)
//            {
//                if (i > 0) passives += ", ";
//                passives += ResourcesPrefabManager.Instance.GetItemPrefab(Passives[i]).Name;
//            }

//            SL.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
//            SL.LogWarning("TEST RESULTS: " + Name);
//            SL.Log("Enemy: " + enemy.ToString());
//            SL.Log("Passives: " + passives);
//            SL.Log("Statuses: " + statuses);
//            SL.Log("---------------------------------");
//            SL.Log("HIGHEST DAMAGE-PER-SECOND:");
//            foreach (var build in highestDPS)
//                SL.Log(build.ToString());
//            SL.Log("---------------------------------");
//            SL.Log("HIGHEST HIT-DAMAGE:");
//            foreach (var build in highestDmg)
//                SL.Log(build.ToString());
//            SL.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
//        }

//        public void RunMultiTest(List<EnemyModel> enemies, bool forceRecache = false)
//        {
//            Dictionary<EnemyModel, TestResult> bestDamages = new Dictionary<EnemyModel, TestResult>();
//            Dictionary<EnemyModel, TestResult> bestDPS = new Dictionary<EnemyModel, TestResult>();

//            foreach (var enemy in enemies)
//            {
//                if (m_cachedBuilds == null || forceRecache)
//                    CacheBuilds(enemy);

//                GetBestDamageBonusGear(m_cachedBuilds, enemy);

//                // check damage and dps against enemy model
//                SL.LogWarning("Checking " + m_cachedBuilds.Count + " builds for enemy " + enemy.Name + "...");

//                var results = new List<TestResult>();
//                foreach (var build in m_cachedBuilds)
//                {
//                    var dmg = build.GetWeaponDamage(enemy);

//                    var dps = build.GetWeaponDamage(enemy, true) * (float)(1 / (decimal)AttackSpeedData.GetTimePerAttack(build.MainWeapon, build.Imbue, enemy));

//                    var result = new TestResult(build, this, dmg, dps);

//                    results.Add(result);
//                }

//                bestDamages.Add(enemy, results.OrderByDescending(it => it.TotalDamage).First());
//                bestDPS.Add(enemy, results.OrderByDescending(it => it.DamagePerSecond).First());
//            }

//            // Build log for results

//            string statuses = "";
//            for (int i = 0; i < Statuses.Count; i++)
//            {
//                if (i > 0) statuses += ", ";
//                statuses += Statuses[i];
//            }
//            string passives = "";
//            for (int i = 0; i < Passives.Count; i++)
//            {
//                if (i > 0) passives += ", ";
//                passives += ResourcesPrefabManager.Instance.GetItemPrefab(Passives[i]).Name;
//            }

//            SL.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
//            SL.LogWarning("TEST RESULTS: " + Name);
//            SL.Log("Passives: " + passives);
//            SL.Log("Statuses: " + statuses);

//            for (int i = 0; i < bestDamages.Count; i++)
//            {
//                var highestDmg = bestDamages.ElementAt(i);
//                var highestDPS = bestDPS.ElementAt(i);
//                var enemy = highestDmg.Key;

//                SL.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
//                SL.Log("Enemy: " + enemy.ToString());
//                SL.Log("---------------------------------");
//                SL.Log("HIGHEST DAMAGE-PER-SECOND:");
//                SL.Log(highestDPS.Value.ToString());
//                SL.Log("---------------------------------");
//                SL.Log("HIGHEST HIT-DAMAGE:");
//                SL.Log(highestDmg.Value.ToString());
//                SL.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
//            }
//        }

//        private List<CharacterBuild> MutateImbues(List<CharacterBuild> list, List<ImbueModel> options)
//        {
//            var newList = new List<CharacterBuild>();

//            foreach (var imbue in options)
//            {
//                foreach (var entry in list)
//                {
//                    var clone = entry.Clone();
//                    clone.Imbue = imbue;
//                    newList.Add(clone);
//                }
//            }

//            return newList;
//        }

//        private List<CharacterBuild> MutateWeapons(List<CharacterBuild> list)
//        {
//            var newList = new List<CharacterBuild>();

//            foreach (var weapon in Database.WeaponModels)
//            {
//                foreach (var entry in list)
//                {
//                    var clone = entry.Clone();
//                    clone.MainWeapon = weapon;
//                    newList.Add(clone);
//                }
//            }

//            return newList;
//        }

//        private void GetBestDamageBonusGear(List<CharacterBuild> list, EnemyModel enemy)
//        {
//            foreach (var build in list)
//            {
//                var weapon = build.MainWeapon;

//                if (weapon.Type.ToString().Contains("1H"))
//                    PickBestOffhand(build, enemy);

//                PickBestHelmet(build, enemy);
//                PickBestChest(build, enemy);
//                PickBestBoots(build, enemy);
//                PickBestBackpack(build, enemy);
//            }
//        }

//        private void PickBestOffhand(CharacterBuild build, EnemyModel enemy)
//        {
//            float highest = 0f;
//            foreach (var option in Database.OffhandModels)
//            {
//                var clone = build.Clone();
//                clone.Offhand = option;

//                float newDmg = clone.GetWeaponDamage(enemy);
//                if (newDmg > highest)
//                {
//                    build.Offhand = option;
//                    highest = newDmg;
//                }
//            }
//        }

//        private void PickBestHelmet(CharacterBuild build, EnemyModel enemy)
//        {
//            float highest = 0f;
//            foreach (var option in Database.HelmetModels)
//            {
//                var clone = build.Clone();
//                clone.Helmet = option;

//                float newDmg = clone.GetWeaponDamage(enemy);
//                if (newDmg > highest)
//                {
//                    build.Helmet = option;
//                    highest = newDmg;
//                }
//            }
//        }

//        private void PickBestChest(CharacterBuild build, EnemyModel enemy)
//        {
//            float highest = 0f;
//            foreach (var option in Database.ChestModels)
//            {
//                var clone = build.Clone();
//                clone.Chest = option;
//                float newDmg = clone.GetWeaponDamage(enemy);
//                if (newDmg > highest)
//                {
//                    build.Chest = option;
//                    highest = newDmg;
//                }
//            }
//        }

//        private void PickBestBoots(CharacterBuild build, EnemyModel enemy)
//        {
//            float highest = 0f;
//            foreach (var option in Database.BootsModels)
//            {
//                var clone = build.Clone();
//                clone.Boots = option;
//                float newDmg = clone.GetWeaponDamage(enemy);
//                if (newDmg > highest)
//                {
//                    build.Boots = option;
//                    highest = newDmg;
//                }
//            }
//        }

//        private void PickBestBackpack(CharacterBuild build, EnemyModel enemy)
//        {
//            float highest = float.MinValue;
//            foreach (var option in Database.BackpackModels)
//            {
//                var clone = build.Clone();
//                clone.Backpack = option;
//                float newDmg = clone.GetWeaponDamage(enemy);
//                if (newDmg > highest)
//                {
//                    build.Backpack = option;
//                    highest = newDmg;
//                }
//            }
//        }
//    }
//}
