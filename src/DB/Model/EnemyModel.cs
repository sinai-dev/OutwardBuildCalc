using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Dataminer;

namespace OutwardBuildCalc.DB.Model
{
    public struct EnemyModel
    {
        public string Name;
        public float Health;
        public float[] DamageResistance;
        public float[] DamageProtection;
        public float ColliderRadius;

        //public EnemyModel(DM_Enemy dataminedEnemy)
        //{
        //    Name = dataminedEnemy.Name + " (" + dataminedEnemy.GameObject_Name + ")";
        //    Health = dataminedEnemy.Max_Health;
        //    ColliderRadius = dataminedEnemy.Collider_Radius;

        //    DamageResistance = new float[9];
        //    DamageProtection = new float[9];

        //    for (int i = 0; i < 9; i++)
        //    {
        //        if (i < dataminedEnemy.Damage_Resistances.Length)
        //            DamageResistance[i] = dataminedEnemy.Damage_Resistances[i] * 0.01f;

        //        if (i < dataminedEnemy.Protection.Length)
        //            DamageProtection[i] = dataminedEnemy.Protection[i];
        //    }
        //}

        public EnemyModel(string name, float health, float[] damageResistances, float colliderRadius, float[] damageProtection = null)
        {
            Name = name;
            Health = health;
            DamageResistance = damageResistances;
            ColliderRadius = colliderRadius;
            DamageProtection = damageProtection ?? new float[9];
        }

        public override string ToString()
        {
            string ret = $"{this.Name} ({this.Health} HP";
            for (int i = 0; i < 9; i++)
            {
                if (DamageResistance[i] > 0f)
                    ret += $", {(DamageType.Types)i}: {DamageResistance[i]}";
            }
            ret += ")";

            return ret;
        }
    }
}
