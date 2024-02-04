using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.PlayerUnits
{
    internal class Selectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private ParticleSystem _ring;
        private SelectedUnitsHandler _selectedUnitsHandler;

        public void InitSelection(ParticleSystem ring, SelectedUnitsHandler selectedUnitsHandler)
        {
            _selectedUnitsHandler = selectedUnitsHandler;
            _ring = ring;
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
            _ring.Play();
        }
    }
}
