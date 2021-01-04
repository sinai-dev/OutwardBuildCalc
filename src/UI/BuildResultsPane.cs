using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.CalcModel;
using UnityEngine;

namespace OutwardBuildCalc.UI
{
    public class BuildResultsPane : UIScrollArea
    {
        public int ResultLimit = 10;

        internal static Dictionary<string, List<CalcResult>> s_lastResults;
        internal static int s_selectedResults = 0;
        //internal static bool[] s_expandedResults = new bool[0];

        public override void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true));

            GUILayout.Label("<size=16>Result Options:</size>");

            GUILayout.BeginHorizontal();

            GUILayout.Label("Result limit:", GUILayout.Width(90));

            var limitString = this.ResultLimit.ToString();
            limitString = GUILayout.TextField(limitString, GUILayout.Width(50));
            if (int.TryParse(limitString, out int newLim))
                ResultLimit = newLim;

            //BuildCalcMenu.s_buildProfile.LimitOneEachWeapon
            //    = GUILayout.Toggle(BuildCalcMenu.s_buildProfile.LimitOneEachWeapon, "Limit One Result Per Weapon?");

            BuildCalcMenu.Profile.AxesOnlyUseSpecial
                = GUILayout.Toggle(BuildCalcMenu.Profile.AxesOnlyUseSpecial, "Use Special Attacks for Axe DPS?");

            BuildCalcMenu.Profile.ConsiderWeaponHex
                = GUILayout.Toggle(BuildCalcMenu.Profile.ConsiderWeaponHex, "Consider Main-hand Hexes?");

            GUILayout.EndHorizontal();

            GUI.color = Color.green;
            if (GUILayout.Button("Calculate / Find Best Builds"))
            {
                BuildCalcMenu.Profile.Calculate(ResultLimit, out List<CalcResult> highestDamage, out List<CalcResult> highestDPS);
                s_lastResults = new Dictionary<string, List<CalcResult>> { { "Highest Damage", highestDamage }, { "Highest DPS", highestDPS } };
                //s_expandedResults = new bool[ResultLimit];
            }
            GUI.color = Color.white;

            if (s_lastResults != null)
            {
                UIStyles.HorizontalLine(Color.grey);

                GUILayout.Label("<size=16>Results:</size>");

                if (BuildProfile.IsCalculating)
                {
                    if (BuildProfile.IsGenerating)
                        GUILayout.Label($"Generating builds... ({BuildProfile.GeneratedBuildCount})");
                    else if (BuildProfile.IsPickingGear)
                        GUILayout.Label($"Picking best gear... ({BuildProfile.GearPickProgress} / {BuildProfile.GeneratedBuildCount})");
                    else
                        GUILayout.Label($"Checking builds... ({BuildProfile.GeneratedBuildCount})");
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    for (int i = 0; i < s_lastResults.Count; i++)
                    {
                        if (i == s_selectedResults)
                            GUI.color = Color.green;

                        if (GUILayout.Button(s_lastResults.ElementAt(i).Key))
                        {
                            s_selectedResults = i;
                            //s_expandedResults = new bool[ResultLimit];
                        }
                        GUI.color = Color.white;
                    }
                    GUILayout.EndHorizontal();

                    var selected = s_lastResults.ElementAt(s_selectedResults);

                    GUILayout.Label($"<b>{selected.Key}</b>");

                    m_scroll = GUILayout.BeginScrollView(m_scroll);

                    var results = s_lastResults.ElementAt(s_selectedResults).Value;
                    for (int i = 0; i < results.Count; i++)
                    {
                        var result = results[i];

                        result.OnGUI();
                    }

                    GUILayout.EndScrollView();
                }
            }

            GUILayout.EndVertical();
        }
    }
}
