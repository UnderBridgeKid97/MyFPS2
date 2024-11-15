using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    ///  �� ������ ����
    /// </summary>
    public class DetectionModule : MonoBehaviour
    {
        #region Variables

        private ActorManager actorManager;

        public UnityAction OnDetectedTarget;        // ���� �����ϸ� ��ϵ� �Լ� ȣ��
        public UnityAction OnLostTarget;            // ���� ��ġ�� ��ϵ� �Լ� ȣ�� 
        #endregion

        private void Start()
        {
            // ����
            actorManager = GameObject.FindObjectOfType<ActorManager>();
        }

        // ������
        public void HandleTargetDetection(Actor actor, Collider[] selfCollider)
        {

        }


        // ���� �����ϸ� 
        public void OnDetected()
        {
            OnDetectedTarget?.Invoke();     
        }
        // ���� ��ġ��
        public void OnLost()
        {
            OnLostTarget?.Invoke();
        }

    }
}