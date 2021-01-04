using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.DB.Model;
using OutwardBuildCalc.CalcModel;

namespace OutwardBuildCalc.DB.StaticData
{
    public static class AttackSpeedData
    {
        //public static bool AxesUseSpecials = true;

        public static float GetTimePerAttack(CharacterBuild build, EnemyModel enemy, bool useAxeSpecial)
        {
            if (build.MainWeapon.Type == Weapon.WeaponType.Bow)
                return 1.445f;

            float multiplier = 1 + build.Imbue.AttackSpeedModifier;
            foreach (var status in build.Statuses)
                multiplier += status.AttackSpeedModifier;

            return GetTimePerAttack(build.MainWeapon.Type, build.MainWeapon.AttackSpeed * multiplier, useAxeSpecial, enemy.ColliderRadius);
        }

        private static float GetTimePerAttack(Weapon.WeaponType type, float speed, bool useAxeSpecial, float colliderRadius = 0.4f)
        {
            if (speed == 0f)
                return 999f;

            Dictionary<Weapon.WeaponType, float[]> dict;
            if (useAxeSpecial && (type == Weapon.WeaponType.Axe_1H || type == Weapon.WeaponType.Axe_2H))
                dict = SpecialSpeeds;
            else
                dict = NormalSpeeds;

            if (!dict.ContainsKey(type))
                return 999f;

            float hitstop = (((colliderRadius - 0.4f) / 0.1f * 37) + 175) * 0.001f;

            if (speed > 1.2f)
            {
                // get 1.0x speed and auto-calculate from value
                var baseSpeed = dict[type][2];
                return hitstop + (baseSpeed * (float)(1 / (decimal)speed));
            }
            else if (speed > 1.1f)
                return hitstop + dict[type][4];
            else if (speed > 1.0f)
                return hitstop + dict[type][3];
            else if (speed > 0.9f)
                return hitstop + dict[type][2];
            else if (speed > 0.8f)
                return hitstop + dict[type][1];
            else
                return hitstop + dict[type][0];
        }

        public static readonly Dictionary<Weapon.WeaponType, float[]> NormalSpeeds = new Dictionary<Weapon.WeaponType, float[]>
        {
            //                                      0.8     0.9     1.0     1.1     1.2
            { Weapon.WeaponType.Sword_1H,   new[] { 0.775f, 0.693f, 0.626f, 0.569f, 0.521f } },
            { Weapon.WeaponType.Axe_1H,     new[] { 0.864f, 0.775f, 0.700f, 0.636f, 0.588f } },
            { Weapon.WeaponType.Mace_1H,    new[] { 1.011f, 0.897f, 0.815f, 0.742f, 0.684f } },
            { Weapon.WeaponType.FistW_2H,   new[] { 0.487f, 0.433f, 0.389f, 0.354f, 0.324f } },
            { Weapon.WeaponType.Halberd_2H, new[] { 0.994f, 0.894f, 0.806f, 0.732f, 0.672f } },
            { Weapon.WeaponType.Sword_2H,   new[] { 1.067f, 0.953f, 0.855f, 0.783f, 0.713f } },
            { Weapon.WeaponType.Axe_2H,     new[] { 1.037f, 0.929f, 0.834f, 0.768f, 0.701f } },
            { Weapon.WeaponType.Mace_2H,    new[] { 1.272f, 1.133f, 1.018f, 0.929f, 0.848f } },
            { Weapon.WeaponType.Spear_2H,   new[] { 0.934f, 0.833f, 0.750f, 0.682f, 0.625f } },
        };

        public static readonly Dictionary<Weapon.WeaponType, float[]> SpecialSpeeds = new Dictionary<Weapon.WeaponType, float[]>
        {
            { Weapon.WeaponType.Axe_1H, new[] { 0.925f, 0.825f, 0.744f, 0.699f, 0.618f } },
            { Weapon.WeaponType.Axe_2H, new[] { 1.043f, 0.943f, 0.841f, 0.762f, 0.717f } },
        };
    }
}
