using Unity.FPS.AI;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    ///  enemy 상태 
    /// </summary>
    public enum AIState
    {
        Patrol,
        Follow,
        Atack
    }


    /// <summary>
    ///  이동하는 enemy의 상태들을 구현하는 클래스 
    /// </summary>
    public class EnemyMobile : MonoBehaviour
    {
        #region Variables

        public Animator animator;
        private EnemyController enemyController;
        
        public AIState AIState {  get; private set; }

        // 이동
        public AudioClip movementSound;
        public MinMaxFloat pitchMovementSpeed;

        private AudioSource audioSource;

        // 데미지 - 이펙트
        public ParticleSystem[] randomHitSparks;

        // 디텍티드
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
            // 참조
            enemyController = GetComponent<EnemyController>();
            enemyController.Damaged += OnDamaged;
            enemyController.OnDetectedTarget += OnDetected;
            enemyController.OnLostTarget += OnLost;
            enemyController.OnAttack += Attacked;

            audioSource = GetComponent<AudioSource>();
            audioSource.clip = movementSound;
            audioSource.Play();

            // 초기화 
            AIState = AIState.Patrol;

        }

        private void Update()
        {
            // 상태 변경/구현
            UpdateAiStateTransition();
            UpdateCurrentAIState();
            
            // 속도에 따른 애니/사운드 효과
            float moveSpeed = enemyController.Agent.velocity.magnitude;
            animator.SetFloat(k_AnimMoveSpeedParameter, moveSpeed);     // 애니
            audioSource.pitch = pitchMovementSpeed.
                                GetValueFromRatio(moveSpeed/enemyController.Agent.speed);


        }

        // 상태의 따른 enemy 구현
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
                    // 일정거리가지는 이동하면서 공격
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

        // 상태변경에 따른 구현 
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
                        enemyController.SetNavDestination(transform.position); //정지
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
            // 스파크 파티클 - 랜덤하게 하나 선택해서 플레이
            if(randomHitSparks.Length >0)
            {
                int randNum = Random.Range(0,randomHitSparks.Length);
                randomHitSparks[randNum].Play();
            }

            // 데미지 애니
            animator.SetTrigger(k_AnimOnDamageParameter);
        }

        //
        private void OnDetected()
        {
            // 상태 변경
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
            // 상태변경
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
            // 애니   
            animator.SetTrigger(k_AnimDeathParameter);
        }

    }
}
