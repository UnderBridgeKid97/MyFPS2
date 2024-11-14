using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


namespace Unity.FPS.AI
{/// <summary>
///  ������ ������ : ���׸��� ���� ���� 
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
    ///  Enemy�� �����ϴ� Ŭ����
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
        public Material bodyMaterial;           // �������� �� ���׸���
        [GradientUsage(true)]                   // 
        public Gradient OnHitBodyGradient;      // �����̸� �÷� �׶���Ʈ ȿ���� ǥ��
        private List<RendererIndexDate> bodyRenderer = new List<RendererIndexDate>();   // body material�� �������ִ� ������ ����Ʈ
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField]private float flashOnHitDuration = 0.5f;
       float lastTimeDamaged = float.NegativeInfinity;
        bool wasDamagerThisFrame = false;

        // patrol
        public NavMeshAgent Agent { get;private set;}
        public PatrolPath PatrolPath { get; set; }
        private int pathDestinationIndex;
        private float pathReachingRadius = 1f;          // ��������

        #endregion

        private void Start()
        {
            // ���� 

            Agent = GetComponent<NavMeshAgent>();

            health = GetComponent<Health>();

            health.OnDamaged += OnDamaged;
            health.Ondie += OnDie;

            // body material�� ������ �ִ� ������ ���� ����Ʈ �����
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
            // ������ ȿ��
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
                //  ��ϵ� �Լ� ȣ��
                Damaged?.Invoke();

                // �������� �� �ð�
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
            // ����ȿ��
            GameObject EffectGo = Instantiate(deathVfxPrefab,deathVfxSpawnPosition.position, Quaternion.identity);
            Destroy(EffectGo,5f);

            // enemy kill
            Destroy(gameObject);
        }

        // ��Ʈ���� ��ȿ����? => ��Ʈ���� ��������?
        private bool IspathVaild()
        {
            return PatrolPath && PatrolPath.wayPoints.Count > 0; // ����Ʈ�� 1�� �̻��̰� 0�̻��϶�
        }

        // ���� ����� waypoint ã��
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
        // ��ǥ������ ��ġ�� ������
        public Vector3 GetDestinationOnPath()
        {
            if(IspathVaild() == false)
            {
                return this.transform.position;
            }

            return PatrolPath.GetPostionOfWayPoint(pathDestinationIndex);
        }

        // ��ǥ ���� ���� - nav �ý��� �̿�
         public void SetNavDestination(Vector3 destination)
        {
            if (Agent)
            {
                  Agent.SetDestination(destination);
            }
        }

        // ���� ���� �� ���� ��ǥ���� ���� 
        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (IspathVaild() == false)
            {
                return;
            }

            // �������� 
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