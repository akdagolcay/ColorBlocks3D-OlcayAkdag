using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Deform;
using DG.Tweening;
using GridSystem;
using Injection;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace GamePlay
{
    public class Movable : MonoBehaviour
    {
        public int HowManyEffected => 3;
        public float Speed => speed;
        public List<Directions> DirectionsList { get; private set; }
        public List<Node> NodeList { get; private set; } = new();
        public string TagName { get; private set; }
        public int Length { get; private set; }
        public bool IsAvailable { get; private set; } = true;
        private ColorType _type;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Transform modelHolder;

        [SerializeField] private float bendFactor = 0.25f;
        [SerializeField] private float speed = 0.5f;

        [FormerlySerializedAs("_bendDeformers")] [SerializedDictionary("Node", "BendDeformer")] [SerializeField]
        private SerializedDictionary<int, SerializedDictionary<Directions, BendDeformer>> bendDeformers;

        private bool _isReversed;

        public void Setup(Material material, Node node, ColorType colorType, List<Directions> directionsList,
            int length, GridController gridController, string tagName)
        {
            TagName = tagName;
            Length = length;
            IsAvailable = true;
            DirectionsList = new List<Directions>(directionsList);
            _type = colorType;

            SetNewNode(node, gridController);
            meshRenderer.material = material;
            if (directionsList.Contains(Directions.Up) && directionsList.Contains(Directions.Down))
            {
                _isReversed = true;
                modelHolder.eulerAngles = new Vector3(0, 90, 0);
            }
            else
            {
                _isReversed = false;
                modelHolder.eulerAngles = Vector3.zero;
            }
        }

        public void ExitGateAnim(Directions direction, Node node, GridController gridController)
        {
            foreach (var exit in node.ExitGate)
            {
                if (exit.Direction == direction)
                {
                    exit.Animation();
                }
            }

            BendAnim(direction, 1, node, 0, gridController);
        }

        public void BendAnim(Directions direction, float multiplier, Node node, int howManyEffected, GridController gridController)
        {
            float factor = bendFactor * multiplier;

            var firstNode = NodeList[0];
            Node nextNode = null;

            for (int i = 1; i < howManyEffected; i++)
            {
                switch (direction)
                {
                    case Directions.Down:
                        nextNode = gridController.Grid.GetNodeWithoutCoord(firstNode.X, firstNode.Y + i + (Length - 1));
                        break;
                    case Directions.Right:
                        nextNode = gridController.Grid.GetNodeWithoutCoord(firstNode.X + i + (Length - 1), firstNode.Y);
                        break;
                    case Directions.Up:
                        nextNode = gridController.Grid.GetNodeWithoutCoord(firstNode.X, firstNode.Y - i);
                        break;
                    case Directions.Left:
                        nextNode = gridController.Grid.GetNodeWithoutCoord(firstNode.X - i, firstNode.Y);
                        break;
                }

                if (nextNode != null && nextNode.Movable != null)
                    nextNode.Movable.BendAnim(direction, multiplier * (0.5f / i), nextNode, 0, gridController);
            }

            int index = NodeList.FindIndex(n => n.X == node.X && n.Y == node.Y);
            if (_isReversed)
            {
                if (direction == Directions.Up)
                {
                    direction = Directions.Left;
                }
                else if (direction == Directions.Down)
                {
                    direction = Directions.Right;
                }
                else if (direction == Directions.Left)
                {
                    direction = Directions.Down;
                }
                else if (direction == Directions.Right)
                {
                    direction = Directions.Up;
                }
            }

            var bendDeformer = bendDeformers[index][direction];

            Animation(factor, bendDeformer);
        }

        private void Animation(float factor, BendDeformer bendDeformer)
        {
            float myFloat = 0;

            DOTween.To(() => myFloat, x => myFloat = x, factor, 0.2f).OnUpdate(() => { bendDeformer.Angle = myFloat; })
                .SetEase(Ease.Linear);
            DOTween.To(() => myFloat, x => myFloat = x, 0, 0.15f).OnUpdate(() => { bendDeformer.Angle = myFloat; })
                .SetEase(Ease.OutBack, 6).SetDelay(0.2f);
        }

        public void Move(Node latestNode, Directions direction, GridController gridController, SignalBus signalBus)
        {
            Node dummyNude = latestNode;
            IsAvailable = false;

            if (latestNode.ExitGate.Count != 0)
            {
                var exitGate = latestNode.ExitGate.FirstOrDefault(gate =>
                    gate.ColorTypes == _type && direction == gate.Direction);

                if (exitGate != null)
                {
                    exitGate.Eat(this, signalBus);
                    ReleaseNode();
                    return;
                }
            }

            signalBus.Fire<CubeMove>();
            if (direction == Directions.Down)
            {
                latestNode = gridController.Grid.GetNodeWithoutCoord(latestNode.X, latestNode.Y - (Length - 1));
            }
            else if (direction == Directions.Right)
            {
                latestNode = gridController.Grid.GetNodeWithoutCoord(latestNode.X - (Length - 1), latestNode.Y);
            }

            var pos = new Vector3(latestNode.XPos, 0, latestNode.YPos);
            transform.DOMove(pos, speed).SetSpeedBased().SetEase(Ease.InSine).OnComplete((() =>
            {
                IsAvailable = true;
                CheckForBend(direction, dummyNude, gridController);
            }));
            SetNewNode(latestNode, gridController);
        }

        private void CheckForBend(Directions direction, Node node, GridController gridController)
        {
            
            if ((node.Y == 0 || node.Y == gridController.Grid.height - 1) &&( direction == Directions.Up|| direction == Directions.Down))
            {
                ExitGateAnim(direction, node, gridController);
            }
            else if ((node.X == 0 || node.X == gridController.Grid.width - 1) &&( direction == Directions.Left || direction == Directions.Right))
            {
                ExitGateAnim(direction, node, gridController);
            }
            else
            {
                BendAnim(direction, 1, node, HowManyEffected, gridController);
            }
        }

        private void ReleaseNode()
        {
            foreach (var node in NodeList)
            {
                node.IsAvailble = true;
                node.Movable = null;
            }

            NodeList.Clear();
        }

        private void SetNewNode(Node node, GridController gridController)
        {
            ReleaseNode();

            if (DirectionsList.Contains(Directions.Down) || DirectionsList.Contains(Directions.Up))
            {
                for (int i = 0; i < Length; i++)
                {
                    var newNode = gridController.Grid.GetNodeWithoutCoord(node.X, node.Y + i);
                    NodeList.Add(newNode);
                    newNode.Movable = this;
                    newNode.IsAvailble = false;
                }
            }
            else if (DirectionsList.Contains(Directions.Right) || DirectionsList.Contains(Directions.Left))
            {
                for (int i = 0; i < Length; i++)
                {
                    var newNode = gridController.Grid.GetNodeWithoutCoord(node.X + i, node.Y);
                    NodeList.Add(newNode);
                    newNode.Movable = this;
                    newNode.IsAvailble = false;
                }
            }
        }
    }
}