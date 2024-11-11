using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Float 의 Min에서 Max사이의 값을 Lerp값 반환 
    /// </summary>
      [System.Serializable]
      public struct MinMaxFloat
      {
        public float Min;
        public float Max;

        public float GetValueFromRatio(float ratio)
        {
            return Mathf.Lerp(Min, Max,ratio);
        }

        /// <summary>
        ///  ratio 매개변수로 받아 color의 min에서 max사이의 lerp값 반환 
        /// </summary>
       [System.Serializable]
       public struct MinMaxColor
       {
          public Color Min;
          public Color Max;

          public Color GetValueFromRatio(float ratio)
          {
            return Color.Lerp(Min, Max, ratio);
          }
       }

        /// <summary>
        ///  ratio 매개변수로 받아 vector3의 min에서 max사이의 lerp값 반환 
        /// </summary>
        [System.Serializable]
        public struct MinMaxVector3
        {
            public Vector3 Min;
            public Vector3 Max;

            public Vector3 GetValueFromRatio(float ratio)
            {
                return Vector3.Lerp(Min, Max, ratio);
            }

        }



      }



    
}