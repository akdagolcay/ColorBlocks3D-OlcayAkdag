using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using GamePlay;
using GridSystem;
using Injection;
using SaveSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Random = UnityEngine.Random;
using Newtonsoft.Json;
using SaveSystem.Serialization;
using UnityEditor;
using Utilities;

namespace Managers.LevelSystem
{
    public class LevelManager : MonoBehaviour
    {
        private const string LevelPathFormat = "Assets/Levels/Level{0}.json";
        private const string LevelsLabel = "Levels";
        private const string UpLabel = "Up";
        private const string ParallelLabel = "Parallel";
        private const string MovableLabel = "Movable";
        private const string ExitLabel = "Exit";

        public int CurrentLevelIndex { get; private set; }

        [SerializedDictionary("ColorType", "MaterialReferenceForMovables")] [SerializeField]
        private SerializedDictionary<ColorType,
            SerializedDictionary<int, SerializedDictionary<string, AssetReferenceT<Material>>>> materialReferences;

        [SerializedDictionary("ColorType", "MaterialReferenceForExitGates")] [SerializeField]
        private SerializedDictionary<ColorType, AssetReferenceT<Material>> materialReferencesExitGate;

        [Tooltip(
            "Randomizes levels after all levels are played.\nIf this is unchecked, levels will be played again in same order.")]
        [SerializeField]
        private bool randomizeAfterRotation = true;


        [Inject] private GridController _gridController;
        [Inject] private SignalBus _signalBus;
        [Inject] private GameManager _gameManager;

        private int _levelCount;
        private Dictionary<AssetReferenceT<Material>, AsyncOperationHandle<Material>> _loadedMaterials = new();
        private List<AssetReferenceT<Material>> _usedMaterials = new();

        private async Task<int> CountAddressableAssetsWithLabel()
        {
            var locations = await Addressables.LoadResourceLocationsAsync(LevelsLabel).Task;

            return locations.Count;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<NextLevel>(NextLevel);
            _signalBus.Subscribe<Restart>(Restart);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<NextLevel>(NextLevel);
            _signalBus.Unsubscribe<Restart>(Restart);
        }

       
        private void Start()
        {
            Setup();
        }

        private async void Setup()
        {
            _levelCount = await CountAddressableAssetsWithLabel();
            LoadCurrentLevel();
        }
        private void NextLevel()
        {
            SaveData.Instance.Level++;
            SaveController.instance.Save();
            UnloadLevel();
            LoadCurrentLevel();
        }

        private void Restart()
        {
            UnloadLevel();
            LoadLevel();
        }
        private void UnloadLevel()
        {
            _signalBus.Fire<OnLevelUnload>();
        }
        private void LoadCurrentLevel()
        {
            _gameManager.GameStart();
            int levelIndex = SaveData.Instance.Level;

            if (levelIndex <= _levelCount)
            {
                CurrentLevelIndex = levelIndex;
            }
            else if (randomizeAfterRotation)
            {
                var randLevel = Random.Range(1, _levelCount + 1);
                while (randLevel == CurrentLevelIndex)
                {
                    randLevel = Random.Range(1, _levelCount + 1);
                }

                CurrentLevelIndex = randLevel;
            }
            else
            {
                levelIndex %= _levelCount;
                CurrentLevelIndex = levelIndex.Equals(0) ? _levelCount : levelIndex;
            }

            LoadLevel();
        }

        private async void LoadLevel()
        {
            string levelPath = string.Format(LevelPathFormat, CurrentLevelIndex);

            AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(levelPath);

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                TextAsset levelData = handle.Result;
                LevelConfig levelConfig = JsonConvert.DeserializeObject<LevelConfig>(levelData.text);
                SetupLevel(levelConfig);
            }
            else
            {
                Application.Quit();
            }

            Addressables.Release(handle);
        }

        private async void SetupLevel(LevelConfig levelConfig)
        {
            _usedMaterials.Clear();
            _gridController.CreateGrid(levelConfig.ColCount, levelConfig.RowCount, 1, true);

            await SetMovables(levelConfig);
            await SetGates(levelConfig);
            ClearLoadedMaterials();
            _signalBus.Fire(new OnLevelLoad { HowManyCube = levelConfig.MovableInfo.Count , MoveCount = levelConfig.MoveLimit});
            _gameManager.GamePlay();
        }

        private async Task SetMovables(LevelConfig levelConfig)
        {
            List<Directions> directionsList = new();

            void Create(MovableInfo movable, Material material)
            {
                directionsList.Clear();
                var tagMovable = MovableLabel + movable.Length;
                var node = _gridController.Grid.GetNodeWithoutCoord(movable.Col, movable.Row);
                var pos = new Vector3(node.XPos, 0, node.YPos);
                var movableIns = ObjectPooler.instance.Spawn(tagMovable, pos).GetComponent<Movable>();
                foreach (var directionMovable in movable.Direction)
                {
                    Directions dir = (Directions)directionMovable;
                    directionsList.Add(dir);
                }

                movableIns.Setup(material, node, (ColorType)movable.Colors, directionsList, movable.Length,
                    _gridController, tagMovable);
            }

            foreach (var movable in levelConfig.MovableInfo)
            {
                ColorType type = (ColorType)movable.Colors;
                int length = movable.Length;
                string direction = movable.Direction.Contains(0) ? UpLabel : ParallelLabel;
                var matRef = materialReferences[type][length][direction];
                if (_loadedMaterials.ContainsKey(matRef))
                {
                    Create(movable, _loadedMaterials[matRef].Result);

                    continue;
                }

                _usedMaterials.Add(matRef);
                AsyncOperationHandle<Material> handle = Addressables.LoadAssetAsync<Material>(matRef);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Create(movable, handle.Result);
                    _loadedMaterials.Add(matRef, handle);
                }
                else
                {
                    Application.Quit();
                }
            }
        }

        private void ClearLoadedMaterials()
        {
            var dummy = new List<AssetReferenceT<Material>>(_loadedMaterials.Keys);
            foreach (var key in dummy)
            {
                if (_usedMaterials.Contains(key)) continue;
                Addressables.Release(_loadedMaterials[key]);
                _loadedMaterials.Remove(key);
            }

            Resources.UnloadUnusedAssets();
        }

        private async Task SetGates(LevelConfig levelConfig)
        {
            void Create(ExitInfo exit, Material material)
            {
                var node = _gridController.Grid.GetNodeWithoutCoord(exit.Col, exit.Row);
                var pos = new Vector3(node.XPos, 0, node.YPos);
                var exitIns = ObjectPooler.instance.Spawn(ExitLabel, pos).GetComponent<ExitGate>();
                node.ExitGate.Add(exitIns);

                exitIns.Setup((ColorType)exit.Colors, material, node, (Directions)exit.Direction);
            }

            foreach (var exit in levelConfig.ExitInfo)
            {
                ColorType type = (ColorType)exit.Colors;

                var matRef = materialReferencesExitGate[type];
                if (_loadedMaterials.ContainsKey(matRef))
                {
                    Create(exit, _loadedMaterials[matRef].Result);

                    continue;
                }

                _usedMaterials.Add(matRef);
                AsyncOperationHandle<Material> handle = Addressables.LoadAssetAsync<Material>(matRef);
                await handle.Task;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Create(exit, handle.Result);
                    _loadedMaterials.Add(matRef, handle);
                }
                else
                {
                    Application.Quit();
                }
            }
        }
    }
}