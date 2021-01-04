using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.DB;
using OutwardBuildCalc.DB.Model;
using OutwardBuildCalc.DB.StaticData;
using OutwardBuildCalc.CalcModel;
using SideLoader;
using SideLoader.Helpers;
using UnityEngine;

namespace OutwardBuildCalc.UI
{
    public static class BuildCalcMenu
    {
        public static BuildProfile Profile;

        internal static int WINDOW_ID;
        private static Rect s_windowRect = new Rect(200f, 100f, 1000f, 700f);

        internal static BuildEditorPane s_editorPane;
        internal static BuildResultsPane s_resultsPane;

        internal static void Init()
        {
            Profile = new BuildProfile();

            s_editorPane = new BuildEditorPane();
            s_resultsPane = new BuildResultsPane();
        }

        private static bool s_show = true;
        public static bool Show
        {
            get => s_show;
            set
            {
                s_show = value;

                if (s_show)
                    ForceUnlockCursor.AddUnlockSource();
                else
                    ForceUnlockCursor.RemoveUnlockSource();
            }
        }

        public static void OnGUI()
        {
            if (!Show)
                return;

            var orig = GUI.skin;
            GUI.skin = UIStyles.WindowSkin;
            s_windowRect = GUI.Window(WINDOW_ID, s_windowRect, WindowFunction, "Build Calc Menu");
            GUI.skin = orig;
        }

        internal static void WindowFunction(int id)
        {
            GUI.DragWindow(new Rect(0, 0, s_windowRect.width - 35, 23));
            //if (GUI.Button(new Rect(s_windowRect.width - 30, 2, 30, 18), "X"))
            //{
            //    Show = false;
            //    return;
            //}

            GUILayout.BeginHorizontal();
            s_editorPane.OnGUI();
            s_resultsPane.OnGUI();
            GUILayout.EndHorizontal();
        }
    }
}
