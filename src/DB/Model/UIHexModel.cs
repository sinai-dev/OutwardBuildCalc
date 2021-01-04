using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardBuildCalc.DB.Model
{
    // Temporary manual implementation. Only used by UI for now.

    // TODO: 
    // - build from DB automaticaly
    // - implement properly into WeaponModel and BuildCalcProfile (instead of using float[] directly)
    // - rename to HexModel when done

    public struct UIHexModel
    {
        public string IdentifierName;
        public float[] DamageModifiers;

        public UIHexModel(string name, float[] damageModifiers)
        {
            this.IdentifierName = name;
            this.DamageModifiers = damageModifiers;
        }

        public static bool operator !=(UIHexModel lhs, UIHexModel rhs) => lhs.IdentifierName != rhs.IdentifierName;
        public static bool operator ==(UIHexModel lhs, UIHexModel rhs) => lhs.IdentifierName == rhs.IdentifierName;
        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        {
            if (!(obj is UIHexModel hex))
                return false;
            return hex.IdentifierName == this.IdentifierName;
        }


        public static Dictionary<string, UIHexModel> Hexes = new Dictionary<string, UIHexModel>
        {
            { "Pain",    new UIHexModel("Pain",    new float[] { -0.25f, 0,0,0,0,0,0,0,0 }) },
            { "Haunted", new UIHexModel("Haunted", new float[] { 0,-0.25f, 0,0,0,0,0,0,0 }) },
            { "Curse",   new UIHexModel("Curse",   new float[] { 0,0,-0.25f, 0,0,0,0,0,0 }) },
            { "Doom",    new UIHexModel("Doom",    new float[] { 0,0,0,-0.25f, 0,0,0,0,0 }) },
            { "Chill",   new UIHexModel("Chill",   new float[] { 0,0,0,0,-0.25f, 0,0,0,0 }) },
            { "Burn",    new UIHexModel("Burn",    new float[] { 0,0,0,0,0,-0.25f, 0,0,0 }) },

            { 
                "Elemental Vulnerability", 
                new UIHexModel("Elemental Vulnerability", 
                    new float[] { 0, -0.25f, -0.25f, -0.25f, -0.25f, -0.25f, 0,0,0 }) 
            },
        };
    }
}
