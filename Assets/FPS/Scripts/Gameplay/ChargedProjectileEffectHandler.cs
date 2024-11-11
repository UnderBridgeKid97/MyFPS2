using UnityEngine;
using Unity.FPS.Game;
using static Unity.FPS.Game.MinMaxFloat;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// ������ �߻�ü�� �߻��Ҷ� �߻�ü�� �Ӽ����� ���� 
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