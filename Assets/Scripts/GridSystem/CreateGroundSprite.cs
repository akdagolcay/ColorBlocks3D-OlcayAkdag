using System;
using System.Collections;
using System.Collections.Generic;
using Injection;
using UnityEngine;
using Utilities;
using Zenject;

namespace GridSystem
{
    public class CreateGroundSprite : IInitializable, IDisposable
    {
        private const string GroundSpriteTag = "GroundSprite";
        private GridController _gridController;
        private SignalBus _signalBus;

        public CreateGroundSprite(SignalBus signalBus, GridController gridController)
        {
            _signalBus = signalBus;
            _gridController = gridController;
        }
    
        public void Initialize()
        {
            _signalBus.Subscribe<CreateSprites>(CreateSprites);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<CreateSprites>(CreateSprites);
        }
        
        private void CreateSprites(CreateSprites createSprites)
        {
            for (int i = 0; i < _gridController.Grid.width; i++)
            {
                for (int j = 0; j < _gridController.Grid.height; j++)
                {
                    var visual = CreateGridSprite(_gridController.Grid.GetWorldPosition(i, j), _gridController.CellSize, createSprites.IsZ);
                    _gridController.Grid.GetNodeWithoutCoord(i, j).GridVisual = visual;
                }
            }
        }
        private GridVisual CreateGridSprite(Vector3 pos, float cellSize, bool isZ)
        {
            if (isZ)
            {
                pos = new Vector3(pos.x, 0, pos.y);
            }

            var obj = ObjectPooler.instance.Spawn(GroundSpriteTag, pos, Quaternion.identity).GetComponent<GridVisual>();
            obj.transform.localScale = new Vector3 (cellSize, cellSize, cellSize);
            return obj;
        }

        
    }
}