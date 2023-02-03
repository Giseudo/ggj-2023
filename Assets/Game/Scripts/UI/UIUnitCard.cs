using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Combat;

namespace Game.UI
{
    public class UIUnitCard : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private UnitData _data;

        public UnitData Data => _data;

        public Action<UIUnitCard> clicked = delegate { };

        public void OnPointerClick(PointerEventData evt)
        {
            clicked.Invoke(this);
        }
    }
}