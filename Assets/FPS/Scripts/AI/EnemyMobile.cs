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

        //
        public AudioClip movementSound;
        public MinMaxFloat pitchMovementSpeed;

        private AudioSource audioSource;

        // animation parameter
        const string k_AnimAttackParameter = "Attack";
        const string k_AnimMoveSpeedParameter = "MoveSpeed";
        const string k_AnimAlertedParameter = "Alerted";
        const string k_AnimOnDamageParameter = "OnDamage";
        const string k_AnimDeathParameter = "Death";

        #endregion

        private void Start()
        {
            // ����

            enemyController = GetComponent<EnemyController>();
            enemyController.Damaged += OnDamaged;

            audioSource = GetComponent<AudioSource>();
            audioSource.clip = movementSound;
            audioSource.Play();

            // �ʱ�ȭ 
            AIState = AIState.Patrol;

        }

        private void Update()
        {
            UpdateCurrentAIState();
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
                    break;
                case AIState.Atack:
                    break;

            }
        }

        private void OnDamaged()
        {

        }


    }
}