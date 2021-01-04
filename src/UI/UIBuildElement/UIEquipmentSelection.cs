using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.DB.Model;
using UnityEngine;

namespace OutwardBuildCalc.UI.UIBuildElement
{
    public class UIEquipmentSelection<T> : UIBuildElementBase where T : struct, IEquipmentModel
    {
        public string SelectionName => Equipment == null ? "None (automatic)" : Equipment.Value.Description;
        public T? Equipment;

        private Dictionary<string, T> m_equipmentOptions;

        public UIEquipmentSelection(IEnumerable options, string title, Action onChanged) : base(options, title, onChanged) { }

        public override void CacheOptions()
        {
            m_equipmentOptions = new Dictionary<string, T>();
            foreach (T option in options)
            {
                if (m_equipmentOptions.ContainsKey(option.Description))
                    continue;

                m_equipmentOptions.Add(option.Description, option);
            }
        }

        public override void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{this.title}</b>", GUILayout.Width(80));

            if (m_selecting)
            {
                GUILayout.EndHorizontal();
                if (GUILayout.Button("<", GUILayout.Width(30)))
                    m_selecting = false;
                else
                {
                    m_searchInput = GUILayout.TextField(m_searchInput);

                    m_scroll = GUILayout.BeginScrollView(m_scroll, GUI.skin.box, GUILayout.Height(150));
                    foreach (var option in this.m_equipmentOptions)
                    {
                        if (!string.IsNullOrEmpty(m_searchInput) && !option.Key.Contains(m_searchInput, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        if (GUILayout.Button(option.Key))
                        {
                            Equipment = option.Value;
                            m_selecting = false;
                            onChanged.Invoke();
                            break;
                        }
                    }
                    GUILayout.EndScrollView();
                }
            }
            else
            {
                if (Equipment != null)
                {
                    if (GUILayout.Button("<color=red>X</color>", GUILayout.Width(40)))
                    {
                        Equipment = null;
                        onChanged.Invoke();
                    }
                }
                else
                {
                    if (GUILayout.Button("Choose", GUILayout.Width(60)))
                        m_selecting = true;
                }
                GUILayout.Label(this.SelectionName);

                GUILayout.EndHorizontal();
            }
        }
    }
}
