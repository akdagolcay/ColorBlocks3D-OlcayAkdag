using System;
using Injection;
using UnityEngine;
using Zenject;

namespace UI
{
    public class GameSceneLoading : MonoBehaviour
    {
        [SerializeField] private GameObject screen;
        [Inject] private SignalBus _signalBus;
        private void OnEnable()
        {
            _signalBus.Subscribe<OnLevelLoad>(Close);
            _signalBus.Subscribe<OnLevelUnload>(Open);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<OnLevelLoad>(Close);
            _signalBus.Unsubscribe<OnLevelUnload>(Open);
        }

        private void Open()
        {
            screen.SetActive(true);
        }
        private void Close()
        {
            screen.SetActive(false);
        }
    }
}
