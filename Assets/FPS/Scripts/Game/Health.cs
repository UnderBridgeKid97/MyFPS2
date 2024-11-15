using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// ü���� �����ϴ� Ŭ���� 
    /// 
    /// </summary>

    public class Health : MonoBehaviour
    {
        #region Variables

        [SerializeField]private float maxHealth = 100f;  // �ִ�hp & �ｺ���ʱⰪ & �������� �ƽ���
        public float CurrentHealtH { get; private set; } // ����hp
        private bool isDeath = false;                    // ���� üũ
        //------- ä�¼��� �⺻ -----------------------

        public UnityAction<float, GameObject> OnDamaged; // �������� �߻��ϸ� ���⿡ ���
        public UnityAction Ondie; // ���� 
        // ��
        public UnityAction<float> OnHeal; // ���̹߻� @@

        // ü�� ������ ���� 
        [SerializeField]private float criticalHealRatio = 0.3f;

        // ���� ���� 
        public bool Invincible {  get; private set; }

        #endregion

        // �� �������� ���� �� �ִ��� üũ
        public bool CanPickUp() => CurrentHealtH < maxHealth; // ü���� ������ �ƴҶ� 

        // UI�� HP������ ��
        public float GetRatio() => CurrentHealtH / maxHealth; // ����ü�� / �ִ�ü�� ���� 

        // ü���� ���� ���Ϸ� �������� �˶� - ����üũ
        public bool IsCritical() => GetRatio() <= criticalHealRatio ;

        private void Start()
        {
            // �ʱ�ȭ
            CurrentHealtH = maxHealth;
            Invincible = false;
        }

        // �� ó��
        public void Heal(float amount)
        {
            float beforeHealth = CurrentHealtH;
            CurrentHealtH += amount;
            CurrentHealtH = Mathf.Clamp(CurrentHealtH, 0f, maxHealth);

            // real Heal ���ϱ�
            float realHeal = CurrentHealtH - beforeHealth;
            if (realHeal >0)
            {
                // �� ����
                OnHeal?.Invoke(realHeal);
            }
        }
        // ������ ó��
        // damageSource : �������� �ִ� ��ü �� 
        public void TakeDamage(float damage, GameObject damageSource)
        {
            
            // ���� üũ
            if (Invincible)
                return;

       //     Debug.Log($"damage:{damage}");

            float beforeHealth = CurrentHealtH; // ������ �Ա����� hp
            
            CurrentHealtH -= damage;

            CurrentHealtH = Mathf.Clamp(CurrentHealtH, 0, maxHealth); // 0���Ϸ� �ȶ����� 0 ���ϰ��� 0���� ����
          //  Debug.Log($"hp:{CurrentHealtH}");

            // real������ ���ϱ�
            float realDamage = beforeHealth - CurrentHealtH;
            if(realDamage > 0f)
            {
                // ������ ���� 
             OnDamaged?.Invoke(realDamage, damageSource ); // ���� ���� ������, �������� �ִ� ��ü / @@@? ~ null�� �ƴϸ� ����
            }

            // ����ó�� �Լ� 
            HandleDeath();
        }

        // ����ó��
        void HandleDeath()
        {
            // ���� ���� üũ / ���� ���� ����
            if (isDeath)
                return;
                
            if(CurrentHealtH <= 0f)
            {
                isDeath = true;

                // ���� ���� 
                Ondie?.Invoke();
            }
        }
    }
}