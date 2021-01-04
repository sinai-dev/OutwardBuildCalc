using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OutwardBuildCalc.UI
{
    public abstract class UIScrollArea
    {
        internal Vector2 m_scroll;

        public abstract void OnGUI();
    }
}
