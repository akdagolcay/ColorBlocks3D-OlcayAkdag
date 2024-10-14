using System;
using Injection;
using SaveSystem;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class GamePlayScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private GameObject moveCountParent;
        [Inject] private SignalBus _signalBus;
        private int _currentMoveCount;
        private void OnEnable()
        {
            _signalBus.Subscribe<OnLevelLoad>(OnLevelLoad);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<OnLevelLoad>(OnLevelLoad);
        }

        private void CubeMoveForUI()
        {
            _currentMoveCount--;
            moveCountText.text = _currentMoveCount.ToString();
        }

        private void OnLevelLoad(OnLevelLoad onLevelLoad)
        {
            levelText.text = $"LEVEL {SaveData.Instance.Level}";
            _currentMoveCount = onLevelLoad.MoveCount;
            
            _signalBus.TryUnsubscribe<CubeMoveForUI>(CubeMoveForUI);
            moveCountParent.SetActive(false);
            if (onLevelLoad.MoveCount != 0)
            {
                _signalBus.Subscribe<CubeMoveForUI>(CubeMoveForUI);
                moveCountParent.SetActive(true);
            }
            
            moveCountText.text = _currentMoveCount.ToString();
        }
    }
}
