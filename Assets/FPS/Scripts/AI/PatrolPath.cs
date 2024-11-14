using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    ///  패트롤 waypoints를 관리하는 클래스 
    /// </summary>

    public class PatrolPath : MonoBehaviour
    {
        #region Variables

        public List<Transform> wayPoints = new List<Transform>();

        // this Path를 패트롤하는 enemy들 
        public List <EnemyController> enemiesToAssign = new List<EnemyController>();

        #endregion

        private void Start()
        {
            // 등록될 enemy에게 패트롤할 패스(this) 지정
            foreach(var enemy in enemiesToAssign)
            {
                enemy.PatrolPath = this;
            }
        }

        // 특정(enemy)위치로 부터 지정된 waypoint와의 거리 구하기
        public float GetDistanceToWayPoint(Vector3 origin, int wayPointIndex)
        {
            if(wayPointIndex <0 || wayPointIndex >= wayPoints.Count || wayPoints[wayPointIndex] == null)
            {
                return -1f;
            }

            return (wayPoints[wayPointIndex].position - origin).magnitude;
        }

        // index로 지정된 waypoint의 위치 반환
        public Vector3 GetPostionOfWayPoint(int index)
        {
            if (index < 0 || index >= wayPoints.Count || wayPoints[index] == null)
            {
                return Vector3.zero;
            }


            return wayPoints[index].position;
        }

        // 기즈모로 Path 그리기
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for(int i = 0; i < wayPoints.Count; i++)
            {
                int nextIndex = i + 1;
                
                if(nextIndex >= wayPoints.Count)
                {
                    nextIndex -= wayPoints.Count;
                }
                Gizmos.DrawLine(wayPoints[i].position, wayPoints[nextIndex].position);
                Gizmos.DrawSphere(wayPoints[i].position, 0.1f);
            }
        }








    }
}