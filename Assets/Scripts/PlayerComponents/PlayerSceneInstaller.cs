using Assets.Scripts.PlayerComponents.Weapons;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerSceneInstaller : MonoInstaller
    {
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private WeaponsInventory _weaponInventory;

        public override void InstallBindings()
        {
            BindPlayer();
        }

        private void BindPlayer()
        {
            WeaponsInventory inventory = Container.InstantiatePrefabForComponent<WeaponsInventory>(_weaponInventory);
            Container.Bind<WeaponsInventory>().FromInstance(inventory).AsSingle().NonLazy();
            Container.Bind<PlayerAttacker>().FromNew().AsSingle().NonLazy();

            Container.Bind<PlayerConfig>().FromInstance(_playerConfig).NonLazy();

            Player player = Container.InstantiatePrefabForComponent<Player>(_playerPrefab, transform.position, Quaternion.identity, null);
            Container.BindInterfacesAndSelfTo<Player>().FromInstance(player);

            inventory.transform.parent = player.transform;
        }
    }
}
