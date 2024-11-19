using System;
using System.Linq;  // 배열안에 포함되어있는지
using Unity.FPS.Game;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.AI
{
    /// <summary>
    ///  적 디텍팅 구현
    /// </summary>
    public class DetectionModule : MonoBehaviour
    {
        #region Variables

        private ActorManager actorManager;

        public UnityAction OnDetectedTarget;        // 적을 감지하면 등록된 함수 호출
        public UnityAction OnLostTarget;            // 적을 놓치면 등록된 함수 호출 

        public GameObject KnownDetectedTarget { get; private set; }

        public bool HadKnownTarget { get; private set; }

        public bool IsSeeingTarget { get; private set; }

        public Transform DetectionSourcePoint;
        public float DetectionRange = 20f;  // 적 감지 거리

        public float knownTargetTimeout = 4f;

        private float TimeLastSeenTarget = Mathf.NegativeInfinity;

        // attack
        public float attackRange = 10f; // 적 공격 거리
        public bool IstargetInAtackRange { get; private set; }

        #endregion

        private void Start()
        {
            // 참조
            actorManager = GameObject.FindObjectOfType<ActorManager>();
        }

        // 디텍팅

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

            // 모든 엑터에게 계속 돌림
            foreach (var otherActor in actorManager.Actors)
            {
                // 아군이면
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

                    // 적을 찾았으면 
                    if (foundValilHit)
                    {
                        Actor hitActor = cloestHit.collider.GetComponentInParent<Actor>();
                        if (hitActor == otherActor)
                        {
                            IsSeeingTarget = true;
                            closetSqrdistance = sqrDistance;    // 최소 디스텐스 초기화

                            TimeLastSeenTarget = Time.time; // 시간 저장
                            KnownDetectedTarget = otherActor.aimPoint.gameObject;
                            // 타겟저장 -> 최종적으로 KnownDetectedTarget 을 구함
                        }
                    }

                }
            }

            // attack range check
            IstargetInAtackRange = (KnownDetectedTarget != null) &&
                    Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <= attackRange;

            // 적을 모르고 있다가 적을 발견한 순간 OnDetected 실행
            if (HadKnownTarget == false && KnownDetectedTarget != null)
            {
                OnDetected();
            }
            // 적을 계속 주시하다가 놓치는 순간 OnLost 실행
            if (HadKnownTarget == true && KnownDetectedTarget == null)
            {
                OnLost();
            }
            // 디텍팅 상태 저장 
            HadKnownTarget = (KnownDetectedTarget != null);
        }
        // 적을 감지하면 
        public void OnDetected()
        {
            OnDetectedTarget?.Invoke();
        }

        // 적을 놓치면
        public void OnLost()
        {
            OnLostTarget?.Invoke();
        }
        #endregion
    }


}
