using UnityEngine;

namespace Unity.FPS.Game
{/// <summary>
///  timeselfdestruct 부착한 게임 오브젝트는 생성 후 지정된 시간에 kill 처리 
/// </summary>
    public class TimeSelfDestruct : MonoBehaviour
    {
        #region Variables

        public float lifeTime = 1f;
        private float spawnTime;    // 생성될때의 시간 

        #endregion

        private void Awake()
        {
            // 생성될떄의 시간을 스폰타임에 저장
            spawnTime = Time.time;
        }

        private void Update()
        {
            if((spawnTime + lifeTime) <= Time.time) // 생성시간+ 라이프타임(1초) < 현재시간 
            {
                Destroy(gameObject);
            }
        }

    }
}