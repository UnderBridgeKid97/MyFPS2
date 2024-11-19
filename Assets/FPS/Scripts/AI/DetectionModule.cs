using System;
using System.Linq;  // �迭�ȿ� ���ԵǾ��ִ���
using Unity.FPS.Game;
using Unity.VisualScripting;
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

        public GameObject KnownDetectedTarget { get; private set; }

        public bool HadKnownTarget { get; private set; }

        public bool IsSeeingTarget { get; private set; }

        public Transform DetectionSourcePoint;
        public float DetectionRange = 20f;  // �� ���� �Ÿ�

        public float knownTargetTimeout = 4f;

        private float TimeLastSeenTarget = Mathf.NegativeInfinity;

        // attack
        public float attackRange = 10f; // �� ���� �Ÿ�
        public bool IstargetInAtackRange { get; private set; }

        #endregion

        private void Start()
        {
            // ����
            actorManager = GameObject.FindObjectOfType<ActorManager>();
        }

        // ������

        #region
        public void HandleTargetDetection(Actor actor, Collider[] selfCollider)
        {
            if (KnownDetectedTarget && !IsSeeingTarget && (Time.time - TimeLastSeenTarget) > knownTargetTimeout)
            {
                KnownDetectedTarget = null;
            }

            float sqrDetectionRange = DetectionRange * DetectionRange;
            IsSeeingTarget = false;
            float closetSqrdistance = Mathf.Infinity;

            // ��� ���Ϳ��� ��� ����
            foreach (var otherActor in actorManager.Actors)
            {
                // �Ʊ��̸�
                if (otherActor.afflliation == actor.afflliation)
                {
                    continue;
                }

                float sqrDistance = (otherActor.aimPoint.position - DetectionSourcePoint.position).sqrMagnitude;
                if (sqrDistance < sqrDetectionRange && sqrDistance < closetSqrdistance)
                {
                    RaycastHit[] hits = Physics.RaycastAll(DetectionSourcePoint.position,
                        (otherActor.aimPoint.position - DetectionSourcePoint.position).normalized, DetectionRange,
                        -1, QueryTriggerInteraction.Ignore);

                    RaycastHit cloestHit = new RaycastHit();
                    cloestHit.distance = Mathf.Infinity;
                    bool foundValilHit = false;
                    foreach (var hit in hits)
                    {
                        if (hit.distance < cloestHit.distance && selfCollider.Contains(hit.collider) == false)
                        {
                            cloestHit = hit;
                            foundValilHit = true;
                        }
                    }

                    // ���� ã������ 
                    if (foundValilHit)
                    {
                        Actor hitActor = cloestHit.collider.GetComponentInParent<Actor>();
                        if (hitActor == otherActor)
                        {
                            IsSeeingTarget = true;
                            closetSqrdistance = sqrDistance;    // �ּ� ���ٽ� �ʱ�ȭ

                            TimeLastSeenTarget = Time.time; // �ð� ����
                            KnownDetectedTarget = otherActor.aimPoint.gameObject;
                            // Ÿ������ -> ���������� KnownDetectedTarget �� ����
                        }
                    }

                }
            }

            // attack range check
            IstargetInAtackRange = (KnownDetectedTarget != null) &&
                    Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <= attackRange;

            // ���� �𸣰� �ִٰ� ���� �߰��� ���� OnDetected ����
            if (HadKnownTarget == false && KnownDetectedTarget != null)
            {
                OnDetected();
            }
            // ���� ��� �ֽ��ϴٰ� ��ġ�� ���� OnLost ����
            if (HadKnownTarget == true && KnownDetectedTarget == null)
            {
                OnLost();
            }
            // ������ ���� ���� 
            HadKnownTarget = (KnownDetectedTarget != null);
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
        #endregion
    }


}
