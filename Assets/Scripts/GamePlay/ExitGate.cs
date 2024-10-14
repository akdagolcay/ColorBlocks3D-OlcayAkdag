using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Deform;
using DG.Tweening;
using GridSystem;
using Injection;
using UnityEngine;
using Utilities;
using Zenject;

namespace GamePlay
{
    public class ExitGate : MonoBehaviour
    {
        private const string ParticleName = "ExitParticle";
        public ColorType ColorTypes { get; private set; }
        public Directions Direction { get; private set; }
        public Node Node { get; private set; }
        [SerializeField] private List<MeshRenderer> doorMeshes;
        [SerializeField] private float offsetZ;
        [SerializeField] private float doorOffsetY;
        [SerializeField] private Transform modelHolder;
        [SerializeField] private Transform destroyPosition;
        [SerializeField] private Transform particlePos;
        [SerializeField] private BendDeformer bendDeformer;
        [SerializeField] private float factor = 35;
        private CancellationTokenSource _source;

        private void OnDestroy()
        {
            if (_source != null)
                _source.Cancel();
        }

        private void SetOffset()
        {
            modelHolder.localPosition = new Vector3(0, 0, offsetZ);
        }

        public void Setup(ColorType colors, Material mat, Node node, Directions direction)
        {
            _source = new CancellationTokenSource();
            SetOffset();
            switch (direction)
            {
                case Directions.Up:
                    transform.eulerAngles = new Vector3(0, 180, 0);
                    break;
                case Directions.Right:
                    transform.eulerAngles = new Vector3(0, -90, 0);
                    break;
                case Directions.Left:
                    transform.eulerAngles = new Vector3(0, 90, 0);
                    break;
                default:
                    transform.eulerAngles = Vector3.zero;
                    break;
            }

            Direction = direction;
            Node = node;
            ColorTypes = colors;

            foreach (var mesh in doorMeshes)
            {
                mesh.material = mat;
            }
        }
        public void Animation()
        {
            float myFloat = 0;

            DOTween.To(() => myFloat, x => myFloat = x, factor, 0.2f).OnUpdate(() =>
            {
                bendDeformer.Angle = myFloat;
            }).SetEase(Ease.Linear);
            DOTween.To(() => myFloat, x => myFloat = x, 0, 0.15f).OnUpdate(() =>
                {
                    bendDeformer.Angle = myFloat;
                })
                .SetEase(Ease.OutBack,6).SetDelay(0.2f);
        }
        public async void Eat(Movable movable, SignalBus signalBus)
        {
            await movable.transform.DOMove(transform.position, movable.Speed).SetSpeedBased().SetEase(Ease.InSine)
                .ToUniTask(cancellationToken: _source.Token);
            if (!DOTween.IsTweening(doorMeshes[0]))
            {
                foreach (var door in doorMeshes)
                {
                    door.transform.DOMoveY(doorOffsetY, 0.1f).SetRelative().SetTarget(door);
                }
            }

            var particle = ObjectPooler.instance.Spawn(ParticleName, particlePos.position)
                .GetComponent<ParticleSystem>();
            particle.GetComponent<ParticleSystemRenderer>().material = doorMeshes[0].material;
            particle.Play();
            var lookPos = new Vector3(destroyPosition.position.x, particlePos.position.y, destroyPosition.position.z);
            particle.transform.LookAt(lookPos);
            var targetPos = destroyPosition.position + (destroyPosition.forward.normalized * (movable.Length - 1));
            await movable.transform.DOMove(targetPos, movable.Speed / 3).SetSpeedBased()
                .SetEase(Ease.Linear).ToUniTask(cancellationToken: _source.Token);
            particle.Stop();

            if (!DOTween.IsTweening(doorMeshes[0]))
            {
                foreach (var door in doorMeshes)
                {
                    door.transform.DOMoveY(-doorOffsetY, 0.1f).SetRelative();
                }
            }

            signalBus.Fire<CubeGone>();
            await UniTask.Yield(cancellationToken: _source.Token);
            signalBus.Fire<CubeMove>();
            ObjectPooler.instance.Release(movable.gameObject, movable.TagName);
        }
    }
}