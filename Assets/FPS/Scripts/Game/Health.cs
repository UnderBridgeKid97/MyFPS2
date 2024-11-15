using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// 체력을 관리하는 클래스 
    /// 
    /// </summary>

    public class Health : MonoBehaviour
    {
        #region Variables

        [SerializeField]private float maxHealth = 100f;  // 최대hp & 헬스의초기값 & 힐받을떄 맥스값
        public float CurrentHealtH { get; private set; } // 현재hp
        private bool isDeath = false;                    // 죽음 체크
        //------- 채력세팅 기본 -----------------------

        public UnityAction<float, GameObject> OnDamaged; // 데미지가 발생하면 여기에 등록
        public UnityAction Ondie; // 죽음 
        // 힐
        public UnityAction<float> OnHeal; // 힐이발생 @@

        // 체력 위험경계 비율 
        [SerializeField]private float criticalHealRatio = 0.3f;

        // 무적 상태 
        public bool Invincible {  get; private set; }

        #endregion

        // 힐 아이템을 먹을 수 있는지 체크
        public bool CanPickUp() => CurrentHealtH < maxHealth; // 체력이 만땅이 아닐때 

        // UI에 HP게이지 값
        public float GetRatio() => CurrentHealtH / maxHealth; // 현재체력 / 최대체력 비율 

        // 체력이 일정 이하로 떨어지면 알람 - 위험체크
        public bool IsCritical() => GetRatio() <= criticalHealRatio ;

        private void Start()
        {
            // 초기화
            CurrentHealtH = maxHealth;
            Invincible = false;
        }

        // 힐 처리
        public void Heal(float amount)
        {
            float beforeHealth = CurrentHealtH;
            CurrentHealtH += amount;
            CurrentHealtH = Mathf.Clamp(CurrentHealtH, 0f, maxHealth);

            // real Heal 구하기
            float realHeal = CurrentHealtH - beforeHealth;
            if (realHeal >0)
            {
                // 힐 구현
                OnHeal?.Invoke(realHeal);
            }
        }
        // 데미지 처리
        // damageSource : 데미지를 주는 주체 ★ 
        public void TakeDamage(float damage, GameObject damageSource)
        {
            
            // 무적 체크
            if (Invincible)
                return;

       //     Debug.Log($"damage:{damage}");

            float beforeHealth = CurrentHealtH; // 데미지 입기전의 hp
            
            CurrentHealtH -= damage;

            CurrentHealtH = Mathf.Clamp(CurrentHealtH, 0, maxHealth); // 0이하로 안떨어짐 0 이하값은 0으로 보정
          //  Debug.Log($"hp:{CurrentHealtH}");

            // real데미지 구하기
            float realDamage = beforeHealth - CurrentHealtH;
            if(realDamage > 0f)
            {
                // 데미지 구현 
             OnDamaged?.Invoke(realDamage, damageSource ); // 실제 들어가는 데미지, 데미지를 주는 주체 / @@@? ~ null이 아니면 실행
            }

            // 죽음처리 함수 
            HandleDeath();
        }

        // 죽음처리
        void HandleDeath()
        {
            // 죽음 상태 체크 / 이중 죽음 방지
            if (isDeath)
                return;
                
            if(CurrentHealtH <= 0f)
            {
                isDeath = true;

                // 죽음 구현 
                Ondie?.Invoke();
            }
        }
    }
}