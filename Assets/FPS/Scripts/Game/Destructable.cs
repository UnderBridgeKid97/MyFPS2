using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// �׾��� �� Health�� ���� ��� ������Ʈ�� ų�ϴ� Ŭ���� 
    /// 
    /// </summary>

    public class Destructable : MonoBehaviour
    {
        #region Variables

        private Health Health;

        #endregion

        private void Start ()
        {   
            // ����
            Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(Health, this,gameObject);

            // UnityAction event�Լ��� ���
            Health.Ondie += Ondie; // health�� �̺�Ʈ �Լ� Ondie�� ��Ʈ���ͺ� Ondie�� ���
            Health.OnDamaged += OnDamage; //  @@

        }
        // ����
        void OnDamage(float damage,GameObject damageSource)
        {
            // TODO : ������ ȿ�� ����
        }



        void Ondie()
        {
            // ������Ʈ ų 
            Destroy(gameObject);
        }

    }
}