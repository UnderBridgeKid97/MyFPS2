using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    ///  enemy ����Ʈ�� �����ϴ� Ŭ���� 
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        #region Variables

        public List<EnemyController> Enemies {  get; private set; }
        public int NumberOfEnemiesTotal { get; private set; }           // �� ����� ���ʹ��� ���� ��
        public int NumberOfEnemiesRemaining => Enemies.Count;           // ���� ����ִ� ���ʹ��� ���� ��


        #endregion

        private void Awake()
        {
            Enemies = new List<EnemyController>();
        }

        // ���
        public void RegisterEnemy(EnemyController newEnemy)
        {
            Enemies.Add(newEnemy);
            NumberOfEnemiesTotal++;
        }

        // ���� 
        public void RemoveEnemy(EnemyController killedenemy)
        {
            Enemies.Remove(killedenemy);


        }

    }
}