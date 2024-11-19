using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    ///  enemy ���� 
    /// </summary>
    public enum AIState
    {
        Patrol,
        Follow,
        Atack
    }


    /// <summary>
    ///  �̵��ϴ� enemy�� ���µ��� �����ϴ� Ŭ���� 
    /// </summary>
    public class EnemyMobile : MonoBehaviour
    {
        #region Variables

        public Animator animator;
        private EnemyController enemyController;
        
        public AIState AIState {  get; private set; }

        // �̵�
        public AudioClip movementSound;
        public MinMaxFloat pitchMovementSpeed;

        private AudioSource audioSource;

        // ������ - ����Ʈ
        public ParticleSystem[] randomHitSparks;

        // ����Ƽ��
        public ParticleSystem[] detectedVfx;
        public AudioClip detectedSfx;

        // attack
        [Range (0f,1f)]public float attackSkipDistanceRatio = 0.5f;
        // animation parameter
        const string k_AnimAttackParameter = "Attack";
        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamageParameter = "OnDamaged";
        const string k_AnimDeathParameter = "Death";

        #endregion

        private void Start()
        {
            // ����
            enemyController = GetComponent<EnemyController>();
            enemyController.Damaged += OnDamaged;
            enemyController.OnDetectedTarget += OnDetected;
            enemyController.OnLostTarget += OnLost;
            enemyController.OnAttack += Attacked;

            audioSource = GetComponent<AudioSource>();
            audioSource.clip = movementSound;
            audioSource.Play();

            // �ʱ�ȭ 
            AIState = AIState.Patrol;

        }

        private void Update()
        {
            // ���� ����/����
            UpdateAiStateTransition();
            UpdateCurrentAIState();
            
            // �ӵ��� ���� �ִ�/���� ȿ��
            float moveSpeed = enemyController.Agent.velocity.magnitude;
            animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);     // �ִ�
            audioSource.pitch = pitchMovementSpeed.
                                GetValueFromRatio(moveSpeed/enemyController.Agent.speed);


        }

        // ������ ���� enemy ����
        private void UpdateCurrentAIState()
        {
            switch (AIState)
            {
                case AIState.Patrol:
                    enemyController.UpdatePathDestination(true);
                    enemyController.SetNavDestination(enemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    enemyController.SetNavDestination(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.KnownDetectedTarget.transform.position);
                    break;
                case AIState.Atack:
                    // �����Ÿ������� �̵��ϸ鼭 ����
                    float distance = Vector3.Distance(enemyController.KnownDetectedTarget.transform.position,
                        enemyController.DetectionModule.DetectionSourcePoint.position);
                    if(distance >= enemyController.DetectionModule.attackRange * attackSkipDistanceRatio )
                    {
                        enemyController.SetNavDestination(enemyController.KnownDetectedTarget.transform.position);
                    }
                    else
                    {
                        enemyController.SetNavDestination(transform.position);
                    }
                    enemyController.OrientToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.KnownDetectedTarget.transform.position);
                    enemyController.TryAttack(enemyController.KnownDetectedTarget.transform.position);
                    break;

            }
        }

        // ���º��濡 ���� ���� 
        void UpdateAiStateTransition()
        {
            switch (AIState)
            {
                case AIState.Patrol:
                    break;
                case AIState.Follow:
                    if (enemyController.IsSeeingTarget && enemyController.IsTargetInAttackRange)
                    {
                        AIState = AIState.Atack;
                        enemyController.SetNavDestination(transform.position); //����
                    }
                    break;
                case AIState.Atack:
                   
                    if (enemyController.IsTargetInAttackRange == false)
                    {
                        AIState = AIState.Follow;
                    }
                        break;

            }

        }
            private void OnDamaged()
        {
            // ����ũ ��ƼŬ - �����ϰ� �ϳ� �����ؼ� �÷���
            if(randomHitSparks.Length >0)
            {
                int randNum = Random.Range(0,randomHitSparks.Length);
                randomHitSparks[randNum].Play();
            }

            // ������ �ִ�
            animator.SetTrigger(k_AnimOnDamageParameter);
        }

        //
        private void OnDetected()
        {
            // ���� ����
            if(AIState == AIState.Patrol)
            {
                AIState = AIState.Follow;
            }


            // VFX
            for (int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Play();
            }

            // SFX
            if(detectedSfx)
            {
                AudioUtility.CreateSfx(detectedSfx,this.transform.position,1f);
            }

            // anim
            animator.SetBool(k_AnimAlertedParameter, true);
        }

        private void OnLost()
        {
            // ���º���
            if(AIState == AIState.Follow && AIState == AIState.Atack)
            {
                AIState = AIState.Patrol;
            }


            // vfx
            for (int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Stop();
            }

            // anim
            animator.SetBool(k_AnimAlertedParameter, false);

        }

        private void Attacked()
        {
            // �ִ�   
            animator.SetTrigger(k_AnimDeathParameter);
        }

    }
}
