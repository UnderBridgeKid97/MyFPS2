using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


namespace Unity.FPS.AI
{/// <summary>
///  렌더러 데이터 : 메테리얼 정보 저장 
/// </summary>
    [System.Serializable]
    public struct RendererIndexDate
    {
        public Renderer renderer;
        public int metarialIndex;

        public RendererIndexDate(Renderer _renderer, int index)
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
        private List<RendererIndexDate> bodyRenderer = new List<RendererIndexDate>();   // body material을 가지고있는 렌더러 리스트
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField]private float flashOnHitDuration = 0.5f;
       float lastTimeDamaged = float.NegativeInfinity;
        bool wasDamagerThisFrame = false;

        // patrol
        public NavMeshAgent Agent { get;private set;}
        public PatrolPath PatrolPath { get; set; }
        private int pathDestinationIndex;
        private float pathReachingRadius = 1f;          // 도착판정

        #endregion

        private void Start()
        {
            // 참조 

            Agent = GetComponent<NavMeshAgent>();

            health = GetComponent<Health>();

            health.OnDamaged += OnDamaged;
            health.Ondie += OnDie;

            // body material을 가지고 있는 렌더러 정보 리스트 만들기
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                for(int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if(renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderer.Add(new RendererIndexDate(renderer, i));
                    }
                }
            }
            bodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
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


    }
}