using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


namespace Unity.FPS.AI
{/// <summary>
///  렌더러 데이터 : 메테리얼 정보 저장 
/// </summary>
    [System.Serializable]
    public struct RendererIndexData
    {
        public Renderer renderer;
        public int metarialIndex;

        public RendererIndexData(Renderer _renderer, int index)
        {
            renderer = _renderer;
            metarialIndex = index;
        }
    }
    /// <summary>
    ///  Enemy를 관리하는 클래스
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        #region Variables

        private Health health;

        // death
        public GameObject deathVfxPrefab;
        public Transform deathVfxSpawnPosition;

        // damage
        public UnityAction Damaged;

        // sfx
        public AudioClip damageSfx;

        // vfx
        public Material bodyMaterial;           // 데미지를 줄 메테리얼
        [GradientUsage(true)]                   // 
        public Gradient OnHitBodyGradient;      // 데지미를 컬러 그라디언트 효과로 표현
        private List<RendererIndexData> bodyRenderer = new List<RendererIndexData>();   // body material을 가지고있는 렌더러 리스트
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField]private float flashOnHitDuration = 0.5f;
       float lastTimeDamaged = float.NegativeInfinity;
        bool wasDamagerThisFrame = false;

        // patrol
        public NavMeshAgent Agent { get;private set;}
        public PatrolPath PatrolPath { get; set; }
        private int pathDestinationIndex;
        private float pathReachingRadius = 1f;          // 도착판정

        // Detection
        private Actor actor;
        private Collider[] selfColliders;

        public DetectionModule DetectionModule { get; private set; }

        public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;

        public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
        
        public bool HadKnownTarget => DetectionModule.HadKnownTarget;

        public Material eyeColorMaterial;
        [ColorUsage(true,true)]public Color defaultEyeColor;
        [ColorUsage(true,true)]public Color AttackEyeColor;

        // eye Material을 가지고 있는 렌더러 데이터
        private RendererIndexData eyeRendererData;
        private MaterialPropertyBlock eyeColorMaterialPropertyBlock;

        public UnityAction OnDetectedTarget;
        public UnityAction OnLostTarget;

        // attack
        public UnityAction OnAttack;

        private float OrientSpeed = 10f; // 무기 돌리는 속도 

        public bool IsTargetInAttackRange => DetectionModule.IstargetInAtackRange;
       
        public bool swapToNextWeapon = false;
        public float delayAfterWeaponSwaop = 0f;
        private float lastTimeWeaponSwapped = Mathf.NegativeInfinity;

        public int currentWeaponIndex;
        private WeaponController currentWeapon;
        private WeaponController[] weapons;

        // 이너미 메니저
        private EnemyManager enemyManager;
        #endregion

        private void Start()
        {
            // 참조 
            enemyManager =GameObject.FindObjectOfType<EnemyManager>();
            enemyManager.RegisterEnemy(this);                           // enemyManager등록
            
            Agent = GetComponent<NavMeshAgent>();

            actor = GetComponent<Actor>();
            selfColliders = GetComponentsInChildren<Collider>();

            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DetectionModule = detectionModules[0];
            DetectionModule.OnDetectedTarget += OnDetected;
            DetectionModule.OnLostTarget += OnLost;

            health = GetComponent<Health>();
            health.OnDamaged += OnDamaged;
            health.Ondie += OnDie;

            // 무기 초기화
            FindAndInitializeAllWeapon();
            var weapon = GetCurrentWeapon();
            weapon.ShowWeapon(true);

            // body material을 가지고 있는 렌더러 정보 리스트 만들기
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                for(int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    // body
                    if(renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));
                    }

                    // eye
                    if (renderer.sharedMaterials[i] == eyeColorMaterial)
                    {
                        eyeRendererData = new RendererIndexData(renderer, i);
                    }

                }
            }

            // body
            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

            // eye
            if(eyeRendererData.renderer != null)
            {
                eyeColorMaterialPropertyBlock = new MaterialPropertyBlock();
                eyeColorMaterialPropertyBlock.SetColor("_EmssionColor",defaultEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                                                          eyeRendererData.metarialIndex);
            }
        }

        private void Update()
        {
            // 디텍션
            DetectionModule.HandleTargetDetection(actor, selfColliders);

            // 데미지 효과
            Color currentColor = OnHitBodyGradient.Evaluate((Time.time - lastTimeDamaged)/flashOnHitDuration);
            bodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach(var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlashMaterialPropertyBlock,data.metarialIndex);
            }
            //
            wasDamagerThisFrame = false;
        }

        private void OnDamaged(float damage,GameObject damageSource)
        {
            if(damageSource && damageSource.GetComponent<EnemyController>() == null)
            {
                //  등록된 함수 호출
                Damaged?.Invoke();

                // 데미지를 준 시간
                lastTimeDamaged = Time.time;


                // sfx
                if(damageSfx && wasDamagerThisFrame == false)
                {
                AudioUtility.CreateSfx(damageSfx, this.transform.position, 0f);
                }
                wasDamagerThisFrame = true; 


            }
        }

        private void OnDie()
        {
            //  이너미 메니저 리스트에서 제거
            enemyManager.RemoveEnemy(this);

            // 폭발효과
            GameObject EffectGo = Instantiate(deathVfxPrefab,deathVfxSpawnPosition.position, Quaternion.identity);
            Destroy(EffectGo,5f);

            // enemy kill
            Destroy(gameObject);
        }

        // 패트롤이 유효한지? => 패트롤이 가능한지?
        private bool IspathVaild()
        {
            return PatrolPath && PatrolPath.wayPoints.Count > 0; // 포인트가 1개 이상이고 0이상일때
        }

        // 가장 가까운 waypoint 찾기
        private void SetPathDestinationToClosestWayPoint()
        { 
            if(IspathVaild()==false)
            {
                pathDestinationIndex = 0;
                return;
            }

            int closestWayPointIndex = 0;

            for (int i = 0; i < PatrolPath.wayPoints.Count; i++)
            {
                float distance = PatrolPath.GetDistanceToWayPoint(transform.position,i);
                float closestDistance = PatrolPath.GetDistanceToWayPoint(transform.position,closestWayPointIndex);
                if (distance < closestDistance)
                {
                    closestWayPointIndex = i;
                }

            }


            pathDestinationIndex = closestWayPointIndex;

        }
        // 목표지점의 위치값 얻어오기
        public Vector3 GetDestinationOnPath()
        {
            if(IspathVaild() == false)
            {
                return this.transform.position;
            }

            return PatrolPath.GetPostionOfWayPoint(pathDestinationIndex);
        }

        // 목표 지점 설정 - nav 시스템 이용
         public void SetNavDestination(Vector3 destination)
        {
            if (Agent)
            {
                  Agent.SetDestination(destination);
            }
        }

        // 도착 판정 후 다음 목표지점 설정 
        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IspathVaild() == false)
            {
                return;
            }

            // 도착판정 
            float distance = (transform.position - GetDestinationOnPath()).magnitude;
            if(distance <pathReachingRadius)
            {
                pathDestinationIndex = inverseOrder ? (pathDestinationIndex -1) : (pathDestinationIndex + 1);

                if (pathDestinationIndex < 0)
                {
                    pathDestinationIndex += PatrolPath.wayPoints.Count;

                }
                if (pathDestinationIndex >=PatrolPath.wayPoints.Count)

                {
                    pathDestinationIndex -= PatrolPath.wayPoints.Count;
                }
            }
        }

        public void OrientToward(Vector3 lookPosition)
        {
            Vector3 lookDirect = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up);
            if(lookDirect.sqrMagnitude !=0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirect);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, OrientSpeed * Time.deltaTime);
            }
        }

        // 적 감지시 호출되는 함수
        private void OnDetected()
        {
            OnDetectedTarget?.Invoke();

            if(eyeRendererData.renderer)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmssionColor", AttackEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                                                          eyeRendererData.metarialIndex);
            }

        }
        //  적 소실 후 호출되는 함수 
        private void OnLost()
        {
            OnLostTarget?.Invoke();

            if (eyeRendererData.renderer)
            {
                eyeColorMaterialPropertyBlock.SetColor("_EmssionColor", defaultEyeColor);
                eyeRendererData.renderer.SetPropertyBlock(eyeColorMaterialPropertyBlock,
                                                          eyeRendererData.metarialIndex);
            }

        }

        // 가지고 있는 무기 찾고 초기화 
        private void FindAndInitializeAllWeapon()
        {
            if(weapons == null)
            {
                weapons = this.GetComponentsInChildren<WeaponController>();

                for(int i = 0; i < weapons.Length; i++)
                {
                    weapons[i].Owner = this.gameObject;
                }
            }

        }

        // 지정한 인덱스에 해당하는 무기를 current로 지정
        private void SetCurrentWeapon(int index)
        {
            currentWeaponIndex = index;
            currentWeapon = weapons[currentWeaponIndex];
            if(swapToNextWeapon)
            {
                lastTimeWeaponSwapped = Time.time;
            }
            else
            {
                lastTimeWeaponSwapped = Mathf.NegativeInfinity;
            }
        }

        // 현재 current weapon 찾기
        public WeaponController GetCurrentWeapon()
        {
            FindAndInitializeAllWeapon();
            if(currentWeapon == null)
            {
                SetCurrentWeapon(0);
            }
            return currentWeapon;
        }

        // 적에게 총구를 돌린다
        public void OrientWeaponsToward(Vector3 lookPosition)
        {
            for(int i = 0;i < weapons.Length; i++)
            {
                Vector3 weaponForward = (lookPosition - weapons[i].transform.position).normalized;
                weapons[i].transform.forward = weaponForward;
            }
        }

        // 공격 - 공격성공, 실패
        public bool TryAttack(Vector3 targetPosition)
        {
            // 무기 교체시 딜레이 시간동안 공격 불가능
            if(lastTimeWeaponSwapped + delayAfterWeaponSwaop >=Time.time)
            {
                return false;
            }

            // 무기shoot
          bool didFire =  GetCurrentWeapon().HandleShootInputs(false, true, false);
            if(didFire && OnAttack !=null)
            {
                OnAttack?.Invoke();

                // 발사를 한번할 때 마다 다음 무기로 교체
                if(swapToNextWeapon == true && weapons.Length >1)
                {
                    int nextWeaponIndex = (currentWeaponIndex + 1)% weapons.Length ;
                    SetCurrentWeapon(nextWeaponIndex);
                }

            }


            return true;

        }



       
    }
}