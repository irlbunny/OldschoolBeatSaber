using UnityEngine;
using UnityEngine.UI;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class GameplayModeIndicator : MonoBehaviour
    {
        [SerializeField] private Image[] _icons;
        [Space]
        [SerializeField] private GameplayMode[] _gameplayModes;
        [SerializeField] private Sprite[] _gameplayIcons;

        public void SetupForGameplayMode(GameplayMode gameplayMode)
        {
            for (var i = 0; i < _icons.Length; i++)
                _icons[i].gameObject.SetActive(false);

            for (var i = 0; i < _gameplayModes.Length; i++)
            {
                if (_gameplayModes[i] == gameplayMode)
                {
                    for (var j = 0; j < _icons.Length; j++)
                    {
                        _icons[j].sprite = _gameplayIcons[(i * 2) + j];
                        _icons[j].gameObject.SetActive(true);
                    }
                    break;
                }
            }
        }
    }
}
