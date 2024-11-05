using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// 죽었을 때 Health를 가진 모든 오브젝트를 킬하는 클래스 
    /// 
    /// </summary>

    public class Destructable : MonoBehaviour
    {
        #region Variables

        private Health Health;

        #endregion

        private void Start ()
        {   
            // 참조
            Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(Health, this,gameObject);

            // UnityAction event함수에 등록
            Health.Ondie += Ondie; // health의 이벤트 함수 Ondie에 디스트럭터블 Ondie를 등록
            Health.OnDamaged += OnDamage; //  @@

        }
        // 예제
        void OnDamage(float damage,GameObject damageSource)
        {
            // TODO : 데미지 효과 구현
        }



        void Ondie()
        {
            // 오브젝트 킬 
            Destroy(gameObject);
        }

    }
}