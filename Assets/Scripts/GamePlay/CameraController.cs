using System;
using GridSystem;
using Injection;
using UnityEngine;
using Zenject;

namespace GamePlay
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;

        [Inject] private SignalBus _signalBus;
        [Inject] private GridController _gridController;

        private void OnEnable()
        {
            _signalBus.Subscribe<OnLevelLoad>(OnLevelLoad);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<OnLevelLoad>(OnLevelLoad);
        }

        private void OnLevelLoad()
        {
            var x = _gridController.Grid.width % 2 == 0 ? -0.5f : 0;
            var y = CalculateX(_gridController.Grid.width);
            var first = _gridController.Grid.GetNodeWithoutCoord(0, 0);
            var last = _gridController.Grid.GetNodeWithoutCoord(0,  _gridController.Grid.height - 1);

            var z = last.YPos - first.YPos - 1;

            transform.position = new Vector3(x, y, z);
        }

        private int CalculateX(int inputValue)
        {
            return inputValue * 2 + 4;
        }
        
    }
}