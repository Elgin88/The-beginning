using Agava.YandexGames;
using UnityEngine;

namespace Assets.Scripts.Yander
{
    public class SDKMetricStart : MonoBehaviour
    {
        private void Start()
        {
            OnCallGameReadyButtonClick();
        }

        public void OnCallGameReadyButtonClick()
        {
            YandexGamesSdk.GameReady();
        }
    }
}