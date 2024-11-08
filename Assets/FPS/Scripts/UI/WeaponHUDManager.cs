using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        #region Variables

        public RectTransform ammopanel;         // ammocountUI �θ� ������Ʈ�� Ʈ������
        public GameObject ammoCountPrefab;      // ammocountUI ������

        private PlayerWeaponsManager weaponsManager;

        #endregion

        private void Awake()
        {
            // ����
            weaponsManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            // 


            weaponsManager.OnAddedWeapon += AddWeapon;
            weaponsManager.OnRemoveWeapon += RemoveWeapon;
        }

        // �����߰��ϸ� ammo ui�ϳ� �߰�
        void AddWeapon(WeaponController newWeapon, int weaponIndex)
        {
          GameObject ammoCountGo =  Instantiate(ammoCountPrefab, ammopanel);
          AmmoCountUI ammoCountUI = ammoCountGo.GetComponent<AmmoCountUI>();
            ammoCountUI.Initialzie(newWeapon, weaponIndex);
        }

        // ���������ϸ� ammo ui�ϳ� ����
        void RemoveWeapon(WeaponController oldWeapon, int weaponIndex)
        {

        }

        //
        void SwitchWeapon(WeaponController weapon)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ammopanel); // ui ������ 
        }
    }
}