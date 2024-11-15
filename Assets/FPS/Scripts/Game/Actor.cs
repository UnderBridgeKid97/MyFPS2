using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    ///  게임에 등장하는 actor
    /// </summary>
    public class Actor : MonoBehaviour
    {
        #region Variables

        // 소속 - 아군, 적군 구분 
        public int afflliation;      // 0 적군, 1 아군

        // 조준점
        public Transform aimPoint;

        private ActorManager actorManager;

        #endregion


        void Start ()
        {
            // Actor 리스트에 추가(등록)
            actorManager= GameObject.FindObjectOfType<ActorManager>();

            // Actor리스트에 포함되어있는지 여부 체크
            if (actorManager.Actors.Contains(this) == false) // false면 포함되어있지 않음
            {
               actorManager.Actors.Add(this); // 리스트에 추가 
            }

        }

        private void OnDestroy()
        {
            // 리스트에서 삭제
            if(actorManager)
            {
               actorManager.Actors.Remove(this);
            }
        }



    }
}