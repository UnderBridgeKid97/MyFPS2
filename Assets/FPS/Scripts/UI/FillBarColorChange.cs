using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    ///  ����������  ������ ��, ��׶���� ���� ����
    /// </summary>
    public class FillBarColorChange : MonoBehaviour
    {
        #region Variables

        public Image foregroundImage;
        public Color defaultForeGroundColor;    // �������� �⺻ �÷�
        public Color flashForeGroundColorFull;  // ������ Ǯ�� ���� ���� �÷��� ȿ�� 

        public Image backGroundImage;
        public Color defaultBackGroundColor;    // ��׶��� �⺻ �÷� 
        public Color flashBackGroundColorEmpty; // ��׶��� ���������� 0�϶� & ��������� �÷��� 

        private float fullValue = 1f;           // �������� Ǯ�϶��� ��
        private float emptyValue = 0f;          // �������� ��������� ��

        private float colorChangeSharpness = 5f; // �÷� ���� �ӵ� ��
        private float prevousValue;             // �������� Ǯ�� ���� ������ ã�� ���� 

        #endregion

        // �� ���� ���� �� �ʱ�ȭ
        public void Initiallize(float fullValueRatio,float emptyValueRatio)
        {
            fullValue= fullValueRatio;
            emptyValue= emptyValueRatio;

            prevousValue = fullValue;
        }

        public void UpdateVisual(float currentRatio)
        {
            // �������� Ǯ�� ���� ���� / ��ũ�� ���� �ʴ� ����
            // currentRatio == fullValue && currentRatio
            if (currentRatio == fullValue && currentRatio != prevousValue)
            {
                foregroundImage.color = flashForeGroundColorFull;
            }
            else if(currentRatio < emptyValue)
            {
                backGroundImage.color = flashBackGroundColorEmpty;
            }

            else
            {
                foregroundImage.color = Color.Lerp(foregroundImage.color, defaultForeGroundColor,
                                                    colorChangeSharpness * Time.deltaTime);
                backGroundImage.color = Color.Lerp(backGroundImage.color, defaultBackGroundColor,
                                                    colorChangeSharpness * Time.deltaTime);
            }


            prevousValue = currentRatio;


        }
    }
}