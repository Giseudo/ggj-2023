using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Game.UI
{
    using Game.Combat;

    public class UITreeHighlight : MonoBehaviour
    {
        [SerializeField]
        private bool _playAtStart;

        [SerializeField]
        private UIRootPoint _rootPoint;

        [SerializeField]
        private Tree _highlightTree;

        private Tween _tween;

        private Renderer[] _renderers;
        private Material[] _materials;

        public Action<bool> highlightChanged = delegate { };

        public void Start()
        {
            if (_playAtStart)
                Highlight(true);

            SetHighlightTree(_highlightTree);

            _rootPoint.clicked += OnPointClick;
        }

        public void OnDestroy()
        {
            _rootPoint.clicked -= OnPointClick;
        }

        public void SetHighlightTree(Tree tree)
        {
            if (tree == null) return;

            _highlightTree = tree;
            _renderers = tree.GetComponentsInChildren<Renderer>();
            _materials = _renderers.Select(renderer => renderer.material).ToArray();
        }

        public void Highlight(bool enable = true)
        {
            _tween?.Kill();
            _tween = DOTween.To(() => enable ? 0 : .5f, x => {
                float value = (enable ? .5f : 0f) * x;

                foreach (Material material in _materials)
                    material.SetFloat("_HighlightIntensity", value);
                
                highlightChanged.Invoke(enable);
            }, 1f, 1f);

            _rootPoint.Rect.anchoredPosition = UICanvas.GetScreenPosition(_highlightTree.transform.position);
            _rootPoint.Pulse(enable);
        }

        private void OnPointClick() => Highlight(false);
    }
}