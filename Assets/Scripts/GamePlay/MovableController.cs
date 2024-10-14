using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GridSystem;
using Injection;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace GamePlay
{
    public class MovableController : MonoBehaviour
    {
        [SerializeField] private float offset;
     
        [Inject] private PlayerInput _input;
        [Inject] private GridController _gridController;
        [Inject] private SignalBus _signalBus;
        private InputAction _swipeAction;
        private InputAction _clickAction;
        private Movable _selected;
        private Node _selectedNode;

        private void OnEnable()
        {
            _swipeAction = _input.actions.FindAction("Swipe");
            _clickAction = _input.actions.FindAction("Click");
            _swipeAction.performed += SwipePerformed;
            _clickAction.started += Click;
            _clickAction.canceled += Canceled;
            _signalBus.Subscribe<OnGameplay>(OnGameplay);
            _signalBus.Subscribe<OnWin>(OnWin);
            _signalBus.Subscribe<OnLost>(OnLost);
            _swipeAction.Disable();
            _clickAction.Disable();
        }

        private void OnDisable()
        {
            _swipeAction.performed -= SwipePerformed;
            _clickAction.started -= Click;
            _clickAction.canceled -= Canceled;
            _signalBus.Unsubscribe<OnGameplay>(OnGameplay);
            _signalBus.Unsubscribe<OnWin>(OnWin);
            _signalBus.Unsubscribe<OnLost>(OnLost);
        }

        private void OnLost()
        {
            _swipeAction.Disable();
            _clickAction.Disable();
            StopAllCoroutines();
        }

        private void OnWin()
        {
            _swipeAction.Disable();
            _clickAction.Disable();
            StopAllCoroutines();
        }

        private void OnGameplay()
        {
            _clickAction.Enable();
        }

        private void Canceled(InputAction.CallbackContext callback)
        {
            _selected = null;
            _swipeAction.Disable();
        }

        private void Click(InputAction.CallbackContext callback)
        {
#if !UNITY_EDITOR
            Vector3 worldPosition =
                Camera.main.ScreenToWorldPoint(new Vector3(Touchscreen.current.position.x.value,  Touchscreen.current.position.y.value, Camera.main.transform.position.y));
#endif       
#if UNITY_EDITOR
            Vector3 worldPosition =
                Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.x.value,  Mouse.current.position.y.value, Camera.main.transform.position.y));
#endif
            
          
            _selectedNode = _gridController.Grid.GetNode(worldPosition.x, worldPosition.z);

           
            if (_selectedNode != null && _selectedNode.Movable != null && _selectedNode.Movable.IsAvailable)
            {
                _swipeAction.Enable();
                _selected = _selectedNode.Movable;
             
            }
        }

        private void SwipePerformed(InputAction.CallbackContext callback)
        {
            if (_selected == null) return;
            var delta = callback.ReadValue<Vector2>();

            if (_selected.DirectionsList.Contains(Directions.Up) && _selected.DirectionsList.Contains(Directions.Down))
            {
               
                if (Mathf.Abs(delta.y) >= offset)
                {
                    if (delta.y > 0)
                    {
                        if (_selected.NodeList[0].Y == 0)
                        {
                            _selected.ExitGateAnim(Directions.Up, _selected.NodeList[0], _gridController);
                            _selected = null;
                            return;
                        }
                        
                        Node latestNode = _selected.NodeList[0];
                        Node checkedNode;
                        do
                        {
                            checkedNode = _gridController.Grid.GetNodeWithoutCoord(latestNode.X, latestNode.Y - 1);
                            if (checkedNode.IsAvailble || _selected.NodeList.Contains(latestNode))
                            {
                                latestNode = checkedNode;
                            }
                        } while ((checkedNode.IsAvailble || _selected.NodeList.Contains(latestNode)) && checkedNode.Y != 0);
                        
                        if (!_selected.NodeList.Contains(latestNode) && latestNode.IsAvailble)
                        {
                            StartCoroutine(MoveMovable(latestNode, Directions.Up));
                        }
                        else
                        {
                            BendAnim(Directions.Up);
                            _selected = null;
                        }
                    }
                    else
                    {

                        if (_selected.NodeList[^1].Y == _gridController.Grid.height - 1)
                        {
                            _selected.ExitGateAnim(Directions.Down, _selected.NodeList[^1], _gridController);
                            _selected = null;
                            return;
                        }
                        Node latestNode = _selected.NodeList[0];
                        Node checkedNode;
                        do
                        {
                            checkedNode = _gridController.Grid.GetNodeWithoutCoord(latestNode.X, latestNode.Y + 1);
                           
                            if (checkedNode.IsAvailble || _selected.NodeList.Contains(latestNode))
                            {
                                latestNode = checkedNode;
                            }
                        } while ((checkedNode.IsAvailble || _selected.NodeList.Contains(latestNode)) && checkedNode.Y != _gridController.Grid.height - 1);

                        if (!_selected.NodeList.Contains(latestNode) && latestNode.IsAvailble)
                        {
                           
                            StartCoroutine(MoveMovable(latestNode , Directions.Down));
                        } else
                        {
                            BendAnim(Directions.Down);
                            _selected = null;
                        }
                    }
                }
            }
            else if (_selected.DirectionsList.Contains(Directions.Left) && _selected.DirectionsList.Contains(Directions.Right))
            {
                if (Mathf.Abs(delta.x) >= offset)
                {
                    if (delta.x < 0)
                    {
                        if (_selected.NodeList[0].X == 0)
                        {
                            _selected.ExitGateAnim(Directions.Left, _selected.NodeList[0], _gridController);
                            _selected = null;
                            return;
                        }
                        Node latestNode = _selected.NodeList[0];
                        Node checkedNode;
                        do
                        {
                            checkedNode = _gridController.Grid.GetNodeWithoutCoord(latestNode.X - 1, latestNode.Y);
                            if (checkedNode.IsAvailble || _selected.NodeList.Contains(latestNode))
                            {
                                latestNode = checkedNode;
                            }
                        } while ((checkedNode.IsAvailble || _selected.NodeList.Contains(latestNode)) && checkedNode.X != 0);

                        if (!_selected.NodeList.Contains(latestNode) && latestNode.IsAvailble)
                        {
                            StartCoroutine(MoveMovable(latestNode, Directions.Left));
                        } else
                        {
                            BendAnim(Directions.Left);
                            _selected = null;
                        }
                    }
                    else
                    {
                        if (_selected.NodeList[^1].X == _gridController.Grid.width - 1)
                        {
                            _selected.ExitGateAnim(Directions.Right, _selected.NodeList[^1], _gridController);
                            _selected = null;
                            return;
                        }
                        Node latestNode = _selected.NodeList[^1];
                        Node checkedNode;
                        do
                        {
                            checkedNode = _gridController.Grid.GetNodeWithoutCoord(latestNode.X + 1, latestNode.Y);
                            if (checkedNode.IsAvailble || _selected.NodeList.Contains(latestNode))
                            {
                                latestNode = checkedNode;
                            }
                        } while ((checkedNode.IsAvailble || _selected.NodeList.Contains(latestNode))&& checkedNode.X != _gridController.Grid.width - 1);

                        if (!_selected.NodeList.Contains(latestNode) && latestNode.IsAvailble)
                        {
                            StartCoroutine(MoveMovable(latestNode , Directions.Right));
                        } else
                        {
                            BendAnim(Directions.Right);
                            _selected = null;
                        }
                    }
                }
            }
            
            IEnumerator MoveMovable(Node latestNode, Directions direction)
            {
               
                _selected.Move(latestNode, direction, _gridController, _signalBus);
                _selected = null;
                _swipeAction.Disable();
                _clickAction.Disable();
                _signalBus.Fire<CubeMoveForUI>();
                yield return new WaitForSeconds(0.2f);
                _clickAction.Enable();
            }

            void BendAnim(Directions direction)
            {
                _selected.BendAnim(direction, 1, _selectedNode, _selected.HowManyEffected, _gridController);
            }
        }
    }
}