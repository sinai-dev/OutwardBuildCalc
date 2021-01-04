//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using OutwardBuildCalc.Profile;

//namespace OutwardBuildCalc.Tests
//{
//    public struct TestResult
//    {
//        public float TotalDamage;
//        public float DamagePerSecond;

//        public CharacterBuild RefBuild;
//        public TestModel RefModel;

//        public TestResult(CharacterBuild build, TestModel model, float totalDmg, float dps)
//        {
//            this.DamagePerSecond = dps;
//            this.TotalDamage = totalDmg;

//            this.RefBuild = build;
//            this.RefModel = model;
//        }

//        public override string ToString()
//        {
//            return $"Weapon: {RefBuild.MainWeapon.Description}, " +
//                $"{RefBuild.Imbue.Name}" +
//                $"{(string.IsNullOrEmpty(RefBuild.Offhand.Name) ? "" : $", {RefBuild.Offhand.Description}")}" +
//                $", {RefBuild.Helmet.Description}, " +
//                $"{RefBuild.Chest.Description}, " +
//                $"{RefBuild.Boots.Description}, " +
//                $"{RefBuild.Backpack.Description}\r\n" +
//                $"Damage: {TotalDamage} | DPS: {DamagePerSecond}";
//        }
//    }
//}
