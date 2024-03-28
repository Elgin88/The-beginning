using UnityEngine;
using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerComponents.Weapons;
using Assets.Scripts.PlayerInput;
using Zenject;

namespace Assets.Scripts.Installers
{
    internal class PlayerSceneInstaller : MonoInstaller
    {
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private DesktopInput _desktopInput;
        [SerializeField] private PlayerData _playerConfig;
        [SerializeField] private Mark _mark;
        [SerializeField] private GameObject _container;

        public override void InstallBindings()
        {
            BindPlayer();
        }

        private void BindPlayer()
        {
            Container.Bind<Mark>().FromInstance(_mark).AsSingle().NonLazy();

            Container.Bind<PlayerData>().FromInstance(_playerConfig).NonLazy();
            Player player = Container.InstantiatePrefabForComponent<Player>(_playerPrefab, transform.position, Quaternion.identity, null);
            Container.BindInterfacesAndSelfTo<Player>().FromInstance(player);

            Container.Bind<PlayerAnimator>().FromComponentOn(player.gameObject).AsSingle().NonLazy();
            Container.Bind<WeaponsInventory>().FromComponentOn(player.gameObject).AsSingle().NonLazy();
            Container.Bind<PlayerMovement>().FromComponentOn(player.gameObject).AsSingle().NonLazy();
            
            Container.Bind<PlayerAttacker>().FromNew().AsSingle().NonLazy();

            DesktopInput input = Container.InstantiatePrefabForComponent<DesktopInput>(_desktopInput);

            input.transform.parent = player.transform;
        }
    }
}
