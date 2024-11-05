using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// 무기(weapon)을 관리하는 클래스 
    /// 
    /// </summary>
    
    // 무기 교체 상태
    

    public class WeaponController : MonoBehaviour
    {
        #region Variavles
        // 무기를 활성화, 비활성화
        public GameObject weaponRoot; // 무기 모델링 gunroot 껏다 켯다

        //  
        public GameObject Owner { get; set; }            // 무기의 주인 
        public GameObject SourcePrefab { get; set; }     // 무기를 생성한 오리지널 프리팹 
        public bool IsweaponActive { get; private set; } // 무기 활성화 여부 

        private AudioSource shotAudioSource;
        public AudioClip switchchWeaponSFX;

        #endregion

        private void Awake()
        {
            // 참조 
            shotAudioSource = GetComponent<AudioSource>();
        }

        // 무기를 활성화, 비활성화
        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            // this 무기로 변경 
            if(show == true && switchchWeaponSFX != null)
            {
                // 무기 변경 효과 플레이
                shotAudioSource.PlayOneShot(switchchWeaponSFX);

            }

            IsweaponActive = show;

        }



    }
}