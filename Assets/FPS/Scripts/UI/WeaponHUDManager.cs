using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        #region Variables

        public RectTransform ammopanel;         // ammocountUI 부모 오브젝트의 트랜스폼
        public GameObject ammoCountPrefab;      // ammocountUI 프리팹

        private PlayerWeaponsManager weaponsManager;

        #endregion

        private void Awake()
        {
            // 참조
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            // 


            weaponsManager.OnAddedWeapon += AddWeapon;
            weaponsManager.OnRemoveWeapon += RemoveWeapon;
        }

        // 무기추가하면 ammo ui하나 추가
        void AddWeapon(WeaponController newWeapon, int weaponIndex)
        {
          GameObject ammoCountGo =  Instantiate(ammoCountPrefab, ammopanel);
          AmmoCountUI ammoCountUI = ammoCountGo.GetComponent<AmmoCountUI>();
            ammoCountUI.Initialzie(newWeapon, weaponIndex);
        }

        // 무기제거하면 ammo ui하나 제거
        void RemoveWeapon(WeaponController oldWeapon, int weaponIndex)
        {

        }

        //
        void SwitchWeapon(WeaponController weapon)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ammopanel); // ui 재정렬 
        }
    }
}