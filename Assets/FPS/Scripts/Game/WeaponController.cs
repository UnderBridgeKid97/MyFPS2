using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 크로스헤어를 관리하는 데이터
    /// </summary>

    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CrossHairSprite;
        public float CrossHairSize;
        public Color CrossHairColor;
    }

    /// <summary>
    /// 무기 슛 타입
    /// </summary>
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
        Snipe

    }


    /// <summary>
    /// 무기(weapon)을 그리기 위한 클래스 
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
        public AudioClip switchchWeaponSFX; // 무기 교환음

        // shooting
      public WeaponShootType shootType;

        // ammo
        [SerializeField]private float maxAmmo = 8f;     // 총에 장전할 수 있는 최대 총알 개수 
        private float currentAmmo;

        [SerializeField]private float delayBetweenShots = 0.5f; // 슛 간격 
        private float lastTimeShot;                             // 마지막으로 슛한 시간

        // vfx, sfx
        public Transform weaponMuzzle;                          // 총구 좌표값
        public GameObject MuzzleFlashPrefab;                    // 발사 효과  이펙트 효과 
        public AudioClip shootSfx;                              // 총알 발사 사운드 

        // 조준
        public float aimZoomRatio=1f; // 조준시 줌인 설정값
        public Vector3 aimOffset;     // 조준시 무기 위치 조정값

        // CrossHair
        public CrossHairData crosshairDefault; // 기본, 상시의 크로스헤어 
        public CrossHairData crosshairTargetInSight; // 적 포착 & 타겟팅시에 변경되는 크로스헤어

        // 반동
        public float recoilForce = 0.5f; // 반동이 되는 힘

        // Projectile
        public Vector3 MuzzleWorldVelocity {  get; private set; }   
        private Vector3 lastMuzzlePosition;                         // 마지막 총구 위치
        public float CurrentCharge {  get; private set; }

        // projectile
        public ProjectileBase projectilePrefab; // 프로젝타일 베이스를 상속받는 프리팹 

        #endregion

        private void Awake()
        {
            // 참조 
            shotAudioSource = GetComponent<AudioSource>();
        }
        private void Start()
        {
            currentAmmo = maxAmmo;
            lastTimeShot = Time.time; // 시작하자마자 쏠 수 있게
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

        // 키 입력에 따른 슛 타입 구현
        public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
        {
            switch(shootType)
            {
                case WeaponShootType.Manual:
                    if(inputDown)
                    {
                      return Tryshoot();
                    }
                    break;

                case WeaponShootType.Automatic:
                    if(inputHeld)
                    {
                       return Tryshoot();
                    }
                    break;

                case WeaponShootType.Charge:
                    break;

                case WeaponShootType.Snipe:
                    if(inputDown)
                    {
                        return Tryshoot();
                    }
                    break;
            }
            return false;
        }

        bool Tryshoot()
        {
            // 총알이1발 이상이고 쿨타임이 돌면
            if (currentAmmo >= 1f && (lastTimeShot + delayBetweenShots) < Time.time) 
            {
                currentAmmo -= 1f; 
                Debug.Log($"currentAmmo:{currentAmmo}");

                HandleShoot();


                return true;
            }

            return false;
        }

        // 슛 연출
        void HandleShoot()
        {
            // vfx
            if(MuzzleFlashPrefab) // 샷건은 머즐임펙트가 없으니까 있는지 물어봐야함
            {
                GameObject effectGo = Instantiate(MuzzleFlashPrefab, weaponMuzzle.position,
                                                       weaponMuzzle.rotation, weaponMuzzle);
                Destroy(effectGo,2f);
            }

            // sfx
            if(shootSfx)
            {
             shotAudioSource.PlayOneShot(shootSfx);

            }


            lastTimeShot = Time.time;

        }
    }
}