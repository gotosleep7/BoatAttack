using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace BoatAttack.UI
{
    public class RaceUIMutil : MonoBehaviour, IRaceUI
    {
        private Boat _boat;

        private Boat _boat2;



        public TextMeshProUGUI speedFormatText;

        public RectTransform map;
        public GameObject gameplayUi;
        public GameObject player1NameUI;
        public GameObject player2NameUI;
        public GameObject raceStat;
        public GameObject matchEnd;

        [Header("player1")]
        public TextMeshProUGUI timeTotal;
        public TextMeshProUGUI speedText;
        public TextMeshProUGUI timeLap;
        public TextMeshProUGUI lapCounter;
        public TextMeshProUGUI positionNumber;

        [Header("player2")]
        public TextMeshProUGUI timeTotal2;
        public TextMeshProUGUI speedText2;
        public TextMeshProUGUI timeLap2;
        public TextMeshProUGUI lapCounter2;
        public TextMeshProUGUI positionNumber2;
        [Header("Assets")]
        public AssetReference playerMarker;
        public AssetReference playerMapMarker;
        public AssetReference raceStatsPlayer;
        private int _playerIndex;
        private int _playerIndex2;
        private int _totalLaps;
        private int _totalPlayers;
        private float _timeOffset;
        private float _smoothedSpeed;
        private float _smoothedSpeed2;
        private float _smoothSpeedVel;
        private float _smoothSpeedVel2;
        private AppSettings.SpeedFormat _speedFormat;
        private RaceStatsPlayer[] _raceStats;


        private void OnEnable()
        {
            RaceManager.raceStarted += SetGameplayUi;
        }

        public void Setup(int player1, int player2)
        {
            _playerIndex = player1;
            _playerIndex2 = player2;
            _boat = RaceManager.RaceData.boats[_playerIndex].Boat;
            _boat2 = RaceManager.RaceData.boats[_playerIndex2].Boat;
            _totalLaps = RaceManager.GetLapCount();
            _totalPlayers = RaceManager.RaceData.boats.Count;
            _timeOffset = Time.time;

            switch (AppSettings.Instance.speedFormat)
            {
                case AppSettings.SpeedFormat._Kph:
                    _speedFormat = AppSettings.SpeedFormat._Kph;
                    speedFormatText.text = "kph";
                    break;
                case AppSettings.SpeedFormat._Mph:
                    _speedFormat = AppSettings.SpeedFormat._Mph;
                    speedFormatText.text = "mph";
                    break;
            }

            StartCoroutine(SetupPlayerMarkers(player1, player1NameUI.transform));
            StartCoroutine(SetupPlayerMarkers(player2, player2NameUI.transform));
            StartCoroutine(SetupPlayerMapMarkers());
            StartCoroutine(CreateGameStats());
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        public void SetGameplayUi(bool enable)
        {
            if (enable)
            {
                foreach (var stat in _raceStats)
                {
                    stat.UpdateStats();
                }
            }
            gameplayUi.SetActive(enable);
        }

        public void SetGameStats(bool enable)
        {
            raceStat.SetActive(enable);
        }

        public void MatchEnd()
        {
            matchEnd.SetActive(true);
            SetGameStats(true);
            SetGameplayUi(false);
        }

        private IEnumerator CreateGameStats()
        {
            _raceStats = new RaceStatsPlayer[RaceManager.RaceData.boatCount];
            for (var i = 0; i < RaceManager.RaceData.boatCount; i++)
            {
                var raceStatLoading = raceStatsPlayer.InstantiateAsync(raceStat.transform);
                yield return raceStatLoading;
                raceStatLoading.Result.name += RaceManager.RaceData.boats[i].boatName;
                raceStatLoading.Result.TryGetComponent(out _raceStats[i]);
                _raceStats[i].Setup(RaceManager.RaceData.boats[i].Boat);
            }
        }

        private IEnumerator SetupPlayerMarkers(int player, Transform parent)
        {
            for (int i = 0; i < RaceManager.RaceData.boats.Count; i++)
            {
                if (i == player) continue;

                // var markerLoading = playerMarker.InstantiateAsync(gameplayUi.transform);
                var markerLoading = playerMarker.InstantiateAsync(parent);
                yield return markerLoading; // wait for marker to load

                markerLoading.Result.name += RaceManager.RaceData.boats[i].boatName;
                if (markerLoading.Result.TryGetComponent<PlayerMarker>(out var pm))
                    pm.Setup(RaceManager.RaceData.boats[i]);
            }
        }

        private IEnumerator SetupPlayerMapMarkers()
        {
            foreach (var boatData in RaceManager.RaceData.boats)
            {
                var mapMarkerLoading = playerMapMarker.InstantiateAsync(map);
                yield return mapMarkerLoading; // wait for marker to load

                if (mapMarkerLoading.Result.TryGetComponent<PlayerMapMarker>(out var pm))
                    pm.Setup(boatData);
            }
        }
        public void UpdateLapCounter(int lap, int playerIndex)
        {
            if (playerIndex == _playerIndex)
            {
                lapCounter.text = $"{lap}/{_totalLaps}";
            }
            else if (playerIndex == _playerIndex2)
            {
                lapCounter2.text = $"{lap}/{_totalLaps}";
            }

        }

        public void UpdatePlaceCounter(int place, int playerIndex)
        {
            if (playerIndex == _playerIndex)
            {
                positionNumber.text = $"{place}/{_totalPlayers}";
            }
            else if (playerIndex == _playerIndex2)
            {
                positionNumber2.text = $"{place}/{_totalPlayers}";
            }
        }

        public void UpdateSpeed(float velocity, int playerIndex)
        {
            var speed = 0f;

            switch (_speedFormat)
            {
                case AppSettings.SpeedFormat._Kph:
                    speed = velocity * 3.6f;
                    break;
                case AppSettings.SpeedFormat._Mph:
                    speed = velocity * 2.23694f;
                    break;
            }

            if (playerIndex == _playerIndex)
            {
                _smoothedSpeed = Mathf.SmoothDamp(_smoothedSpeed, speed, ref _smoothSpeedVel, 1f);
                speedText.text = _smoothedSpeed.ToString("000");

            }
            else if (playerIndex == _playerIndex2)
            {
                _smoothedSpeed2 = Mathf.SmoothDamp(_smoothedSpeed2, speed, ref _smoothSpeedVel2, 1f);
                speedText2.text = _smoothedSpeed2.ToString("000");
            }
        }

        public void FinishMatch()
        {
            RaceManager.UnloadRace();
        }

        public void LateUpdate()
        {
            var rawTime = RaceManager.RaceTime;
            timeTotal.text = $"time {RaceUIUtil.FormatRaceTime(rawTime)}";
            timeTotal2.text = $"time {RaceUIUtil.FormatRaceTime(rawTime)}";

            var l = (_boat.SplitTimes.Count > 0) ? rawTime - _boat.SplitTimes[_boat.LapCount - 1] : 0f;
            timeLap.text = $"lap {RaceUIUtil.FormatRaceTime(l)}";
            var l2 = (_boat2.SplitTimes.Count > 0) ? rawTime - _boat2.SplitTimes[_boat2.LapCount - 1] : 0f;
            timeLap2.text = $"lap {RaceUIUtil.FormatRaceTime(l2)}";
        }

    }
}
