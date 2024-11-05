using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// ����(weapon)�� �����ϴ� Ŭ���� 
    /// 
    /// </summary>
    
    // ���� ��ü ����
    

    public class WeaponController : MonoBehaviour
    {
        #region Variavles
        // ���⸦ Ȱ��ȭ, ��Ȱ��ȭ
        public GameObject weaponRoot; // ���� �𵨸� gunroot ���� �ִ�

        //  
        public GameObject Owner { get; set; }            // ������ ���� 
        public GameObject SourcePrefab { get; set; }     // ���⸦ ������ �������� ������ 
        public bool IsweaponActive { get; private set; } // ���� Ȱ��ȭ ���� 

        private AudioSource shotAudioSource;
        public AudioClip switchchWeaponSFX;

        #endregion

        private void Awake()
        {
            // ���� 
            shotAudioSource = GetComponent<AudioSource>();
        }

        // ���⸦ Ȱ��ȭ, ��Ȱ��ȭ
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            // this ����� ���� 
            if(show == true && switchchWeaponSFX != null)
            {
                // ���� ���� ȿ�� �÷���
                shotAudioSource.PlayOneShot(switchchWeaponSFX);

            }

            IsweaponActive = show;

        }



    }
}