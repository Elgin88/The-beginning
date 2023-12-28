using Assets.Scripts.PlayerComponents.Weapons;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerSceneInstaller : MonoInstaller
    {
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private Weapon[] _baseWeapons;

        public override void InstallBindings()
        {
            Container.Bind<Weapon[]>().FromInstance(_baseWeapons).NonLazy();
            Container.Bind<WeaponsInventory>().FromNew().AsSingle().NonLazy();
            Container.Bind<PlayerAttacker>().FromNew().AsSingle().NonLazy();

            BindPlayer();
        }

        private void BindPlayer()
        {
            Container.Bind<PlayerConfig>().FromInstance(_playerConfig).NonLazy();

            Player player = Container.InstantiatePrefabForComponent<Player>(_playerPrefab, transform.position, Quaternion.identity, null);
            Container.BindInterfacesAndSelfTo<Player>().FromInstance(player);
        }
    }
}
