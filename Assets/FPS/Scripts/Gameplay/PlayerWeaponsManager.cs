using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 
    /// �÷��̾ ���� ����(weaponController)�� �����ϴ� Ŭ���� 
    /// 
    /// </summary>
    
    // ���� ��ü ����
    public enum WeaponSwitchState
    {
        Up, // ���� ��������� true
        Down, // ������ �ٿ� false
        PutDownPrvious, // ���Ⱑ ������ ����
        PutUpNew, // ���� �ö󰥶�
    }

    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region Variavles
        // ���� ���� - ������ �����Ҷ� ó�� �������� ���޵Ǵ� ���� ����Ʈ( �κ��丮 ����)
        public List<WeaponController> startingWeapons = new List<WeaponController>();

        // ��������
        // ���⸦ �����ϴ� ������Ʈ
        public Transform weaponParentSocket;

        // �÷��̾ �����߿� ��� �ٴϴ� ���� ����Ʈ ( �迭)
        private WeaponController[] weaponSlots = new WeaponController[9];
        // ���� ����Ʈ(����)�� Ȱ��ȭ�� ���⸦ �����ϴ� �ε���
        public int ActiveWeaponIndex { get; private set; }

        // ���� ��ü 
        public UnityAction<WeaponController> OnSwitchToWeapon; // ���� ��ü�Ҷ����� ��ϵ� �Լ� ȣ�� 

        // 
        public UnityAction<WeaponController,int> OnAddedWeapon;    // ���� �߰��Ҷ����� ��ϵ� �Լ� ȣ�� 
        public UnityAction<WeaponController,int> OnRemoveWeapon;   // ������ ���Ⱑ ���� �� ������  �Լ�ȣ�� 

        private WeaponSwitchState weaponSwitchState;    // ���� ��ü�� ���� 

        // �ڵ鷯 ����
        private PlayerInputHandler playerInputHandler;

        // ���� ��ü�� ���Ǵ� ������ġ
        public Vector3 weaponMainLocalPosition;

        public Transform defaultWeaponPosition;
        public Transform downWeaponPosition;
        public Transform aimngWeaponPosition;

        private int weaponSwitchNewIndex;           // ���� �ٲ�� ���� �ε��� 

        private float weaponSwitchTimeStarted = 0f;
        [SerializeField]private float weaponSwitchDelay = 1f;

        // �� ����
        public bool IsPointingAtEnemy { get; private set; } // �� ���� ����
        public Camera weaponCamera;                         // weaponCamera���� ray�� �� Ȯ��

        // ����
        // ī�޶� ����
        private PlayerCharacterController playerCharacterController;
        [SerializeField] private float defaultFov = 60f;          // ī�޶� �⺻fov�� 
        [SerializeField] private float weaponFovMultiplayer;      // fov ���� ���


        public bool IsAiming  { get; private set; } // ���� ���� ����
        [SerializeField]
        private float aimngAnimationSpeed = 10f;   // ���� �̵�, fov ���� �ӵ� ,lerp�ӵ�

        // ��鸲
        [SerializeField]private float bobFrequency = 10f;
        [SerializeField]private float bobSharpness = 10f;
        [SerializeField]private float defaultBobAmount = 0.05f;  // ���� ��鸲 �� 
        [SerializeField]private float aimngBobAmount = 0.02f;    // ���� �� ��鸲 ��
        private float weaponBobFactor;                           // ��鸲 ��� 
        private Vector3 lastCharacterPosition;                   // ���� �����ӿ����� �̵��ӵ��� ���ϱ� ���� ����
        private Vector3 weaponBobLocalPosition;                  // �̵��� ��鸲�� ���� ��갪, �̵����������� 0

        // �ݵ�
        [SerializeField]private float recoilSharpness = 50f;    // �ڷ� �и��� �̵� �ӵ� 
        [SerializeField]private float maxRecoilDistance = 0.5f; // �ݵ��� �ڷ� �и� �� �ִ� �ִ� �Ÿ�
        private float recoilRepositionSharpness = 10f;          // ���ڸ��� ���ƿ��� �ӵ� 
        private Vector3 accumulateRecoil;                       // �ݵ��� �ڷ� �и��� �� 
        private Vector3 weaponRecoilLocalPosition;              // �ݵ��� �̵��� ���� ��갪, �ݵ� �� ���ڸ��� ���ƿ��� 0 

        // ���� ���
        private bool isScopeOn = false;
        [SerializeField]private float distanceOnScope = 0.1f;   // ������ ���� ��Ʈ�ѷ��� �ִ°� ����

        public UnityAction OnScopedWeapon;                      // ���ݸ�� ���۽� ��ϵ� �Լ� ȣ��
        public UnityAction OffScopedWeapon;                     // ���ݸ�� ������ ��ϵ� �Լ� ȣ�� 
        #endregion


        private void Start()
        {
            // ����
            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerCharacterController = GetComponent<PlayerCharacterController>();

            // �ʱ�ȭ
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;

            // ��Ƽ�� ���� SHOW�Լ� ���
            OnSwitchToWeapon += OnweaponSwitched;

            // ���� ��� �Լ� ���
            OnScopedWeapon += OnScope;
            OffScopedWeapon += OffScope;

            // Fov �ʱⰪ ���� 
            SetFov(defaultFov);

            //���޹��� ���� ����
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon); // ���� ���� 
            }
            SwitchWeapon(true);

         
        }

        private void Update()
        {
            // ���� ��Ƽ�� ����
            WeaponController activeWeapon = GetActiveWeapon();

            if(weaponSwitchState == WeaponSwitchState.Up) // ���Ⱑ ���� �ö�������� �� &�� �����ϰ�
            {

                // ���� �Է°� ó�� 
                IsAiming = playerInputHandler.GetAimInputHeld();

                // ���� ��� ó�� 
                if(activeWeapon.shootType == WeaponShootType.Snipe)
                {
                    if(playerInputHandler.GetAimInputDown())
                    {
                        // ���� ��� ����
                        isScopeOn = true;
                      //  OnScopedWeapon?.Invoke();
                    }
                    if(playerInputHandler.GetAimInputUp())
                    {
                        // ���� ��� ����
                        OffScopedWeapon?.Invoke();

                    }
                }


                //��
               bool isFire = activeWeapon.HandleShootInputs(
                    playerInputHandler.GetFireInputDown(),
                    playerInputHandler.GetFireInputHeld(),
                    playerInputHandler.GetFireInputUp());
              
                if(isFire) //isfire�� Ʈ��� �߻�ȰŰ� ���⼭ �ݵ�ȿ�� �߻�
                {
                    // �ݵ� ȿ��
                    accumulateRecoil += Vector3.back /* 0,0,-1*/* activeWeapon.recoilForce;
                    accumulateRecoil = Vector3.ClampMagnitude(accumulateRecoil, maxRecoilDistance); // Vector3.ClampMagnitude : ���Ͱ��� clamp�� �����
                }
            }


            if(!IsAiming && (weaponSwitchState == WeaponSwitchState.Up || weaponSwitchState == WeaponSwitchState.Down))
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;      // ���� ���߿��� �Է��ص� �ȹٲ�
                    SwitchWeapon(switchUp);
                }
            }

            // �� ����
            IsPointingAtEnemy = false;
            if(activeWeapon) // ��Ƽ�� ������ ���̸� ���Ⱑ �����ϱ� ���� �ƴҶ���
            {
                RaycastHit hit;
                if(Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward,out hit,300))
                {
                    // �ݶ��̴� üũ - ��(Damageable) �Ǻ�
                    Damageable damageable = hit.collider.GetComponent<Damageable>();
                    if(damageable) // �ｺ�� ���� ��
                    {
                        IsPointingAtEnemy = true;
                    }
                }
            }
         
        }
      
        private void LateUpdate()
        {
            UpdateWeaponBob();
            UpdateWeaponRecoil();
            UpdateWeaponAiming();
            UpdateWeaponSwitching();

         // ���� ��� ���� ���� ���� ���������� �Ҵ� => ���� ������ġ 
         weaponParentSocket.localPosition = weaponMainLocalPosition + weaponBobLocalPosition + weaponRecoilLocalPosition;

        }

        // �ݵ�
        void UpdateWeaponRecoil()
        {
            //     �и��� ��ġ                    �и��� ��(-0.99)(z = -1) => ���� �з��� ����-1���� �и���
            if(weaponRecoilLocalPosition.z >= accumulateRecoil.z * 0.99f)
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, accumulateRecoil,
                                                                   recoilSharpness * Time.deltaTime);
            }
            else // �з��� -1���� �����ϸ� ���ڸ��� ���ư���( z = 0)
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, Vector3.zero,
                                                      recoilRepositionSharpness * Time.deltaTime);
                accumulateRecoil = weaponRecoilLocalPosition; // �ڷ� �и��� ���� z=0��ġ�� ���� �ȱ׷� ������ ������ if�� ��� �ɷ��� 
            }

        }

        // ī�޶�fov�� ���� : ����, �ܾƿ�
        private void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMultiplayer;
        }
        
        // ���� ���ؿ� ���� ����, ���� ��ġ ����, fov�� ���� 
        void UpdateWeaponAiming()
        {
            //���⸦ ��� �������� ���� ����
            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                WeaponController activeWeapon = GetActiveWeapon();

                if (IsAiming && activeWeapon)  // ���ؽ� : ����Ʈ -> aming ��ġ�� �̵�, fov ����Ʈ -> aimZoomRatio
                {

                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                        aimngWeaponPosition.localPosition + activeWeapon.aimOffset
                        , aimngAnimationSpeed * Time.deltaTime);

                    // ���� ��� ����
                    if(isScopeOn)
                    {
                        // weaponMainLocalPosition, ��ǥ���������� �Ÿ��� ���Ѵ�
                        float dist = Vector3.Distance(weaponMainLocalPosition, aimngWeaponPosition.localPosition + activeWeapon.aimOffset);
                        if(dist <distanceOnScope)
                        {
                            OnScopedWeapon?.Invoke();
                            isScopeOn = false;
                        }
                    }
                    else
                    {
                        float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                       activeWeapon.aimZoomRatio * defaultFov, aimngAnimationSpeed * Time.deltaTime);
                        SetFov(fov);
                    }

                }
                else         // ������ Ǯ������ : aming ��ġ -> ����Ʈ�� �̵� fov :aimzoomratio -> 
                {
                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                        defaultWeaponPosition.localPosition,
                        aimngAnimationSpeed * Time.deltaTime);

                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView,
                       defaultFov, aimngAnimationSpeed * Time.deltaTime);
                    SetFov(fov);
                }

            }
        }

        // �̵��� ���� ���� ��鸲 �� ���ϱ�
        void UpdateWeaponBob()
        {
            // �������� ����
            if(Time.deltaTime >0)
            {
                // �÷��̾ �� �����ӵ��� �̵��� �Ÿ�
                // playerCharacterController.transform.position -lastCharacterPosition
                // ���� �����ӿ��� �÷��̾� �̵��ӵ�
                Vector3 playerCharacterVelocity = 
                    (playerCharacterController.transform.position-lastCharacterPosition)/Time.deltaTime;

                float characterMovementFctor = 0f;
                if(playerCharacterController.IsGrounded)
                {
                    characterMovementFctor = Mathf.Clamp01(playerCharacterVelocity.magnitude /
                    (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }

                // �ӵ��� ���� ��鸲 ��� 
                weaponBobFactor = Mathf.Lerp(weaponBobFactor, characterMovementFctor, bobSharpness * Time.deltaTime);

                // ��鸲�� (���ؽ�, ����) 
                float bobAmount =IsAiming ? aimngBobAmount :  defaultBobAmount;
                float frequency = bobFrequency;
                // �¿� ��鸲 
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;  // ���� Ŀ������ ������ ��鸲 
                // �� �Ʒ� ��鸲(�¿� ��鸲�� ����)
                float vBobValue = ((Mathf.Sin(Time.time * frequency) * 0.5f)+0.5f )* bobAmount * weaponBobFactor;

                // ��鸲 ���� ������ ����
                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y =Mathf.Abs(vBobValue);


                // �÷��̾��� ���� �������� ������ ��ġ�� ����
                lastCharacterPosition = playerCharacterController.transform.position;
            }
        }



        // ���¿� ���� ���� ����
        void UpdateWeaponSwitching()
        {
            // Lerp �ð�
            float switchingTimeFactor = 0f;
            if(weaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01(Time.time - weaponSwitchTimeStarted / weaponSwitchDelay);
            }

            // �����ð� ���� ���� ���� �ٲٱ�
            if(switchingTimeFactor >=1f)
            {
                if(weaponSwitchState == WeaponSwitchState.PutDownPrvious)
                {
                    // ���� ���� false, ���ο� ���� true
                    WeaponController oldWeapon = GetActiveWeapon();
                    if (oldWeapon !=null)
                    {
                        oldWeapon.ShowWeapon(false);
                    }

                    ActiveWeaponIndex = weaponSwitchNewIndex;
                    WeaponController newWeapon = GetActiveWeapon();
                    OnSwitchToWeapon?.Invoke(newWeapon);

                    switchingTimeFactor = 0f;
                    if(newWeapon != null)
                    {
                        weaponSwitchTimeStarted = Time.time;
                        weaponSwitchState = WeaponSwitchState.PutUpNew;
                    }
                    else
                    {
                        weaponSwitchState = WeaponSwitchState.Down;
                    }

                }
                else if(weaponSwitchState == WeaponSwitchState.PutUpNew)
                {
                    weaponSwitchState = WeaponSwitchState.Up;
                }

            }
            // �����ð����� ������ ��ġ �̵�
            if (weaponSwitchState == WeaponSwitchState.PutDownPrvious)
            {
                weaponMainLocalPosition = Vector3.Lerp(defaultWeaponPosition.localPosition, downWeaponPosition.localPosition, switchingTimeFactor);
            }
            else if (weaponSwitchState == WeaponSwitchState.PutUpNew)
            {
              weaponMainLocalPosition = Vector3.Lerp(downWeaponPosition.localPosition,defaultWeaponPosition.localPosition, switchingTimeFactor);
            }
        }

        // weaponSlot�� ���� ���������� ������ WeaponController ������Ʈ �߰� 
        public bool AddWeapon(WeaponController weaponPrefab)
        {
            // �߰��ϴ� ���� ���� ���� üũ - �ߺ��˻�
            if (HasWeapon(weaponPrefab) != null)
            {
                Debug.Log("Has same Weapon");
                return false;
            }

            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null)
                {
                    WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket); // ��Ĺ�� �ڽ����� ����
                    weaponInstance.transform.localPosition = Vector3.zero; // �� ��ġ��
                    weaponInstance.transform.localRotation = Quaternion.identity;

                    weaponInstance.Owner = this.gameObject;
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                    weaponInstance.ShowWeapon(false);

                    // ��������
                    OnAddedWeapon.Invoke(weaponInstance,i);

                    weaponSlots[i] = weaponInstance;  // null�̸� �󽽷Կ� �� 
                    return true;
                }
            }
            Debug.Log("weaponSlots full!");
            return false;
        }
        // WeaponSlots�� �������� ���� ����
        public bool RemoveWeapon(WeaponController oldWeapon)
        {
            for(int i = 0; i < weaponSlots.Length; i ++)
            {
                // ���� ���� ã��
                if (weaponSlots[i] == oldWeapon)
                {
                    // ����
                    weaponSlots[i] = null;

                    OnRemoveWeapon?.Invoke(oldWeapon, i);

                    Destroy(oldWeapon.gameObject);

                    if(i == ActiveWeaponIndex)
                    {
                        SwitchWeapon(true);
                    }
                    return true;

                }    
            }
            return false;
        }



        // �Ű������� ���� ���������� ���� ���Ⱑ �ִ��� üũ 
        private WeaponController HasWeapon(WeaponController weaponPrefab)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null && weaponSlots[i].SourcePrefab == weaponPrefab)
                {
                    return weaponSlots[i];
                }

            }
            return null;
        }

        // ���� Ȱ��ȭ�� ����
        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }

        // ������ ���Կ� ���Ⱑ �ִ��� ���� üũ
        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            if (index >= 0 && index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }
            return null;
        }

        // 0~9  
        // ���� �ٲٱ�, ���� ��� �ִ� ���� false, ���ο� ���� true
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1;        // ���� ��Ƽ�� �� ���� �ε��� 
            int closestSlotDistance = weaponSlots.Length;
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlot(ActiveWeaponIndex, i, ascendingOrder);
                    if (distanceToActiveIndex < closestSlotDistance)
                    {
                        closestSlotDistance = distanceToActiveIndex;

                        newWeaponIndex = i;
                    }
                }
            }
            // ���� ��Ƽ���� ���� �ε����� ���� ��ü
            SwitchToWeaponIndex(newWeaponIndex);
        }

        // ���� ��Ƽ���� ���� �ε����� ���� ��ü
        private void SwitchToWeaponIndex(int newWeaponIndex)
        {
            // newWeaponIndex �� üũ 
            if(newWeaponIndex >=0 && newWeaponIndex != ActiveWeaponIndex)
            {
                #region
                /* if (ActiveWeaponIndex >= 0)
                 {
                     WeaponController nowWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
                     nowWeapon.ShowWeapon(false);
                }
                 WeaponController newWeapon = GetWeaponAtSlotIndex(newWeaponIndex);
                 newWeapon.ShowWeapon(true);
                 ActiveWeaponIndex = newWeaponIndex;*/
                #endregion
                weaponSwitchNewIndex = newWeaponIndex;
                weaponSwitchTimeStarted = Time.time;

                // ���� ��Ƽ���� ���Ⱑ �ִ°�?
                if(GetActiveWeapon()==null)
                {
                    weaponMainLocalPosition = downWeaponPosition.position;
                    weaponSwitchState = WeaponSwitchState.PutUpNew;
                    ActiveWeaponIndex = newWeaponIndex;

                    WeaponController weaponController = GetWeaponAtSlotIndex(newWeaponIndex);
                    OnSwitchToWeapon?.Invoke(weaponController);
                }
                else
                {
                    weaponSwitchState = WeaponSwitchState.PutDownPrvious; // ��Ƽ���� ���������� ����
                }
            }
        }

        // ���԰� �Ÿ�
        private int GetDistanceBetweenWeaponSlot(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlot = 0;

            if(ascendingOrder)
            {
                distanceBetweenSlot = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlot = fromSlotIndex - toSlotIndex;
            }

            if(distanceBetweenSlot < 0)
            {
                distanceBetweenSlot = distanceBetweenSlot + weaponSlots.Length;
            }
          
            return distanceBetweenSlot;
        }

        void OnweaponSwitched(WeaponController newWeapon)
        {
            if(newWeapon != null)
            {
                newWeapon.ShowWeapon(true);
            }
        }

        void OnScope()
        {
            weaponCamera.enabled = false;
        }
        void OffScope()
        {
            weaponCamera.enabled = true ;
        }

    }
}