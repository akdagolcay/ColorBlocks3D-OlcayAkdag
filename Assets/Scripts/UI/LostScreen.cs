using System;
using Injection;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class LostScreen : MonoBehaviour
    {
        [SerializeField] private GameObject screen;
        [SerializeField] private Button button;
        [Inject] private SignalBus _signalBus;
        
        private void OnEnable()
        {
            _signalBus.Subscribe<OnLost>(OnLost);
            _signalBus.Subscribe<OnGameplay>(OnGameplay);
            button.onClick.AddListener(Restart);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<OnLost>(OnLost);
            _signalBus.Unsubscribe<OnGameplay>(OnGameplay);
            button.onClick.RemoveAllListeners();
        }

        private void OnGameplay()
        {
            screen.SetActive(false);
        }

        private void OnLost()
        {
            screen.SetActive(true);
        }
        
        private void Restart()
        {
            _signalBus.Fire<Restart>();
        }
    }
}
