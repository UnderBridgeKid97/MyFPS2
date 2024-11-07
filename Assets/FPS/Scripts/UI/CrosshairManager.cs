using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Gameplay;

namespace Unity.FPS.UI
{

    public class CrosshairManager : MonoBehaviour
    {
        #region Variables

        public Image crosshairImage;            // ũ�ν���� UI �̹���
        public Sprite nullCrosshairSprite;      // ��Ƽ���� ���Ⱑ ������

        private RectTransform crosshairRectTransform;
        private CrossHairData crosshairDefault;  // ����, �⺻��
        private CrossHairData crosshairTarget;   // Ÿ���� �Ǿ�����

        private CrossHairData crosshaircurrent;  // ���������� �׸��� ũ�ν����
        [SerializeField]private float crosshairUpdateShrpness = 5.0f;   // Lerp����

        private PlayerWeaponsManager weaponsManager;

        private bool wasPointingAtEnemy;

        #endregion

        private void Start()
        {
            // ����
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>(); // ���� ������Ʈ �������� (���� �ϳ� �����ϴ� �����Ͽ� ���)
            // ��Ƽ���� ���� ũ�ν���� ���̱� 
            OnWeaponChanged(weaponsManager.GetActiveWeapon());

            weaponsManager.OnSwitchToWeapon += OnWeaponChanged;            
        }
        private void Update()
        {
            UpdateCrosshairPointAtEnemy(false);

            wasPointingAtEnemy = weaponsManager.IsPointingAtEnemy;
        }
        // ũ�ν� ��� �׸���
        void UpdateCrosshairPointAtEnemy(bool force)
        {
            if (crosshairDefault.CrossHairSprite == null)
                return;

            // ����?, Ÿ����?
            if((force || wasPointingAtEnemy ==false) && weaponsManager.IsPointingAtEnemy == true) // ���� �����ϴ� ����
            {
                crosshaircurrent = crosshairTarget;
                crosshairImage.sprite = crosshaircurrent.CrossHairSprite;
                crosshairRectTransform.sizeDelta = crosshaircurrent.CrossHairSize * Vector2.one;
            }
            else if ((force || wasPointingAtEnemy == true) && weaponsManager.IsPointingAtEnemy == false) // ���� ��ġ�� ����
            {
                crosshaircurrent = crosshairDefault;
                crosshairImage.sprite = crosshaircurrent.CrossHairSprite;
                crosshairRectTransform.sizeDelta = crosshaircurrent.CrossHairSize * Vector2.one;
            }

            crosshairImage.color = Color.Lerp(crosshairImage.color,crosshaircurrent.CrossHairColor
                ,crosshairUpdateShrpness*Time.deltaTime);
            crosshairRectTransform.sizeDelta = Mathf.Lerp(crosshairRectTransform.sizeDelta.x,
                crosshaircurrent.CrossHairSize, crosshairUpdateShrpness * Time.deltaTime) * Vector2.one;

        }



        // ���Ⱑ �ٲ� ������ crosshairimage�� ������ ���� ũ�ν���� �̹����� �ٲٱ�
        void OnWeaponChanged(WeaponController newWeapon)
        {
            if(newWeapon)
            {
                crosshairImage.enabled = true;
                crosshairRectTransform = crosshairImage.GetComponent<RectTransform>();
                // ��Ƽ�� ����
                crosshairDefault = newWeapon.crosshairDefault;
                crosshairTarget = newWeapon.crosshairTargetInSight;
               // corsshairImage.sprite = newWeapon.crosshairDefault.CrossHairSprite;

            }
            else
            {
                if((nullCrosshairSprite))
                {
                    crosshairImage.sprite = nullCrosshairSprite;
                }
                else
                {
                    crosshairImage.enabled = false;
                }
               
            }
            UpdateCrosshairPointAtEnemy(true);
        }

    }
        
}
