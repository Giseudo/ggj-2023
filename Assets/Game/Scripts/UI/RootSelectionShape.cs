using UnityEngine;
using Game.Core;

namespace Game.UI
{
    public class RootSelectionShape : MonoBehaviour
    {
        public bool IsHidden { get; private set; }

        public void OnEnable()
        {
            Hide();
        }

        public void OnDisable()
        { }

        public void Hide()
        {
            IsHidden = true;

            transform.localScale = Vector3.zero;
        }

        public void Show()
        {
            IsHidden = false;

            transform.localScale = Vector3.one;
        }
    }
}