using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.DB.Model;
using OutwardBuildCalc.UI;
using SideLoader;
using UnityEngine;

namespace OutwardBuildCalc.CalcModel
{
    public class CalcResult
    {
        public DamageList Damage;
        public float DPS;

        //public string Enemy;

        public CharacterBuild RefBuild;

        private bool m_isExpanded;
        private bool m_generatedLabels;
        private string m_weaponName;
        private string m_longDescription;

        public void OnGUI()
        {
            if (!m_generatedLabels)
                GenerateLabels();

            UIStyles.HorizontalLine(Color.grey);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(m_isExpanded ? "Less" : "More", GUILayout.Width(50)))
                m_isExpanded = !m_isExpanded;

            GUILayout.Label($"{m_weaponName} | <b>Damage:</b> {Damage.TotalDamage} | <b>DPS:</b> {DPS}");

            GUILayout.EndHorizontal();

            if (m_isExpanded)
                GUILayout.Label(m_longDescription);
        }

        private void GenerateLabels()
        {
            m_generatedLabels = true;

            m_weaponName = $"{RefBuild.MainWeapon.Description}";

            string dmgTypes = "";
            foreach (var type in Damage.List)
            {
                Color color = Color.white;
                switch (type.Type)
                {
                    case DamageType.Types.Ethereal:
                        color = Color.magenta; break;
                    case DamageType.Types.Decay:
                        color = Color.green; break;
                    case DamageType.Types.Electric:
                        color = Color.yellow; break;
                    case DamageType.Types.Frost:
                        color = Color.cyan; break;
                    case DamageType.Types.Fire:
                        color = Color.red; break;
                }
                if (dmgTypes != "") dmgTypes += ", ";
                dmgTypes += $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{type.Damage:F3} ({type.Type})</color>";
            }

            string passives = "";
            foreach (var passive in RefBuild.Passives)
            {
                if (passives != "")
                    passives += ", ";
                else
                    passives = $"\r\n\r\n<b>Passives</b>: ";
                passives += passive.Name;
            }

            string statuses = "";
            foreach (var status in RefBuild.Statuses)
            {
                if (statuses != "")
                    statuses += ", ";
                else
                    statuses = "\r\n\r\n<b>Statuses</b>: ";
                statuses += status.Name;
            }

            string bonuses = "";
            for (int i = 0; i < 6; i++)
            {
                if (i > 0) bonuses += ", ";
                bonuses += $"{(DamageType.Types)i}: {100f * (RefBuild.GetDamageMultiplier((DamageType.Types)i) - 1):F0}%";
            }

            string imbue = "";
            if (!string.IsNullOrEmpty(RefBuild.Imbue.Name))
                imbue = $"<b>Imbue:</b> {RefBuild.Imbue.Name}\r\n\r\n";

            m_longDescription = $"<b>Damage Breakdown:</b> {dmgTypes}\r\n\r\n"+
                $"{imbue}" +
                $"<b>Gear:</b> " +
                $"{(string.IsNullOrEmpty(RefBuild.Offhand.Name) ? "" : $"{RefBuild.Offhand.Description}, ")}" +
                $"{RefBuild.Helmet.Description}, " +
                $"{RefBuild.Chest.Description}, " +
                $"{RefBuild.Boots.Description}, " +
                $"{RefBuild.Backpack.Description}" +
                $"{passives}" +
                $"{statuses}" +
                $"\r\n\r\n<b>Total Bonus</b>: {bonuses}";
        }
    }
}
