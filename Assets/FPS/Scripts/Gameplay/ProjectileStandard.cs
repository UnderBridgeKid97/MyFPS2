using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.VisualScripting;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    ///  �߻�ü ǥ����
    /// </summary>

    public class ProjectileStandard : ProjectileBase
    {
        #region Variables

        // ���� 
        private ProjectileBase projectileBase;
        private float maxLifeTime = 5f;

        // �̵�
        [SerializeField]private float speed = 20f;
        [SerializeField]private float gravityDown = 0f;
        public Transform root;
        public Transform tip; // ������Ÿ�� ���

        private Vector3 velocity;   //���� �ӵ�
        private Vector3 lastRootPosition; // 
        private float shotTime; // 

        // �浹
        private float radius = 0.01f;               // �浹 �˻��ϴ� ��ü�� �ݰ� 
        public LayerMask hittableLayers = -1;       // Hit�� ������ Layer ���� 
        private List<Collider> ignoredColliders;    // Hit������ �����ϴ� �浹ü ����Ʈ -> �� �ݶ��̴��� hit������ ������

        // �浹����
        public GameObject impactVFXPrefab;                            // Ÿ�� ����Ʈ
        [SerializeField] private float impactVFXlifeTime = 5f;  // ����� ȿ��
        private float impactVFXSpawmOffset = 0.1f;              // ������ 

        public AudioClip impactSFXClip;                         // Ÿ���� 

        #endregion

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>(); // �ڱⰡ ��ӹް��ִ� �θ� �����ϱ�
            projectileBase.OnShoot += OnShoot;

            // kill
            Destroy(gameObject, maxLifeTime);
        }

        //  shoot �� ����
        new void OnShoot()
        {
            velocity = transform.forward * speed; // 
            transform.position += projectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            lastRootPosition = root.position; 

            // �浹 ���� ����Ʈ ���� - projectile�� �߻��ϴ� �ڽ��� �浹ü�� �����ͼ� ��� 
            ignoredColliders= new List<Collider>();
            Collider[] ownerColliders = projectileBase.Owner.GetComponentsInChildren<Collider>(); // ��ü�� �ִ� ��� �ݶ��̴��� ��� GetComponent!s!InChildren
            ignoredColliders.AddRange(ownerColliders); //  AddRange =  ������ ��� ����Ʈ ���� 

            // �ѱ��� ���� �հ� �� �� �ִ� ���� ����
            
            PlayerWeaponsManager weaponsManager = projectileBase.Owner.GetComponent<PlayerWeaponsManager>(); 
            if(weaponsManager)
            {
                Vector3 cameraToMuzzle = projectileBase.InitialPosition - weaponsManager.weaponCamera.transform.position;
                if(Physics.Raycast(weaponsManager.weaponCamera.transform.position,cameraToMuzzle.normalized,
                    out RaycastHit hit, cameraToMuzzle.magnitude, hittableLayers,
                    QueryTriggerInteraction.Collide))
                {
                    if(IsHitValid(hit))
                    {
                        //  ī�޶���� ������� ���̿� ��ȿ�� �浹ü�� ������
                        OnHit(hit.point, hit.normal, hit.collider); 
                    }
                }

            }
        }

        private void Update()
        {
            // �̵�
            transform.position += velocity * Time.deltaTime; // 

            // �߷�
            if(gravityDown > 0f)
            {
                velocity += Vector3.down * gravityDown * Time.deltaTime;
            }

            // �浹
            RaycastHit cloestHit = new RaycastHit(); // ���� ����� �浹ü üũ
            cloestHit.distance = Mathf.Infinity;
            bool foundHit = false;                   // hit�� �浹ü�� ã�Ҵ��� ���� Ȯ�� 

            // sphere Cast 
            Vector3 displacementSinceLastFrame = tip.position - lastRootPosition; // ���������� ���� ������� ��ġ�� �Ÿ� 
            RaycastHit[] hits = Physics.SphereCastAll(lastRootPosition, radius,
                displacementSinceLastFrame.normalized,displacementSinceLastFrame.magnitude,
                hittableLayers,QueryTriggerInteraction.Collide);

            foreach(var hit in hits)
            {
                if( IsHitValid(hit) && hit.distance < cloestHit.distance) // ������ hit���� ���� �� �Ÿ��˻� 
                {
                    foundHit = true; 
                    cloestHit = hit;
                }
            }
            // hit�� �浹ü�� ã����
            if(foundHit)
            {
                if (cloestHit.distance <= 0f)
                {
                    cloestHit.point = root.position;
                    cloestHit.normal = -transform.forward;
                }

                OnHit(cloestHit.point,cloestHit.normal,cloestHit.collider);
            }

            lastRootPosition = root.position;
        }

        // ��ȿ�� hit���� ����
        bool IsHitValid(RaycastHit hit)
        {
            // IgnoreHitDectection ������Ʈ�� ���� �ݶ��̴� ����
            if(hit.collider.GetComponent<IgnoreHitDectection>())
            {
                return false;
            }

            // /gnoredColliders�� ���Ե� �ݶ��̴� ���� 
            if(ignoredColliders != null && ignoredColliders.Contains(hit.collider)) 
            {
                return false;
            }

            // trigger collider  & Damageable�� ������ 
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null) // is trigger�� üũ������ = Ʈ���Ÿ�
            {
                return false;
            }


            return true;
        }

        // Hit ����, ������ ����, vfx, sfx ..
        void OnHit(Vector3 point, Vector3 normal, Collider collider) //��ġ ���� �ݶ��̴�
        {
            // vfx
            if(impactVFXPrefab) // ����Ʈ�� ������
            {
               GameObject impactObject = Instantiate(impactVFXPrefab, point + (normal * impactVFXSpawmOffset),
                   Quaternion.LookRotation(normal)); // �浹 ������Ʈ�� �������� �ణ �༭ ���� ���� �Ⱥ��̴°� ����

                if(impactVFXlifeTime >0f)
                {
                    Destroy(impactObject, impactVFXlifeTime);
                }
            }

            // sfx
            if(impactSFXClip) // null�� �ƴϸ�
            {
                // �浹��ġ�� ���� ������Ʈ�� �����ϰ� AudioSource ������Ʈ�� �߰��ؼ� ������ Ŭ���� �÷��� 
                AudioUtility.CreateSfx(impactSFXClip, point, 1f,3f);

            }



            // �߻�ü kill
            Destroy(gameObject);
        }

    }
}