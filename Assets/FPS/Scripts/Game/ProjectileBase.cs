using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    ///  발사체의 기본이 되는 부모 클래스 
    /// </summary>

    public abstract class ProjectileBase : MonoBehaviour
    {
        #region Variables

        public GameObject Owner {  get; private set; }  // 발사체의 주체 

        public Vector3 InitialPosition { get; private set; }

        public Vector3 InitialDirection { get;private set; }

        public Vector3 InheritedMuzzleVelocity { get; private set; }

        public float initialCharge {  get; private set; } // 초기 차징 값

        public UnityAction OnShoot;                         // 웨폰 발사시 등록된 메서드 호출

        #endregion

        public void Shoot(WeaponController controller)
        {
            // Projectile의 초기값 설정
            Owner = controller.Owner;                                   // 주체 
            InitialPosition = this.transform.position;                  // 위치  
            InitialDirection = this.transform.forward;                  // 나가가는 방향
            InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;   // 총구의 속도
            initialCharge = controller.CurrentCharge;                   // 초기 차지값

            OnShoot?.Invoke();
        }



    }
}