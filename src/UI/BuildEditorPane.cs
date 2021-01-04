using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OutwardBuildCalc.DB;
using OutwardBuildCalc.DB.Model;
using OutwardBuildCalc.CalcModel;
using OutwardBuildCalc.UI.UIBuildElement;
using UnityEngine;
using SideLoader;

namespace OutwardBuildCalc.UI
{
    public class BuildEditorPane : UIScrollArea
    {
        private UIListHexes m_hexChoices;

        private UIListWeaponTypes m_blacklistedWeapons;

        private UIEquipmentSelection<WeaponModel> m_weaponChoice;
        private UIEquipmentSelection<EquipmentModel> m_offhandChoice;
        private UIEquipmentSelection<EquipmentModel> m_helmetChoice;
        private UIEquipmentSelection<EquipmentModel> m_chestChoice;
        private UIEquipmentSelection<EquipmentModel> m_bootsChoice;
        private UIEquipmentSelection<EquipmentModel> m_backpackChoice;

        private UIListImbues m_imbueOptions;
        private UIListPassives m_passives;
        private UIListStatuses m_statuses;

        public BuildEditorPane()
        {
            SetupUIElements();
        }

        internal void SetupUIElements()
        {
            var profile = BuildCalcMenu.Profile;

            m_hexChoices = new UIListHexes(null, "Universal Hexes", () =>
            {
                UpdateHexes(m_hexChoices.m_chosenOptions);
            });

            m_weaponChoice = new UIEquipmentSelection<WeaponModel>(Database.WeaponModels, "Weapon", () =>
            {
                profile.ChosenWeapon = m_weaponChoice.Equipment;
            });

            m_blacklistedWeapons = new UIListWeaponTypes(null, "Allowed Weapon Types", () =>
            {
                profile.WeaponBlacklist = m_blacklistedWeapons.m_notChosenOptions;
            });

            m_offhandChoice = new UIEquipmentSelection<EquipmentModel>(Database.OffhandModels, "Off-hand", () =>
            {
                profile.ChosenOffhand = m_offhandChoice.Equipment;
            });
            m_helmetChoice = new UIEquipmentSelection<EquipmentModel>(Database.HelmetModels, "Helmet", () =>
            {
                profile.ChosenHelmet = m_helmetChoice.Equipment;
            });
            m_chestChoice = new UIEquipmentSelection<EquipmentModel>(Database.ChestModels, "Chest", () =>
            {
                profile.ChosenChest = m_chestChoice.Equipment;
            });
            m_bootsChoice = new UIEquipmentSelection<EquipmentModel>(Database.BootsModels, "Boots", () =>
            {
                profile.ChosenBoots = m_bootsChoice.Equipment;
            });
            m_backpackChoice = new UIEquipmentSelection<EquipmentModel>(Database.BackpackModels, "Backpack", () =>
            {
                profile.ChosenBackpack = m_backpackChoice.Equipment;
            });

            m_imbueOptions = new UIListImbues(Database.ImbueModels, "Imbue Options", () =>
            {
                profile.AllowedImbues = m_imbueOptions.m_chosenOptions;
            });

            m_passives = new UIListPassives(Database.PassiveModels, "Passives", () =>
            {
                profile.ChosenPassives = m_passives.m_chosenOptions;
            });

            m_statuses = new UIListStatuses(Database.StatusModels, "Status Effects", () =>
            {
                profile.ChosenStatuses = m_statuses.m_chosenOptions;
            });
        }

        private void UpdateHexes(List<UIHexModel> hexes)
        {
            var dict = new Dictionary<string, float[]>();
            foreach (var hex in hexes)
            {
                dict.Add(hex.IdentifierName, hex.DamageModifiers);
            }
            BuildCalcMenu.Profile.NaturalHexes = dict;
        }

        public override void OnGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(400f), GUILayout.ExpandHeight(true));

            m_scroll = GUILayout.BeginScrollView(m_scroll);

            GUILayout.Label("<size=16>Enemy Resists:</size>");

            GUILayout.BeginHorizontal();
            for (int i = 0; i < 6; i++)
            {
                if (i == 3)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                GUILayout.Label(((DamageType.Types)i).ToString(), GUILayout.Width(60));
                var resInput = (BuildCalcMenu.Profile.Enemy.DamageResistance[i] * 100f).ToString("F0");
                resInput = GUILayout.TextField(resInput, GUILayout.Width(50));
                if (float.TryParse(resInput, out float f))
                    BuildCalcMenu.Profile.Enemy.DamageResistance[i] = f * 0.01f;
            }
            GUILayout.EndHorizontal();

            m_hexChoices.OnGUI();

            UIStyles.HorizontalLine(Color.grey);

            GUILayout.Label("<size=16>Player Setup:</size>");

            m_weaponChoice.OnGUI();
            if (m_weaponChoice.Equipment == null)
                m_blacklistedWeapons.OnGUI();

            m_offhandChoice.OnGUI();
            m_helmetChoice.OnGUI();
            m_chestChoice.OnGUI();
            m_bootsChoice.OnGUI();
            m_backpackChoice.OnGUI();

            UIStyles.HorizontalLine(Color.grey);
            m_imbueOptions.OnGUI();
            UIStyles.HorizontalLine(Color.grey);
            m_passives.OnGUI();
            UIStyles.HorizontalLine(Color.grey);
            m_statuses.OnGUI();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
