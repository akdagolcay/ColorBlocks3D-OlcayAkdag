using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace UI
{
    public class LoadingSceneLoader : MonoBehaviour
    {
        [Inject] private SaveController _saveController;
        private CancellationTokenSource _source;
        private void Awake()
        {
            _source = new CancellationTokenSource();
            LoadWaiting();
        }

        private void OnDestroy()
        {
            _source.Cancel();
        }

        private async void LoadWaiting()
        {
            await UniTask.WaitUntil(() => _saveController.LoadingDone, cancellationToken: _source.Token);
            SceneManager.LoadScene("GameScene");
        }
    }
}
