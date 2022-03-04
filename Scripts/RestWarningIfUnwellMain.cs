// Project:         RestWarningIfUnwell mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    3/3/2022, 7:00 AM
// Last Edit:		3/3/2022, 7:00 AM
// Version:			1.00
// Special Thanks:  
// Modifier:			

using System;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;

namespace RestWarningIfUnwell
{
    public class RestWarningIfUnwellMain : MonoBehaviour
    {
        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<RestWarningIfUnwellMain>();
        }

        void Awake()
        {
            InitMod();

            mod.IsReady = true;
        }

        private static void InitMod()
        {
            Debug.Log("Begin mod init: RestWarningIfUnwell");

            DaggerfallUI.UIManager.OnWindowChange += UIManager_OnRestWindowOpened;

            Debug.Log("Finished mod init: RestWarningIfUnwell");
        }

        public static bool showedWarning = false;

        // Triggered when the DaggerfallRestWindow UI window is opened
        public static void UIManager_OnRestWindowOpened (object sender, EventArgs e)
        {
            if (!GameManager.Instance.StateManager.GameInProgress)
                return;

            if (showedWarning && DaggerfallUI.UIManager.WindowCount == 0) // Kind of hacky way to reset the warning when player exits the rest window entirely by any means.
                showedWarning = false;

            if (GameManager.Instance.StateManager.CurrentState == StateManager.StateTypes.Game)
            {
                if (!showedWarning && DaggerfallUI.Instance.UserInterfaceManager.TopWindow is DaggerfallRestWindow)
                {
                    // Only warn if one or more poisons are present (both active and waiting states.)
                    bool warn = false;
                    LiveEffectBundle[] poisonBundles = GameManager.Instance.PlayerEffectManager.PoisonBundles;
                    foreach (LiveEffectBundle poisonBundle in poisonBundles)
                    {
                        foreach (IEntityEffect effect in poisonBundle.liveEffects)
                        {
                            if (effect is PoisonEffect)
                            {
                                PoisonEffect poison = (PoisonEffect)effect;
                                if (poison.CurrentState != PoisonEffect.PoisonStates.Complete)
                                {
                                    warn = true;
                                }
                            }
                        }
                    }

                    // Only warn if one or more diseases are present (both active and incubation states.)
                    LiveEffectBundle[] bundles = GameManager.Instance.PlayerEffectManager.DiseaseBundles;
                    foreach (LiveEffectBundle bundle in bundles)
                    {
                        foreach (IEntityEffect effect in bundle.liveEffects)
                        {
                            if (effect is DiseaseEffect)
                            {
                                warn = true;
                            }
                        }
                    }

                    if (warn)
                    {
                        TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.CreateTokens(TextFile.Formatting.JustifyCenter,
                            "You are currently unwell, either due to disease or poisoning.",
                            "Resting right now may lead you to an early grave.");
                        DaggerfallMessageBox textBox = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
                        textBox.SetTextTokens(tokens);
                        textBox.ClickAnywhereToClose = true;
                        textBox.Show();

                        showedWarning = true;
                    }
                }
            }
        }
    }
}
