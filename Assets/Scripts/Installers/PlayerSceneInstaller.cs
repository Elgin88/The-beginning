using Assets.Scripts.PlayerComponents;
using Assets.Scripts.PlayerComponents.Weapons;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Installers
{
    internal class PlayerSceneInstaller : MonoInstaller
    {
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private PlayerAnimator _playerAnimator;
        [SerializeField] private Mark _mark;

        public override void InstallBindings()
        {
            BindPlayer();
        }

        private void BindPlayer()
        {
            Container.Bind<Mark>().FromInstance(_mark).AsSingle().NonLazy();


            Container.Bind<PlayerConfig>().FromInstance(_playerConfig).NonLazy();
            Player player = Container.InstantiatePrefabForComponent<Player>(_playerPrefab, transform.position, Quaternion.identity, null);
            Container.BindInterfacesAndSelfTo<Player>().FromInstance(player);

            Container.Bind<PlayerAnimator>().FromComponentOn(player.gameObject).AsSingle().NonLazy();
            Container.Bind<WeaponsInventory>().FromComponentOn(player.gameObject).AsSingle().NonLazy();
            
            Container.Bind<PlayerMovement>().FromNew().AsSingle().NonLazy();
            Container.Bind<PlayerAttacker>().FromNew().AsSingle().NonLazy();
            PlayerInput input = Container.InstantiatePrefabForComponent<PlayerInput>(_playerInput);

            input.transform.parent = player.transform;
        }
    }
}
