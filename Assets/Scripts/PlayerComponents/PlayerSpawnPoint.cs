using UnityEngine;
using Zenject;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerSpawnPoint : MonoBehaviour
    {
        private Player _player;

        [Inject]
        private void Construct(Player player)
        {
            _player = player;
            _player.transform.position = transform.position;
        }
    }
}
