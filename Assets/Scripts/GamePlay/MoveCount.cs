using System;
using Injection;
using Managers;
using UnityEngine;
using Zenject;

namespace GamePlay
{
    public class MoveCount : IInitializable, IDisposable
    {
        private int _moveCount;
        private readonly SignalBus _signalBus;
        private readonly GameManager _gameManager;
        private bool _noMoveCount;
        public MoveCount(SignalBus signalBus, GameManager gameManager)
        {
            _gameManager = gameManager;
            _signalBus = signalBus;
        }
        
        public void Initialize()
        {
            _signalBus.Subscribe<OnLevelLoad>(OnLevelLoad);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<OnLevelLoad>(OnLevelLoad);
        }

        private void CubeMove()
        {
            _moveCount--;
            if (_moveCount <= 0 && _gameManager.GameState == GameStates.GamePlay)
            {
                _gameManager.Lost();
            }
        }

        private void OnLevelLoad(OnLevelLoad onLevelLoad)
        {
            _signalBus.TryUnsubscribe<CubeMove>(CubeMove);
            if (onLevelLoad.MoveCount != 0)
            {
                _signalBus.Subscribe<CubeMove>(CubeMove);
            }
            _moveCount = onLevelLoad.MoveCount;
        }
    }
}
