using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Game.Core;

namespace Game.UI
{
    using Game.Combat;

    public class UITreeHighlight : MonoBehaviour
    {
        [SerializeField]
        private Tree _highlightTree;

        private Tween _tween;

        private Renderer[] _renderers;
        private Material[] _materials;

        private bool _isHighlighting = false;
        public Action<bool> highlightChanged = delegate { };

        public bool IsHighlighting => _isHighlighting;

        public void Start()
        {
            SetHighlightTree(_highlightTree);

            if (GameManager.Scenes.CurrentLevel == 0)
                Highlight(true);
        }

        public void OnDestroy()
        { }

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
            }, 1f, 1f);

            highlightChanged.Invoke(enable);
            _isHighlighting = enable;
        }
    }
}