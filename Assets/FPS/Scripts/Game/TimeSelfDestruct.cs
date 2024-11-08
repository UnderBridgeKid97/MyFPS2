using UnityEngine;

namespace Unity.FPS.Game
{/// <summary>
///  timeselfdestruct ������ ���� ������Ʈ�� ���� �� ������ �ð��� kill ó�� 
/// </summary>
    public class TimeSelfDestruct : MonoBehaviour
    {
        #region Variables

        public float lifeTime = 1f;
        private float spawnTime;    // �����ɶ��� �ð� 

        #endregion

        private void Awake()
        {
            // �����ɋ��� �ð��� ����Ÿ�ӿ� ����
            spawnTime = Time.time;
        }

        private void Update()
        {
            if((spawnTime + lifeTime) <= Time.time) // �����ð�+ ������Ÿ��(1��) < ����ð� 
            {
                Destroy(gameObject);
            }
        }

    }
}