using System;
using TMPro;
using UnityEngine;
using VRUI;

namespace OldschoolBeatSaber.Behaviours.UI
{
    public class SongDetailViewController : VRUIViewController
    {
        [SerializeField] private TextMeshProUGUI _songNameText;
        [SerializeField] private TextMeshProUGUI _durationText;
        [SerializeField] private TextMeshProUGUI _bpmText;
        [SerializeField] private TextMeshProUGUI _notesCountText;
        [SerializeField] private TextMeshProUGUI _obstaclesCountText;

        public event Action<SongDetailViewController> DidPressPlayButtonEvent;

        public void PlayButtonPressed()
        {
            if (DidPressPlayButtonEvent != null)
                DidPressPlayButtonEvent(this);
        }

        public async void SetLevelData(IPreviewBeatmapLevel previewBeatmapLevel, IDifficultyBeatmap difficultyLevel)
        {
            var beatmapDataBasicInfo = await difficultyLevel.GetBeatmapDataBasicInfoAsync();
            _songNameText.text = string.Format("{0}\n<size=80%>{1}</size>", previewBeatmapLevel.songName, previewBeatmapLevel.songSubName);
            _durationText.text = previewBeatmapLevel.songDuration.MinSecDurationText();
            _bpmText.text = previewBeatmapLevel.beatsPerMinute.ToString();
            _notesCountText.text = beatmapDataBasicInfo.cuttableNotesCount.ToString();
            _obstaclesCountText.text = beatmapDataBasicInfo.obstaclesCount.ToString();
        }
    }
}
