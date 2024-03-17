using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.PlayerUnits
{
    internal class Selectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private ParticleSystem _ring;

        private bool _isSelected;
        private Coroutine _selected;

        public bool IsSelected => _isSelected;

        public event Action<Selectable> Selected;
        public event Action<Selectable> Deselected;

        private void Awake()
        {
            _ring = Instantiate(_ring, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), Quaternion.identity);
            _ring.Stop();

            _isSelected = false;
        }

        private void OnDisable()
        {
            _isSelected = false;
            Deselected?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _ring.Play();
            _ring.transform.position = transform.position;
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
                _isSelected = false;
                _ring.Stop();
                Deselected?.Invoke(this);
            }
            else
            {
                _isSelected = true;

                if (_selected != null)
                    StopCoroutine(_selected);

                _selected = StartCoroutine(SelectedRing());

                _ring.Play();
                Selected?.Invoke(this);
            }
        }

        private IEnumerator SelectedRing()
        {
            while (_isSelected)
            {
                _ring.transform.position = transform.position;

                yield return null;
            }
        }
    }
}
