using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    ///  충전용 발사체를 발사할때 발사체의 속성값을 설정 
    /// </summary>
    public class ProjectileChargeParameter : MonoBehaviour
    {
        #region Variables

        private ProjectileBase projectileBase;

        // 
        public MinMaxFloat Damage;
        public MinMaxFloat Speed;
        public MinMaxFloat GravityDown;
        public MinMaxFloat Radius;

        #endregion

        private void OnEnable() // 활성화 될때 참조함
        {
            // 참조 
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        // 발사체 발사시 ProjectileBased의 OnShoot 델리게이트 함수에서 호출
        // 발사의 속성값을 charge값에 따라 설정
        void OnShoot()
        {
            // 충전량에 따라 발사체 속성값 설정 
            ProjectileStandard projectileStandard = GetComponent<ProjectileStandard>(); // 객체 가져오기
            projectileStandard.damage = Damage.GetValueFromRatio(projectileBase.initialCharge);
            projectileStandard.speed = Speed.GetValueFromRatio(projectileBase.initialCharge);
            projectileStandard.gravityDown = GravityDown.GetValueFromRatio(projectileBase.initialCharge);
            projectileStandard.radius = Radius.GetValueFromRatio(projectileBase.initialCharge);
        }

    }
}