using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    ///  ������ �߻�ü�� �߻��Ҷ� �߻�ü�� �Ӽ����� ���� 
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

        private void OnEnable() // Ȱ��ȭ �ɶ� ������
        {
            // ���� 
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        // �߻�ü �߻�� ProjectileBased�� OnShoot ��������Ʈ �Լ����� ȣ��
        // �߻��� �Ӽ����� charge���� ���� ����
        void OnShoot()
        {
            // �������� ���� �߻�ü �Ӽ��� ���� 
            ProjectileStandard projectileStandard = GetComponent<ProjectileStandard>(); // ��ü ��������
            projectileStandard.damage = Damage.GetValueFromRatio(projectileBase.initialCharge);
            projectileStandard.speed = Speed.GetValueFromRatio(projectileBase.initialCharge);
            projectileStandard.gravityDown = GravityDown.GetValueFromRatio(projectileBase.initialCharge);
            projectileStandard.radius = Radius.GetValueFromRatio(projectileBase.initialCharge);
        }

    }
}