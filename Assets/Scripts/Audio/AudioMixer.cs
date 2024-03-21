using System;
using UnityEngine;
using UnityEngine.Audio;
using Assets.Scripts.Constants;

namespace Assets
{
    internal class AudioMixer : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup _mixer;

        private float _minVolume = -80;
        private float _maxVolume = 0;

        private bool _isMuted = false;

        public bool IsMuted => _isMuted;

        public event Action<bool> Muted;

        public void ChangeVolume(float volume)
        {
            _mixer.audioMixer.SetFloat(PlayerConfigs.MusicVolume, volume);
            PlayerPrefs.SetFloat(PlayerConfigs.MusicVolume, volume);

            _isMuted = false;
            Muted?.Invoke(_isMuted);
        }

        public void ToggleMusic()
        {
            if (_isMuted)
            {
                SetVolumeValue();

                _isMuted = false;
            }
            else
            {
                _mixer.audioMixer.SetFloat(PlayerConfigs.MusicVolume, _minVolume);
                _isMuted = true;
            }

            Muted?.Invoke(_isMuted);
        }

        public void Mute()
        {
            if (_isMuted == false)
                _mixer.audioMixer.SetFloat(PlayerConfigs.MusicVolume, _minVolume);
        }

        public void Unmute()
        {
            float value = PlayerPrefs.GetFloat(PlayerConfigs.MusicVolume);

            if (_isMuted == false)
            {
                SetVolumeValue();
            }
        }

        private void SetVolumeValue()
        {
            float value = PlayerPrefs.GetFloat(PlayerConfigs.MusicVolume);

            if (value > _minVolume)
                _mixer.audioMixer.SetFloat(PlayerConfigs.MusicVolume, value);
            else
                _mixer.audioMixer.SetFloat(PlayerConfigs.MusicVolume, _maxVolume);
        }
    }
}
