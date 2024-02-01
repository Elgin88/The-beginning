using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.PlayerUnits
{
    internal class Selectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private ParticleSystem _selectedRing;

        private ParticleSystem _ring;
        private SelectedUnitsHandler _selectedUnitsHandler;

        private void Start()
        {
            _selectedUnitsHandler = new SelectedUnitsHandler();

            _ring = Instantiate(_selectedRing, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), Quaternion.identity);

            _ring.Stop();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _ring.Play();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _ring.Stop();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _selectedUnitsHandler.AddUnit(this);
        }
    }
}
