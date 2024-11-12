using UnityEngine;
using Unity.FPS.Game;
using static Unity.FPS.Game.MinMaxFloat;
using UnityEngine.Rendering;

namespace Unity.FPS.Gameplay
{
    public class ChargeWeapon : MonoBehaviour
    {
        #region Variables

        public GameObject chargingObject;               // �����ϴ� �߻�ü
        public GameObject spiningFrame;                // �߻�ü�� ���ΰ� �ִ� ������
        public GameObject distOrbitPartclePrefab;       // �߻�ü�� ���ΰ� �ִ� ȸ���ϴ� ����Ʈ 

        public MinMaxVector3 scale;                     // �߻�ü ũ�� ������

        [SerializeField] private Vector3 offset;
        public Transform parentTransform;

        public MinMaxFloat orbitY;                      // ����Ʈ ������
        public MinMaxVector3 radius;                    // ����Ʈ ������

        public MinMaxFloat spiningSpeed;                // ȸ�� ������

        // sfx
        public AudioClip chargeSound;
        public AudioClip loopChargeWeaponSFX;

        private float fadeLoopDuration = 0.5f;
        [SerializeField]public bool useProceduralPitchOnLoop;

        public float maxProceduralPitchValue = 2.0f;

        private AudioSource audioSource;
        private AudioSource audioSourceLoop;
        //
        public GameObject particleInstance {  get; private set; }
        private ParticleSystem diskOrbitParticle;
        private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule;

        private WeaponController weaponController;  // ���� 

        private float lastChargeTriggerTimeStamp;
        private float endChargeTime;
        private float chargeRatio;                  // ���� ������
        #endregion

        private void Awake()
        {
            // chargeSound play
           audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = chargeSound;
            audioSource.playOnAwake = false;

            // loopChargeWeaponSFX play
            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceLoop.clip = loopChargeWeaponSFX;
            audioSourceLoop.playOnAwake = false;
            audioSourceLoop.loop = true;
            

        }

        void SpawnParticleSystem()
        {
           particleInstance = Instantiate(distOrbitPartclePrefab, parentTransform != null ? parentTransform : transform);
            particleInstance.transform.localPosition += offset;


            FindRefernce();
        }
        void FindRefernce()
        {
            diskOrbitParticle = particleInstance.GetComponent<ParticleSystem>();
            velocityOverLifetimeModule = diskOrbitParticle.velocityOverLifetime;

            weaponController = GetComponent<WeaponController>();
        }

        private void Update()
        {
            if(particleInstance ==null)
            {
                // �ѹ��� ��ü ����� 
                SpawnParticleSystem();
            }

            diskOrbitParticle.gameObject.SetActive(weaponController.IsweaponActive);
            chargeRatio = weaponController.CurrentCharge;

            // vfx
            // disk.frame
            chargingObject.transform.localScale = scale.GetValueFromRatio(chargeRatio);
            if(spiningFrame)
            {
                spiningFrame.transform.localRotation *= Quaternion.Euler(0f,
                    spiningSpeed.GetValueFromRatio(chargeRatio)*Time.deltaTime,
                    0f);
            }

            // particle
            velocityOverLifetimeModule.orbitalY = orbitY.GetValueFromRatio(chargeRatio);
            diskOrbitParticle.transform.localScale = radius.GetValueFromRatio(chargeRatio);

            // sfx
            if (chargeRatio > 0f)
            {
               if(audioSourceLoop.isPlaying == false && 
                   weaponController.lastChargeTriggerTimeStamp > lastChargeTriggerTimeStamp)
                {
                    lastChargeTriggerTimeStamp = weaponController.lastChargeTriggerTimeStamp;
                    if(useProceduralPitchOnLoop == false)
                    {
                        endChargeTime = Time.time + chargeSound.length;
                           audioSource.Play();
                    }
                    audioSourceLoop.Play();
                }

            }
            else
            {
                audioSource.Stop();
                audioSourceLoop.Stop();
            }
            if(useProceduralPitchOnLoop == false) // �ΰ��� ������ ���̵� ȿ��
            {
                float volumeRtio = Mathf.Clamp01((endChargeTime - Time.time - fadeLoopDuration)/ fadeLoopDuration);
                audioSource.volume = volumeRtio;
                audioSourceLoop.volume = 1f -volumeRtio;
            }
            else // ���������� ���Ȥ���� ����ȿ ǥ�� 
            {
                audioSourceLoop.pitch = Mathf.Lerp(1.0f, maxProceduralPitchValue, chargeRatio);
            }
        }


    }
}