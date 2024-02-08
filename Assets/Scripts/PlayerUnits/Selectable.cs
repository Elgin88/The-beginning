using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.PlayerUnits
{
    internal class Selectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private ParticleSystem _ring;
        private SelectedUnitsHandler _handler;
        private bool _isSelected;

        public event Action<Selectable> Selected;
        public event Action<Selectable> Unselected;

        public bool IsSelected => _isSelected;

        public void Init(ParticleSystem ring, SelectedUnitsHandler handler)
        {
            _ring = ring;
            _ring = Instantiate(_ring, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), Quaternion.identity);
            _ring.Stop();

            _isSelected = false;
            _handler = handler;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _ring.Play();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isSelected == false)
                _ring.Stop();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_isSelected) 
            {
                Debug.Log("unselected");
                _isSelected = false;
                _ring.Stop();
                _handler.RemoveUnit(this);
            }
            else
            {
                Debug.Log("selected");
                _isSelected = true;
                _ring.Play();
                _handler.AddUnit(this);
            }
        }
    }
}
