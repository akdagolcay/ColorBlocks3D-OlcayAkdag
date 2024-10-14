using System;
using Injection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class WinScreen : MonoBehaviour
    {
        [SerializeField] private GameObject screen;
        [SerializeField] private Button button;
        [Inject] private SignalBus _signalBus;
        
        private void OnEnable()
        {
            _signalBus.Subscribe<OnWin>(OnWin);
            _signalBus.Subscribe<OnGameplay>(OnGameplay);
            button.onClick.AddListener(NextLevel);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<OnWin>(OnWin);
            _signalBus.Unsubscribe<OnGameplay>(OnGameplay);
            button.onClick.RemoveAllListeners();
        }

        private void OnGameplay()
        {
            screen.SetActive(false);
        }

        private void OnWin()
        {
            screen.SetActive(true);
        }
        
        private void NextLevel()
        {
            _signalBus.Fire<NextLevel>();
        }
    }
}
