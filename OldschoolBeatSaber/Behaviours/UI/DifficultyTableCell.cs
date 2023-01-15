using HMUI;
using TMPro;
using UnityEngine;

namespace OldschoolBeatSaber.Behaviours.UI
{
#pragma warning disable CS0436
    public class DifficultyTableCell : TableCell
#pragma warning restore CS0436
    {
        [SerializeField] private UnityEngine.UI.Image _bgImage;
        [SerializeField] private UnityEngine.UI.Image _highlightImage;
        [SerializeField] private TextMeshProUGUI _difficultyText;

        public string DifficultyText
        {
            get => _difficultyText.text;
            set => _difficultyText.text = value;
        }

        protected override void SelectionDidChange(TransitionType transitionType)
        {
            if (selected)
            {
                _highlightImage.enabled = false;
                _bgImage.enabled = true;
                _difficultyText.color = Color.black;
            }
            else
            {
                _bgImage.enabled = false;
                _difficultyText.color = Color.white;
            }
        }

        protected override void HighlightDidChange(TransitionType transitionType)
        {
            _highlightImage.enabled = !selected ? highlighted : false;
        }
    }
}
