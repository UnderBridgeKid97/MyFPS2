using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class EnemyController : MonoBehaviour
    {
        #region Variables

        private Health health;

        // death
        public GameObject deathVfxPrefab;
        public Transform deathVfxSpawnPosition;


        #endregion

        private void Start()
        {
            // 참조 
            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.Ondie += OnDie;

        }

        private void OnDamaged(float damage,GameObject damageSource)
        {

        }

        private void OnDie()
        {
            // 폭발효과
            GameObject EffectGo = Instantiate(deathVfxPrefab,deathVfxSpawnPosition.position, Quaternion.identity);
            Destroy(EffectGo,5f);

            // enemy kill
            Destroy(gameObject);
        }


    }
}