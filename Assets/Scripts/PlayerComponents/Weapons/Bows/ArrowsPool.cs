using UnityEngine;

namespace Assets.Scripts.PlayerComponents.Weapons.Bows
{
    internal class ArrowsPool
    {
        private Arrow _arrowPrefab;
        private Arrow[] _pool;
        private LayerMask _mask;

        private float _arrowDamage;
        private int _capacity = 10;

        public ArrowsPool(Arrow arrowPrefab, float arrowDamage, LayerMask targetMask) 
        {
            _arrowPrefab = arrowPrefab;
            _arrowDamage = arrowDamage;
            _mask = targetMask;

            _pool = CreateArrows();
        }

        public Arrow GetArrow()
        {
            foreach (var arrow in _pool)
            {
                if (arrow.gameObject.activeSelf == false)
                {
                    arrow.gameObject.SetActive(true);

                    return arrow;
                }
            }

            throw new System.Exception("Not enough Arrows in The pool!!!");
        }

        private Arrow[] CreateArrows()
        {
            Arrow[] pool = new Arrow[_capacity];

            for (int i = 0; i < _capacity; i++) 
            { 
                Arrow arrow = GameObject.Instantiate(_arrowPrefab);
                arrow.Init(_arrowDamage, _mask);
                arrow.gameObject.SetActive(false);
                pool[i] = arrow;
            }

            return pool;
        }
    }
}
