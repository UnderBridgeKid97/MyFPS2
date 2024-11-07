using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Game
{
    /// <summary>
    ///  �߻�ü�� �⺻�� �Ǵ� �θ� Ŭ���� 
    /// </summary>

    public abstract class ProjectileBase : MonoBehaviour
    {
        #region Variables

        public GameObject Owner {  get; private set; }  // �߻�ü�� ��ü 

        public Vector3 InitialPosition { get; private set; }

        public Vector3 InitialDirection { get;private set; }

        public Vector3 InheritedMuzzleVelocity { get; private set; }

        public float initialCharge {  get; private set; } // �ʱ� ��¡ ��

        public UnityAction OnShoot;                         // ���� �߻�� ��ϵ� �޼��� ȣ��

        #endregion

        public void Shoot(WeaponController controller)
        {
            // Projectile�� �ʱⰪ ����
            Owner = controller.Owner;                                   // ��ü 
            InitialPosition = this.transform.position;                  // ��ġ  
            InitialDirection = this.transform.forward;                  // �������� ����
            InheritedMuzzleVelocity = controller.MuzzleWorldVelocity;   // �ѱ��� �ӵ�
            initialCharge = controller.CurrentCharge;                   // �ʱ� ������

            OnShoot?.Invoke();
        }



    }
}