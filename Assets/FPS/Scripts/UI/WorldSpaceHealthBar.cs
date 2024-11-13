using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WorldSpaceHealthBar : MonoBehaviour
    {
        #region

        public Health health;
        public Image healthBarImage;

        public Transform healthBarPivot;

        // hp�� Ǯ�̸� healthbar�� �����
        [SerializeField]private bool hideFullHealthBar = true;
        #endregion

        private void Update()
        {
            healthBarImage.fillAmount = health.GetRatio();

            // UI�� �÷��̾ �ٶ󺸵��� �Ѵ�

            healthBarPivot.LookAt(Camera.main.transform.position);

            // hp�� Ǯ�̸� healthbar�� �����
            if(hideFullHealthBar )
            {
                healthBarPivot.gameObject.SetActive(healthBarImage.fillAmount != 1f);
            }
        }


    }
}