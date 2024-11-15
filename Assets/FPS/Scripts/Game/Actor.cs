using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    ///  ���ӿ� �����ϴ� actor
    /// </summary>
    public class Actor : MonoBehaviour
    {
        #region Variables

        // �Ҽ� - �Ʊ�, ���� ���� 
        public int afflliation;      // 0 ����, 1 �Ʊ�

        // ������
        public Transform aimPoint;

        private ActorManager actorManager;

        #endregion


        void Start ()
        {
            // Actor ����Ʈ�� �߰�(���)
            actorManager= GameObject.FindObjectOfType<ActorManager>();

            // Actor����Ʈ�� ���ԵǾ��ִ��� ���� üũ
            if (actorManager.Actors.Contains(this) == false) // false�� ���ԵǾ����� ����
            {
               actorManager.Actors.Add(this); // ����Ʈ�� �߰� 
            }

        }

        private void OnDestroy()
        {
            // ����Ʈ���� ����
            if(actorManager)
            {
               actorManager.Actors.Remove(this);
            }
        }



    }
}