using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using Unity.VisualScripting;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    ///  발사체 표준형
    /// </summary>

    public class ProjectileStandard : ProjectileBase
    {
        #region Variables

        // 생성 
        private ProjectileBase projectileBase;
        private float maxLifeTime = 5f;

        // 이동
        [SerializeField]private float speed = 20f;
        [SerializeField]private float gravityDown = 0f;
        public Transform root;
        public Transform tip; // 프로젝타일 헤드

        private Vector3 velocity;   //벡터 속도
        private Vector3 lastRootPosition; // 
        private float shotTime; // 

        // 충돌
        private float radius = 0.01f;               // 충돌 검사하는 구체의 반경 
        public LayerMask hittableLayers = -1;       // Hit가 가능한 Layer 지정 
        private List<Collider> ignoredColliders;    // Hit판정시 무시하는 충돌체 리스트 -> 이 콜라이더는 hit판정을 무시함

        // 충돌연출
        public GameObject impactVFXPrefab;                            // 타격 이펙트
        [SerializeField] private float impactVFXlifeTime = 5f;  // 비쥬얼 효과
        private float impactVFXSpawmOffset = 0.1f;              // 오프셋 

        public AudioClip impactSFXClip;                         // 타격음 

        #endregion

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>(); // 자기가 상속받고있는 부모 켓컴하기
            projectileBase.OnShoot += OnShoot;

            // kill
            Destroy(gameObject, maxLifeTime);
        }

        //  shoot 값 설정
        new void OnShoot()
        {
            velocity = transform.forward * speed; // 
            transform.position += projectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            lastRootPosition = root.position; 

            // 충돌 무시 리스트 생성 - projectile을 발사하는 자신의 충돌체를 가져와서 등록 
            ignoredColliders= new List<Collider>();
            Collider[] ownerColliders = projectileBase.Owner.GetComponentsInChildren<Collider>(); // 주체에 있는 모든 콜라이더를 등록 GetComponent!s!InChildren
            ignoredColliders.AddRange(ownerColliders); //  AddRange =  지정한 모든 리스트 포함 

            // 총구가 벽을 뚫고 쏠 수 있는 버그 수정
            
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
                        //  카메라부터 머즐까지 사이에 유효한 충돌체가 있으면
                        OnHit(hit.point, hit.normal, hit.collider); 
                    }
                }

            }
        }

        private void Update()
        {
            // 이동
            transform.position += velocity * Time.deltaTime; // 

            // 중력
            if(gravityDown > 0f)
            {
                velocity += Vector3.down * gravityDown * Time.deltaTime;
            }

            // 충돌
            RaycastHit cloestHit = new RaycastHit(); // 가장 가까운 충돌체 체크
            cloestHit.distance = Mathf.Infinity;
            bool foundHit = false;                   // hit한 충돌체를 찾았는지 여부 확인 

            // sphere Cast 
            Vector3 displacementSinceLastFrame = tip.position - lastRootPosition; // 마지막으로 부터 현재까지 위치한 거리 
            RaycastHit[] hits = Physics.SphereCastAll(lastRootPosition, radius,
                displacementSinceLastFrame.normalized,displacementSinceLastFrame.magnitude,
                hittableLayers,QueryTriggerInteraction.Collide);

            foreach(var hit in hits)
            {
                if( IsHitValid(hit) && hit.distance < cloestHit.distance) // 유요한 hit인지 판정 후 거리검사 
                {
                    foundHit = true; 
                    cloestHit = hit;
                }
            }
            // hit한 충돌체를 찾으면
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

        // 유효한 hit인지 판정
        bool IsHitValid(RaycastHit hit)
        {
            // IgnoreHitDectection 컴포넌트를 가진 콜라이더 무시
            if(hit.collider.GetComponent<IgnoreHitDectection>())
            {
                return false;
            }

            // /gnoredColliders에 포함된 콜라이더 무시 
            if(ignoredColliders != null && ignoredColliders.Contains(hit.collider)) 
            {
                return false;
            }

            // trigger collider  & Damageable가 없으면 
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null) // is trigger에 체크했으면 = 트리거면
            {
                return false;
            }


            return true;
        }

        // Hit 구현, 데미지 판정, vfx, sfx ..
        void OnHit(Vector3 point, Vector3 normal, Collider collider) //위치 방향 콜라이더
        {
            // vfx
            if(impactVFXPrefab) // 임펙트가 있으면
            {
               GameObject impactObject = Instantiate(impactVFXPrefab, point + (normal * impactVFXSpawmOffset),
                   Quaternion.LookRotation(normal)); // 충돌 오브젝트에 오프셋을 약간 줘서 벽에 박혀 안보이는걸 방지

                if(impactVFXlifeTime >0f)
                {
                    Destroy(impactObject, impactVFXlifeTime);
                }
            }

            // sfx
            if(impactSFXClip) // null이 아니면
            {
                // 충돌위치에 게임 오브젝트를 생성하고 AudioSource 컴포넌트를 추가해서 지정된 클립을 플레이 
                AudioUtility.CreateSfx(impactSFXClip, point, 1f,3f);

            }



            // 발사체 kill
            Destroy(gameObject);
        }

    }
}