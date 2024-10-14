using GamePlay;
using GridSystem;
using Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

namespace Injection
{
    public class GameSceneInstaller : MonoInstaller<GameSceneInstaller>
    {
        [SerializeField] private PlayerInput playerInput;
        public override void InstallBindings()
        {
            //Signals
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<CreateSprites>();
            Container.DeclareSignal<OnLevelUnload>();
            Container.DeclareSignal<OnLevelLoad>();
            Container.DeclareSignal<OnGameplay>();
            Container.DeclareSignal<OnWin>();
            Container.DeclareSignal<OnLost>();
            Container.DeclareSignal<CubeGone>();
            Container.DeclareSignal<Restart>();
            Container.DeclareSignal<NextLevel>();
            Container.DeclareSignal<CubeMoveForUI>().OptionalSubscriber();
            Container.DeclareSignal<CubeMove>().OptionalSubscriber();
            
            //Bindings
            Container.Bind<GridController>().AsSingle();
            Container.Bind<PlayerInput>().FromInstance(playerInput).AsSingle();
            Container.Bind<GameManager>().AsSingle();
            Container.BindInterfacesTo<CreateGroundSprite>().AsSingle();
            Container.BindInterfacesTo<WinController>().AsSingle();
            Container.BindInterfacesTo<MoveCount>().AsSingle();
        }
    }
}