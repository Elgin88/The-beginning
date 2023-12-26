using UnityEngine;
using Zenject;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerSpawnPoint : MonoBehaviour
    {
        private Player _player;

        private void Start ()
        {
            _player.transform.position = transform.position;
        }

        [Inject]
        private void Construct(Player player)
        {
            _player = player;
        }
    }
}
