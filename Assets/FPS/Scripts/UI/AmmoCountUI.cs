using UnityEngine;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;
using TMPro;
using UnityEngine.UI;

namespace Unity.FPS.UI
{/// <summary>
///  WeaponConTroller ������ Ammo ī��Ʈ UI
/// </summary>
    public class AmmoCountUI : MonoBehaviour
    {
        #region Variables 

        private PlayerWeaponsManager playerWeaponsManager;

        private WeaponController weaponController;
        private int weaponIndex;

        // UI
        public TextMeshProUGUI weaponIndexText;

        public Image ammoFillImage;     // ammo rate�� ���� ������

        [SerializeField]private float ammoFillSharpness = 10f;      // ������ ä��ų� �ٴ� �ӵ� 
        private float weaponSwitchSharpness = 10f;  // ���� ��ü�� UI�� �ٲ�� �ӵ� 

        public CanvasGroup canvasGroup;
        [SerializeField][Range(0,1)]private float unSelectedOpacity = 0.5f;
        private Vector3 unSelectedScale = Vector3.one * 0.8f;

        // �������� �� ���� 
        public FillBarColorChange fillBarColorChange;
        #endregion

        // AmmoCount UI ��  �ʱ�ȭ 
       public void Initialzie(WeaponController weapon, int _weaponIndex)
        {
          weaponController = weapon; 
          weaponIndex = _weaponIndex; 

          // ���� �ε���
          weaponIndexText.text = (weaponIndex +1).ToString(); // 0,1,2���̶� 1 2 3���� ���Ϸ��� +1

            // ������ �� �� �ʱ�ȭ
            fillBarColorChange.Initiallize(1f, 0.1f);


           // ����  
           playerWeaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
        }

        private void Update()
        {
            // ä��� �ӵ�
            float currentFillRate = weaponController.CurrentAmmoRatio;
            ammoFillImage.fillAmount = Mathf.Lerp(ammoFillImage.fillAmount,
                                                    currentFillRate , ammoFillSharpness * Time.deltaTime);

            // ��Ƽ�� ���� ���� - ���İ� �ٲ�� �ӵ�
            bool isActiveWeapon = weaponController == playerWeaponsManager.GetActiveWeapon();
            float currentOpacity = isActiveWeapon ? 1.0f : unSelectedOpacity;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, currentOpacity, 
                                        weaponSwitchSharpness * Time.deltaTime);

            Vector3 currentScale = isActiveWeapon ? Vector3.one : unSelectedScale;
            transform.localScale = Vector3.Lerp(transform.localScale, currentScale,
                                                weaponSwitchSharpness * Time.deltaTime);

            // ���� ����
            fillBarColorChange.UpdateVisual(currentFillRate);



        }


    }
}