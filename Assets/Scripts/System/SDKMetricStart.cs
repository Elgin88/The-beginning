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
#if UNITY_EDITOR
            return;
#endif

#if UNITY_WEBGL
            YandexGamesSdk.GameReady();
#endif
        }
    }
}