using SaveSystem;
using UnityEngine;
using Zenject;

namespace Injection
{
    public class LoadingSceneInjection : MonoInstaller
    {
        [SerializeField] private SaveController saveController;

        public override void InstallBindings()
        {
            Container.Bind<SaveController>().FromInstance(saveController);
        }
    }
}