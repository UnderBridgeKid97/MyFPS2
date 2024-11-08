using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    ///  오디오 플레이 관련 기능 구현 
    /// </summary>

    public class AudioUtility : MonoBehaviour
    {
        // 지정된 위치에 게임오브젝트 생성 후 오디오 소스 컴퍼넌트 추가 해서 지정된 클립 재생
        // 사운드 클립 재생이 끝나면 자동으로 킬 - TimeSelfDEstruct 컴포넌트 이용
        public static void CreateSfx(AudioClip clip,Vector3 position, float spartialBlend,
                                                                 float rolloffDistanceMin=1f)
        {
            GameObject impactSfxInstance = new GameObject(); // 빈 오브젝트 생성하기 
            impactSfxInstance.transform.position = position; // 위치

            // audio clip play
           AudioSource source =  impactSfxInstance.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spartialBlend;
            source.minDistance = rolloffDistanceMin;
            source.Play();

            // 오브젝트 kill
           TimeSelfDestruct timeSelfDestruct = impactSfxInstance.AddComponent<TimeSelfDestruct>(); // 자폭 컴포넌트 추가
            timeSelfDestruct.lifeTime = clip.length; // 클립의 플레이타임만큼 생존 후 kill

        }


    }
}