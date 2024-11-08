using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    ///  ����� �÷��� ���� ��� ���� 
    /// </summary>

    public class AudioUtility : MonoBehaviour
    {
        // ������ ��ġ�� ���ӿ�����Ʈ ���� �� ����� �ҽ� ���۳�Ʈ �߰� �ؼ� ������ Ŭ�� ���
        // ���� Ŭ�� ����� ������ �ڵ����� ų - TimeSelfDEstruct ������Ʈ �̿�
        public static void CreateSfx(AudioClip clip,Vector3 position, float spartialBlend,
                                                                 float rolloffDistanceMin=1f)
        {
            GameObject impactSfxInstance = new GameObject(); // �� ������Ʈ �����ϱ� 
            impactSfxInstance.transform.position = position; // ��ġ

            // audio clip play
           AudioSource source =  impactSfxInstance.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = spartialBlend;
            source.minDistance = rolloffDistanceMin;
            source.Play();

            // ������Ʈ kill
           TimeSelfDestruct timeSelfDestruct = impactSfxInstance.AddComponent<TimeSelfDestruct>(); // ���� ������Ʈ �߰�
            timeSelfDestruct.lifeTime = clip.length; // Ŭ���� �÷���Ÿ�Ӹ�ŭ ���� �� kill

        }


    }
}