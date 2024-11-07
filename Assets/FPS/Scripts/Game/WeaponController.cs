using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// ũ�ν��� �����ϴ� ������
    /// </summary>

    [System.Serializable]
    public struct CrossHairData
    {
        public Sprite CrossHairSprite;
        public float CrossHairSize;
        public Color CrossHairColor;
    }

    /// <summary>
    /// ���� �� Ÿ��
    /// </summary>
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge,
        Snipe

    }


    /// <summary>
    /// ����(weapon)�� �׸��� ���� Ŭ���� 
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
        public AudioClip switchchWeaponSFX; // ���� ��ȯ��

        // shooting
      public WeaponShootType shootType;

        // ammo
        [SerializeField]private float maxAmmo = 8f;     // �ѿ� ������ �� �ִ� �ִ� �Ѿ� ���� 
        private float currentAmmo;

        [SerializeField]private float delayBetweenShots = 0.5f; // �� ���� 
        private float lastTimeShot;                             // ���������� ���� �ð�

        // vfx, sfx
        public Transform weaponMuzzle;                          // �ѱ� ��ǥ��
        public GameObject MuzzleFlashPrefab;                    // �߻� ȿ��  ����Ʈ ȿ�� 
        public AudioClip shootSfx;                              // �Ѿ� �߻� ���� 

        // ����
        public float aimZoomRatio=1f; // ���ؽ� ���� ������
        public Vector3 aimOffset;     // ���ؽ� ���� ��ġ ������

        // CrossHair
        public CrossHairData crosshairDefault; // �⺻, ����� ũ�ν���� 
        public CrossHairData crosshairTargetInSight; // �� ���� & Ÿ���ýÿ� ����Ǵ� ũ�ν����

        // �ݵ�
        public float recoilForce = 0.5f; // �ݵ��� �Ǵ� ��

        // Projectile
        public Vector3 MuzzleWorldVelocity {  get; private set; }   
        private Vector3 lastMuzzlePosition;                         // ������ �ѱ� ��ġ
        public float CurrentCharge {  get; private set; }

        // projectile
        public ProjectileBase projectilePrefab; // ������Ÿ�� ���̽��� ��ӹ޴� ������ 

        #endregion

        private void Awake()
        {
            // ���� 
            shotAudioSource = GetComponent<AudioSource>();
        }
        private void Start()
        {
            currentAmmo = maxAmmo;
            lastTimeShot = Time.time; // �������ڸ��� �� �� �ְ�
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

        // Ű �Է¿� ���� �� Ÿ�� ����
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
            // �Ѿ���1�� �̻��̰� ��Ÿ���� ����
            if (currentAmmo >= 1f && (lastTimeShot + delayBetweenShots) < Time.time) 
            {
                currentAmmo -= 1f; 
                Debug.Log($"currentAmmo:{currentAmmo}");

                HandleShoot();


                return true;
            }

            return false;
        }

        // �� ����
        void HandleShoot()
        {
            // vfx
            if(MuzzleFlashPrefab) // ������ ��������Ʈ�� �����ϱ� �ִ��� ���������
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