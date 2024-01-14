using Assets.Scripts.PlayerComponents.Weapons;

namespace Assets.Scripts.PlayerComponents
{
    internal class PlayerAttacker
    {
        private PlayerAnimator _animator;

        private WeaponsInventory _inventory;
        private Weapon _currentWeapon;

        public PlayerAttacker(WeaponsInventory inventory, PlayerAnimator animator)
        {
            _inventory = inventory;
            _animator = animator;

            _inventory.Init();
            ChangeWeapon();
        }

        public void Attack()
        {
            if (_currentWeapon.CanAttack)
            {
                _animator.SetAnimatorAttackTrigger(_currentWeapon);

                _currentWeapon.Attack();
            }
        }

        public void ChangeWeapon()
        {
            _currentWeapon = _inventory.ChangeWeapon();

            _animator.SetAnimatorChangeWeaponTrigger(_currentWeapon);
        }
    }
}
