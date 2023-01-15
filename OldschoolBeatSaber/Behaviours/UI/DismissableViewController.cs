using System;
using VRUI;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class DismissableViewController : VRUIViewController
    {
        public event Action<DismissableViewController> DidFinishEvent;

        public void DismissButtonWasPressed()
        {
            if (DidFinishEvent != null)
                DidFinishEvent(this);
        }
    }
}
