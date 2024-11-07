using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class ScopeUI : MonoBehaviour
    {
        #region Variables

        public GameObject scopeUI;

        private PlayerWeaponsManager weaponManager;
        #endregion

        private void Start()
        {
            // 참조
            weaponManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            // 액션 함수 등록
            weaponManager.OnScopedWeapon += OnScope;
            weaponManager.OffScopedWeapon += OffScope;
        }


        public void OnScope()
        {
            scopeUI.SetActive(true);
        }

        public void OffScope()
        {
            scopeUI.SetActive(false);
        }

    }
}