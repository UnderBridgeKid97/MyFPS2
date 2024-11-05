using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// �������� �Դ� �浹ü(hitbox)�� �����Ǿ� �������� �����ϴ� Ŭ���� 
    /// 
    /// </summary>

    public class Damageable : MonoBehaviour
    {
        #region Variables

        private Health health;

        // ������ ��� 
        [SerializeField]private float damageMultiplier = 1f;

        // �ڽ��� ���� ������ ��� 
        [SerializeField]private float sensiblilityToSelfDamage = 0.5f;

        #endregion

        private void Awake()
        {
            // ���� 
            health = GetComponent<Health>(); 
            if(health = null) // �ڱ����� ��ã���� & ������
            {
               health = GetComponentInParent<Health>(); // �θ� ��ü���� ã�� 
            }
        }

        public void InflictDamage(float damage, bool isExplosionDamage , GameObject damageSource)
        {
            if (health == null)
                return;

            //  totalDamage : ���� ������ �� (������)
            var totalDamage = damage;

            // ���� ������ üũ -  ���� �������϶��� damageMultiplier�� ������� �ʴ´�
            if(isExplosionDamage == false)
            {
                totalDamage *= damageMultiplier;
            }

            // �ڽ��� ���� ��������
            if(health.gameObject == damageSource)
            {
                totalDamage *=sensiblilityToSelfDamage;
            }

            // ������ ������
            health.TakeDamage(totalDamage, damageSource);
        }


    }
}