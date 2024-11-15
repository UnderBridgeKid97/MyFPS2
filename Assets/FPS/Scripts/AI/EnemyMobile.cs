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

            audioSource = GetComponent<AudioSource>();
            audioSource.clip = movementSound;
            audioSource.Play();

            // 초기화 
            AIState = AIState.Patrol;

        }

        private void Update()
        {
            // 상태구현
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
                    break;
                case AIState.Atack:
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


    }
}