using System;
using UnityEngine;
using VRUI;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class SoloModeSelectionViewController : VRUIViewController
    {
        [SerializeField] private HowToPlayViewController _howToPlayViewController;

        public Action<SoloModeSelectionViewController, GameplayMode> DidSelectModeEvent;
        public Action<SoloModeSelectionViewController> DidFinishEvent;

        protected override void DidActivate()
        {
            base.DidActivate();

            screen.screenSystem.leftScreen.SetRootViewController(_howToPlayViewController);
            screen.screenSystem.rightScreen.SetRootViewController(null);
        }

        public void FreePlayModeButtonWasPressed()
        {
            if (DidSelectModeEvent != null)
            {
                DidSelectModeEvent(this, GameplayMode.SoloStandard);
            }
        }

        public void OneSaberModeButtonWasPressed()
        {
            if (DidSelectModeEvent != null)
            {
                DidSelectModeEvent(this, GameplayMode.SoloOneSaber);
            }
        }

        public void NoArrowsModeButtonWasPressed()
        {
            if (DidSelectModeEvent != null)
            {
                DidSelectModeEvent(this, GameplayMode.SoloNoArrows);
            }
        }

        public void DismissButtonWasPressed()
        {
            if (DidFinishEvent != null)
            {
                DidFinishEvent(this);
            }
        }
    }
}
