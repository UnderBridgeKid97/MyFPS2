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
            // ����
            weaponManager = GameObject.FindObjectOfType<PlayerWeaponsManager>();
            // �׼� �Լ� ���
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