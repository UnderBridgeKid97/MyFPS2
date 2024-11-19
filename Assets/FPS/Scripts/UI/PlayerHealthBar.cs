using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Gameplay;

namespace Unity.FPS.UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        #region Variables

        private Health playerHealth;
        public Image healthFillImage;
        #endregion

        private void Start()
        {
            // ÂüÁ¶ 
          PlayerCharacterController playerCharacterController =
                GameObject.FindObjectOfType<PlayerCharacterController>();
            playerHealth = playerCharacterController.GetComponent<Health>();
        }

        private void Update()
        {
            healthFillImage.fillAmount = playerHealth.GetRatio();    
        }
    }
}