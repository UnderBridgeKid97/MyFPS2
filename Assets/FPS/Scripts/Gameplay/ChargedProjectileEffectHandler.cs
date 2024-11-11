using UnityEngine;
using Unity.FPS.Game;
using static Unity.FPS.Game.MinMaxFloat;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 충전용 발사체를 발사할때 발사체의 속성값을 설정 
    /// </summary>
    public class ChargedProjectileEffectHandler : MonoBehaviour
    {
        #region Variables

        private ProjectileBase projectileBase;

        public GameObject chargeObject;
        public MinMaxVector3 scale;

        #endregion

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        void OnShoot()
        {
            chargeObject.transform.localScale = scale.GetValueFromRatio(projectileBase.initialCharge);
        }









    }
}