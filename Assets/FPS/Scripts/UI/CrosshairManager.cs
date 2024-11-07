using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Gameplay;

namespace Unity.FPS.UI
{

    public class CrosshairManager : MonoBehaviour
    {
        #region Variables

        public Image crosshairImage;            // 크로스헤어 UI 이미지
        public Sprite nullCrosshairSprite;      // 액티브한 무기가 없을때

        private RectTransform crosshairRectTransform;
        private CrossHairData crosshairDefault;  // 평상시, 기본시
        private CrossHairData crosshairTarget;   // 타겟팅 되었을때

        private CrossHairData crosshaircurrent;  // 실직적으로 그리는 크로스헤어
        [SerializeField]private float crosshairUpdateShrpness = 5.0f;   // Lerp변수

        private PlayerWeaponsManager weaponsManager;

        private bool wasPointingAtEnemy;

        #endregion

        private void Start()
        {
            // 참조
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>(); // 직접 컨포넌트 가져오기 (씬에 하나 존재하는 가정하에 사용)
            // 액티브한 무기 크로스헤어 보이기 
            OnWeaponChanged(weaponsManager.GetActiveWeapon());

            weaponsManager.OnSwitchToWeapon += OnWeaponChanged;            
        }
        private void Update()
        {
            UpdateCrosshairPointAtEnemy(false);

            wasPointingAtEnemy = weaponsManager.IsPointingAtEnemy;
        }
        // 크로스 헤어 그리기
        void UpdateCrosshairPointAtEnemy(bool force)
        {
            if (crosshairDefault.CrossHairSprite == null)
                return;

            // 평상시?, 타겟팅?
            if((force || wasPointingAtEnemy ==false) && weaponsManager.IsPointingAtEnemy == true) // 적을 포착하는 순간
            {
                crosshaircurrent = crosshairTarget;
                crosshairImage.sprite = crosshaircurrent.CrossHairSprite;
                crosshairRectTransform.sizeDelta = crosshaircurrent.CrossHairSize * Vector2.one;
            }
            else if ((force || wasPointingAtEnemy == true) && weaponsManager.IsPointingAtEnemy == false) // 적을 놓치는 순간
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



        // 무기가 바뀔 때마다 crosshairimage를 각각의 무기 크로스페어 이미지로 바꾸기
        void OnWeaponChanged(WeaponController newWeapon)
        {
            if(newWeapon)
            {
                crosshairImage.enabled = true;
                crosshairRectTransform = crosshairImage.GetComponent<RectTransform>();
                // 액티브 무기
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
