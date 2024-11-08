using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    /// <summary>
    ///  게이지바의  게이지 색, 백그라운드색 변경 구현
    /// </summary>
    public class FillBarColorChange : MonoBehaviour
    {
        #region Variables

        public Image foregroundImage;
        public Color defaultForeGroundColor;    // 게이지의 기본 컬러
        public Color flashForeGroundColorFull;  // 게이지 풀로 차는 순간 플래시 효과 

        public Image backGroundImage;
        public Color defaultBackGroundColor;    // 백그라운드 기본 컬러 
        public Color flashBackGroundColorEmpty; // 백그라운드 게이지값이 0일때 & 비어있을때 컬러값 

        private float fullValue = 1f;           // 게이지가 풀일때의 값
        private float emptyValue = 0f;          // 게이지가 비었을때의 값

        private float colorChangeSharpness = 5f; // 컬러 변경 속도 값
        private float prevousValue;             // 게이지가 풀로 차는 순간을 찾는 변수 

        #endregion

        // 색 변경 관련 값 초기화
        public void Initiallize(float fullValueRatio,float emptyValueRatio)
        {
            fullValue= fullValueRatio;
            emptyValue= emptyValueRatio;

            prevousValue = fullValue;
        }

        public void UpdateVisual(float currentRatio)
        {
            // 게이지가 풀로 차는 순간 / 싱크가 맞지 않는 순간
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