using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OutwardBuildCalc.UI.UIBuildElement
{
    public abstract class UIBuildElementBase
    {
        internal IEnumerable options;
        internal string title;
        internal Action onChanged;

        internal bool m_selecting;
        internal string m_searchInput;
        internal Vector2 m_scroll;

        public UIBuildElementBase(IEnumerable options, string title, Action onChanged)
        {
            this.options = options;
            this.title = title;
            this.onChanged = onChanged;

            CacheOptions();
        }

        public abstract void OnGUI();

        public abstract void CacheOptions();
    }
}
