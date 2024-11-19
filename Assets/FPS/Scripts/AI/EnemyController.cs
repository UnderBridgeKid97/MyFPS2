using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


namespace Unity.FPS.AI
{/// <summary>
///  ������ ������ : ���׸��� ���� ���� 
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
        private List<RendererIndexData> bodyRenderer = new List<RendererIndexData>();   // body material�� �������ִ� ������ ����Ʈ
        MaterialPropertyBlock bodyFlashMaterialPropertyBlock;

        [SerializeField]private float flashOnHitDuration = 0.5f;
       float lastTimeDamaged = float.NegativeInfinity;
        bool wasDamagerThisFrame = false;

        // patrol
        public NavMeshAgent Agent { get;private set;}
        public PatrolPath PatrolPath { get; set; }
        private int pathDestinationIndex;
        private float pathReachingRadius = 1f;          // ��������

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

        // eye Material�� ������ �ִ� ������ ������
        private RendererIndexData eyeRendererData;
        private MaterialPropertyBlock eyeColorMaterialPropertyBlock;

        public UnityAction OnDetectedTarget;
        public UnityAction OnLostTarget;

        // attack
        public UnityAction OnAttack;

        private float OrientSpeed = 10f; // ���� ������ �ӵ� 

        public bool IsTargetInAttackRange => DetectionModule.IstargetInAtackRange;
       
        public bool swapToNextWeapon = false;
        public float delayAfterWeaponSwaop = 0f;
        private float lastTimeWeaponSwapped = Mathf.NegativeInfinity;

        public int currentWeaponIndex;
        private WeaponController currentWeapon;
        private WeaponController[] weapons;

        // �̳ʹ� �޴���
        private EnemyManager enemyManager;
        #endregion

        private void Start()
        {
            // ���� 
            enemyManager =GameObject.FindObjectOfType<EnemyManager>();
            enemyManager.RegisterEnemy(this);                           // enemyManager���
            
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

            // ���� �ʱ�ȭ
            FindAndInitializeAllWeapon();
            var weapon = GetCurrentWeapon();
            weapon.ShowWeapon(true);

            // body material�� ������ �ִ� ������ ���� ����Ʈ �����
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
            // ���ؼ�
            DetectionModule.HandleTargetDetection(actor, selfColliders);

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
            //  �̳ʹ� �޴��� ����Ʈ���� ����
            enemyManager.RemoveEnemy(this);

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

        public void OrientToward(Vector3 lookPosition)
        {
            Vector3 lookDirect = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up);
            if(lookDirect.sqrMagnitude !=0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirect);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, OrientSpeed * Time.deltaTime);
            }
        }

        // �� ������ ȣ��Ǵ� �Լ�
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
        //  �� �ҽ� �� ȣ��Ǵ� �Լ� 
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

        // ������ �ִ� ���� ã�� �ʱ�ȭ 
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

        // ������ �ε����� �ش��ϴ� ���⸦ current�� ����
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

        // ���� current weapon ã��
        public WeaponController GetCurrentWeapon()
        {
            FindAndInitializeAllWeapon();
            if(currentWeapon == null)
            {
                SetCurrentWeapon(0);
            }
            return currentWeapon;
        }

        // ������ �ѱ��� ������
        public void OrientWeaponsToward(Vector3 lookPosition)
        {
            for(int i = 0;i < weapons.Length; i++)
            {
                Vector3 weaponForward = (lookPosition - weapons[i].transform.position).normalized;
                weapons[i].transform.forward = weaponForward;
            }
        }

        // ���� - ���ݼ���, ����
        public bool TryAttack(Vector3 targetPosition)
        {
            // ���� ��ü�� ������ �ð����� ���� �Ұ���
            if(lastTimeWeaponSwapped + delayAfterWeaponSwaop >=Time.time)
            {
                return false;
            }

            // ����shoot
          bool didFire =  GetCurrentWeapon().HandleShootInputs(false, true, false);
            if(didFire && OnAttack !=null)
            {
                OnAttack?.Invoke();

                // �߻縦 �ѹ��� �� ���� ���� ����� ��ü
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