using Agava.YandexGames;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Yandex
{
    public class SDKInitializer : MonoBehaviour
    {
        private void Awake()
        {
            YandexGamesSdk.CallbackLogging = true;
            Debug.Log("AYAY tararai tararai");
        }

        private IEnumerator Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            yield return YandexGamesSdk.Initialize(OnInitialized);
            Debug.Log("START INIT");
#endif
            yield return null;
        }

        private void OnInitialized()
        {
            Debug.Log("FINISHED INIT");
            SceneManager.LoadScene("StudyLevel");
        }
    }
}