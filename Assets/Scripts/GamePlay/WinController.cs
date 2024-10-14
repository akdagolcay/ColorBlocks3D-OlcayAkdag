using System;
using Injection;
using Managers;
using Zenject;

namespace GamePlay
{
    public class WinController : IInitializable, IDisposable
    {
        private readonly GameManager _gameManager;
        private int _currentCubes;
        
        readonly SignalBus _signalBus;

        public WinController(SignalBus signalBus, GameManager gameManager)
        {
            _gameManager = gameManager;
            _signalBus = signalBus;
        }
        
        public void Initialize()
        {
            _signalBus.Subscribe<OnLevelLoad>(OnLevelLoad);
            _signalBus.Subscribe<CubeGone>(CubeGone);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<OnLevelLoad>(OnLevelLoad);
            _signalBus.Unsubscribe<CubeGone>(CubeGone);
        }
        
        private void CubeGone()
        {
            _currentCubes--;
            if (_currentCubes <= 0)
            {
                _gameManager.Win();
            }
        }

        private  void OnLevelLoad(OnLevelLoad onLevelLoad)
        {
            _currentCubes = onLevelLoad.HowManyCube;
        }
    }
}
    