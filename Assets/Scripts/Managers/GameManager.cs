using System;
using Injection;
using UnityEngine;
using Zenject;

namespace Managers
{
    public class GameManager
    {
        [Inject] private SignalBus _signalBus;
        public GameStates GameState { get; private set; } = GameStates.Start;

        public void GameStart()
        {
            GameState = GameStates.Start;
        }

        public void GamePlay()
        {
            _signalBus.Fire(new OnGameplay());
            GameState = GameStates.GamePlay;
        }

        public void Win()
        {
            _signalBus.Fire<OnWin>();
            GameState = GameStates.Win;
        }

        public void Lost()
        {
            _signalBus.Fire<OnLost>();
            GameState = GameStates.Lost;
        }
    }
}