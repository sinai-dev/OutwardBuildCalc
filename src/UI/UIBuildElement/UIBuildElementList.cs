using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.DB.Model;
using SideLoader;
using UnityEngine;

namespace OutwardBuildCalc.UI.UIBuildElement
{
    public abstract class UIBuildElementList<T> : UIBuildElementBase
    {
        private bool m_editing;

        internal List<T> m_chosenOptions;
        internal List<T> m_notChosenOptions;

        internal Dictionary<string, T> m_allOptions = new Dictionary<string, T>();

        public UIBuildElementList(IEnumerable options, string title, Action onChanged) : base(options, title, onChanged) { }

        public abstract string GetDisplayName(T value);

        public override void CacheOptions()
        {
            m_chosenOptions = new List<T>();
            m_notChosenOptions = new List<T>();
            m_allOptions = new Dictionary<string, T>();

            var dict = (IDictionary)options;
            foreach (var option in dict.Values)
            {
                m_allOptions.Add(GetDisplayName((T)option), (T)option);
                m_notChosenOptions.Add((T)option);
            }
        }

        public void AddChoice(T choice)
        {
            m_notChosenOptions.Remove(choice);
            m_chosenOptions.Add(choice);
            onChanged.Invoke();
        }

        public void RemoveChoice(T choice)
        {
            m_chosenOptions.Remove(choice);
            m_notChosenOptions.Add(choice);
            onChanged.Invoke();
        }

        public override void OnGUI()
        {
            GUILayout.Label($"<b><size=15>{title}</size></b>");
            if (m_chosenOptions.Count > 0)
            {
                GUILayout.Label("<b>Chosen:</b>");
                GUI.color = Color.green;
                for (int i = m_chosenOptions.Count - 1; i >= 0; i--)
                {
                    var opt = m_chosenOptions[i];
                    if (GUILayout.Button(GetDisplayName(opt)))
                        RemoveChoice(opt);
                }
                GUI.color = Color.white;
            }
            else
                GUILayout.Label("<b>Chosen: <color=red>NONE</color></b>");

            if (m_editing || this is UIListWeaponTypes)
            {
                if (m_notChosenOptions.Count > 0)
                {
                    GUILayout.BeginHorizontal();
                    if (!(this is UIListWeaponTypes) && GUILayout.Button("<", GUILayout.Width(35)))
                        m_editing = false;
                    else
                    {
                        GUILayout.Label("<b>Not Chosen:</b>");

                        GUILayout.EndHorizontal();
                        GUI.color = Color.red;
                        for (int i = m_notChosenOptions.Count - 1; i >= 0; i--)
                        {
                            var opt = m_notChosenOptions[i];
                            if (GUILayout.Button(GetDisplayName(opt)))
                                AddChoice(opt);
                        }
                        GUI.color = Color.white;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Choose...", GUILayout.Width(80)))
                    m_editing = true;
            }
        }
    }

    public class UIListHexes : UIBuildElementList<UIHexModel>
    {
        public UIListHexes(IEnumerable options, string title, Action onChanged) : base(options, title, onChanged) { }

        public override string GetDisplayName(UIHexModel value)
        {
            if (value.IdentifierName == "Burn")
                return "Scorched";
            return value.IdentifierName;
        }

        public override void CacheOptions()
        {
            options = UIHexModel.Hexes;
            base.CacheOptions();
        }
    }

    public class UIListWeaponTypes : UIBuildElementList<Weapon.WeaponType>
    {
        private bool m_picking;

        public UIListWeaponTypes(IEnumerable options, string title, Action onChange) : base(options, title, onChange) { }

        public override void CacheOptions()
        {
            m_notChosenOptions = new List<Weapon.WeaponType>();
            m_chosenOptions = new List<Weapon.WeaponType>()
            {
                Weapon.WeaponType.Axe_1H, Weapon.WeaponType.Axe_2H,
                Weapon.WeaponType.Bow, Weapon.WeaponType.Halberd_2H,
                Weapon.WeaponType.Mace_1H, Weapon.WeaponType.Mace_2H,
                Weapon.WeaponType.Sword_1H, Weapon.WeaponType.Sword_2H,
                Weapon.WeaponType.Spear_2H, Weapon.WeaponType.FistW_2H
            };

            BuildCalcMenu.Profile.WeaponBlacklist = m_notChosenOptions;
        }

        public override string GetDisplayName(Weapon.WeaponType value) => value.ToString();

        public override void OnGUI()
        {
            if (!m_picking)
            {
                if (GUILayout.Button("Choose Allowed Weapon-Types", GUILayout.Width(200)))
                    m_picking = true;
            }
            else
            {
                if (GUILayout.Button("< Back", GUILayout.Width(100)))
                    m_picking = false;
                else
                {
                    base.OnGUI();
                    UIStyles.HorizontalLine(Color.grey);
                }
            }
        }
    }

    public class UIListStatuses : UIBuildElementList<StatusModel>
    {
        public UIListStatuses(IEnumerable options, string title, Action onChanged) : base(options, title, onChanged) { }

        public override string GetDisplayName(StatusModel value) => value.IdentifierName;

        public List<string> ToStringList() => this.m_chosenOptions.Select(it => it.IdentifierName).ToList();
    }

    public class UIListImbues : UIBuildElementList<ImbueModel>
    {
        public UIListImbues(IEnumerable options, string title, Action onChanged) : base(options, title, onChanged) { }

        public override string GetDisplayName(ImbueModel value) => value.Name;

        internal List<int> ToIntList() => m_chosenOptions.Select(it => it.PresetID).ToList();
    }

    public class UIListPassives : UIBuildElementList<PassiveModel>
    {
        public UIListPassives(IEnumerable options, string title, Action onChanged) : base(options, title, onChanged) { }

        public override string GetDisplayName(PassiveModel value) => value.Name;

        public List<int> ToIntList() => this.m_chosenOptions.Select(it => it.ItemID).ToList();
    }
}
