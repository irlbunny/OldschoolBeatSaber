using HMUI;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

namespace OldschoolBeatSaber.Behaviours.UI
{
#pragma warning disable CS0436
    public class SongListTableCell : TableCell
#pragma warning restore CS0436
    {
        [SerializeField] private UnityEngine.UI.Image _bgImage;
        [SerializeField] private UnityEngine.UI.Image _highlightImage;
        [SerializeField] private UnityEngine.UI.Image _coverImage;

        [SerializeField] private TextMeshProUGUI _songNameText;
        [SerializeField] private TextMeshProUGUI _authorText;

        private CancellationTokenSource _settingDataCancellationTokenSource;
        private string _settingDataFromLevelId;

        public virtual async void SetDataFromLevelAsync(IPreviewBeatmapLevel previewBeatmapLevel)
        {
            if (_settingDataFromLevelId != previewBeatmapLevel.levelID)
            {
                try
                {
                    _settingDataFromLevelId = previewBeatmapLevel.levelID;

                    if (_settingDataCancellationTokenSource != null)
                        _settingDataCancellationTokenSource.Cancel();

                    _settingDataCancellationTokenSource = new CancellationTokenSource();

                    _songNameText.text = string.Format("{0}\n<size=80%>{1}</size>", previewBeatmapLevel.songName, previewBeatmapLevel.songSubName).Replace("\n", " ");
                    _authorText.text = previewBeatmapLevel.songAuthorName;

                    _coverImage.sprite = null;
                    _coverImage.color = Color.clear;

                    var cancellationToken = _settingDataCancellationTokenSource.Token;
                    var coverImage = await previewBeatmapLevel.GetCoverImageAsync(cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();

                    _coverImage.sprite = coverImage;
                    _coverImage.color = Color.white;

                    cancellationToken = default;
                }
                catch (OperationCanceledException)
                { }
                finally
                {
                    if (_settingDataFromLevelId == previewBeatmapLevel.levelID)
                        _settingDataFromLevelId = null;
                }
            }
        }

        protected override void SelectionDidChange(TransitionType transitionType)
        {
            if (selected)
            {
                _highlightImage.enabled = false;
                _bgImage.enabled = true;
                _songNameText.color = Color.black;
                _authorText.color = Color.black;
            }
            else
            {
                _bgImage.enabled = false;
                _songNameText.color = Color.white;
                _authorText.color = new(1f, 1f, 1f, .4f);
            }
        }

        protected override void HighlightDidChange(TransitionType transitionType)
        {
            _highlightImage.enabled = !selected ? highlighted : false;
        }
    }
}
