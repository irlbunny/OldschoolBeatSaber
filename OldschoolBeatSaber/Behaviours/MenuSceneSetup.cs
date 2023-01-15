using OldschoolBeatSaber.Behaviours.UI;
using UnityEngine;
using VRUI;

namespace OldschoolBeatSaber.Behaviours
{
    public class MenuSceneSetup : MonoBehaviour
    {
        [SerializeField] private VRUIScreenSystem _screenSystem;
        [SerializeField] private MenuMasterViewController _menuMasterViewController;

        private void Start()
        {
            _screenSystem.mainScreen.SetRootViewController(_menuMasterViewController);
        }
    }
}
