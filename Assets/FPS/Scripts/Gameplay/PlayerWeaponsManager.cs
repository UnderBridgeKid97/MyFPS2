using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// 
    /// 플레이어가 가진 무기(weaponController)를 관리하는 클래스 
    /// 
    /// </summary>
    
    // 무기 교체 상태
    public enum WeaponSwitchState
    {
        Up, // 무기 들고있을때 true
        Down, // 완전히 다운 false
        PutDownPrvious, // 무기가 내려간 상태
        PutUpNew, // 무기 올라갈때
    }

    public class PlayerWeaponsManager : MonoBehaviour
    {
        #region Variavles
        // 무기 지급 - 게임을 시작할때 처음 유저에게 지급되는 무기 리스트( 인벤토리 개념)
        public List<WeaponController> startingWeapons = new List<WeaponController>();

        // 무기장착
        // 무기를 장착하는 오브젝트
        public Transform weaponParentSocket;

        // 플레이어가 게임중에 들고 다니는 무기 리스트 ( 배열)
        private WeaponController[] weaponSlots = new WeaponController[9];
        // 무기 리스트(슬롯)을 활성화된 무기를 관리하는 인덱스
        public int ActiveWeaponIndex { get; private set; }

        // 무기 교체 
        public UnityAction<WeaponController> OnSwitchToWeapon; // 무기 교체할때마다 등록된 함수 호출 

        // 
        public UnityAction<WeaponController,int> OnAddedWeapon;    // 무기 추가할때마다 등록된 함수 호출 
        public UnityAction<WeaponController,int> OnRemoveWeapon;   // 장착된 무기가 제거 될 때마다  함수호출 

        private WeaponSwitchState weaponSwitchState;    // 무기 교체시 상태 

        // 핸들러 참조
        private PlayerInputHandler playerInputHandler;

        // 무기 교체시 계산되는 최종위치
        public Vector3 weaponMainLocalPosition;

        public Transform defaultWeaponPosition;
        public Transform downWeaponPosition;
        public Transform aimngWeaponPosition;

        private int weaponSwitchNewIndex;           // 새로 바뀌는 무기 인덱스 

        private float weaponSwitchTimeStarted = 0f;
        [SerializeField]private float weaponSwitchDelay = 1f;

        // 적 포착
        public bool IsPointingAtEnemy { get; private set; } // 적 포착 여부
        public Camera weaponCamera;                         // weaponCamera에서 ray로 적 확인

        // 조준
        // 카메라 세팅
        private PlayerCharacterController playerCharacterController;
        [SerializeField] private float defaultFov = 60f;          // 카메라 기본fov값 
        [SerializeField] private float weaponFovMultiplayer;      // fov 연산 계수


        public bool IsAiming  { get; private set; } // 무기 조준 여부
        [SerializeField]
        private float aimngAnimationSpeed = 10f;   // 무기 이동, fov 연출 속도 ,lerp속도

        // 흔들림
        [SerializeField]private float bobFrequency = 10f;
        [SerializeField]private float bobSharpness = 10f;
        [SerializeField]private float defaultBobAmount = 0.05f;  // 평상시 흔들림 량 
        [SerializeField]private float aimngBobAmount = 0.02f;    // 조준 중 흐들림 량
        private float weaponBobFactor;                           // 흔들림 계수 
        private Vector3 lastCharacterPosition;                   // 현재 프레임에서의 이동속도를 구하기 위한 변수
        private Vector3 weaponBobLocalPosition;                  // 이동시 흔들림량 최종 계산값, 이동하지않으면 0

        // 반동
        [SerializeField]private float recoilSharpness = 50f;    // 뒤로 밀리는 이동 속도 
        [SerializeField]private float maxRecoilDistance = 0.5f; // 반동시 뒤로 밀릴 수 있는 최대 거리
        private float recoilRepositionSharpness = 10f;          // 제자리로 돌아오는 속도 
        private Vector3 accumulateRecoil;                       // 반동시 뒤로 밀리는 양 
        private Vector3 weaponRecoilLocalPosition;              // 반동시 이동한 최종 계산값, 반동 후 제자리에 돌아오면 0 

        // 저격 모드
        private bool isScopeOn = false;
        [SerializeField]private float distanceOnScope = 0.1f;   // 원래는 웨폰 컨트롤러에 있는게 좋음

        public UnityAction OnScopedWeapon;                      // 저격모드 시작시 등록된 함수 호출
        public UnityAction OffScopedWeapon;                     // 저격모드 해제시 등록된 함수 호출 
        #endregion


        private void Start()
        {
            // 참조
            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerCharacterController = GetComponent<PlayerCharacterController>();

            // 초기화
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;

            // 액티브 무기 SHOW함수 등록
            OnSwitchToWeapon += OnweaponSwitched;

            // 저격 모드 함수 등록
            OnScopedWeapon += OnScope;
            OffScopedWeapon += OffScope;

            // Fov 초기값 설정 
            SetFov(defaultFov);

            //지급받은 무기 장착
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon); // 무기 생성 
            }
            SwitchWeapon(true);

         
        }

        private void Update()
        {
            // 현재 액티브 무기
            WeaponController activeWeapon = GetActiveWeapon();

            if(weaponSwitchState == WeaponSwitchState.Up) // 무기가 위로 올라와잇을떄 줌 &슛 가능하게
            {

                // 조준 입력값 처리 
                IsAiming = playerInputHandler.GetAimInputHeld();

                // 저격 모드 처리 
                if(activeWeapon.shootType == WeaponShootType.Snipe)
                {
                    if(playerInputHandler.GetAimInputDown())
                    {
                        // 저격 모드 시작
                        isScopeOn = true;
                      //  OnScopedWeapon?.Invoke();
                    }
                    if(playerInputHandler.GetAimInputUp())
                    {
                        // 저격 모드 해제
                        OffScopedWeapon?.Invoke();

                    }
                }


                //슛
               bool isFire = activeWeapon.HandleShootInputs(
                    playerInputHandler.GetFireInputDown(),
                    playerInputHandler.GetFireInputHeld(),
                    playerInputHandler.GetFireInputUp());
              
                if(isFire) //isfire가 트루면 발사된거고 여기서 반동효과 발생
                {
                    // 반동 효과
                    accumulateRecoil += Vector3.back /* 0,0,-1*/* activeWeapon.recoilForce;
                    accumulateRecoil = Vector3.ClampMagnitude(accumulateRecoil, maxRecoilDistance); // Vector3.ClampMagnitude : 벡터값을 clamp로 찍어줌
                }
            }


            if(!IsAiming && (weaponSwitchState == WeaponSwitchState.Up || weaponSwitchState == WeaponSwitchState.Down))
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;      // 연출 도중에는 입력해도 안바뀜
                    SwitchWeapon(switchUp);
                }
            }

            // 적 포착
            IsPointingAtEnemy = false;
            if(activeWeapon) // 액티브 웨폰이 널이면 무기가 없으니까 널이 아닐때만
            {
                RaycastHit hit;
                if(Physics.Raycast(weaponCamera.transform.position, weaponCamera.transform.forward,out hit,300))
                {
                    // 콜라이더 체크 - 적(Damageable) 판별
                    Damageable damageable = hit.collider.GetComponent<Damageable>();
                    if(damageable) // 헬스를 가진 적
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

         // 최종 계산 값을 소켓 로컬 포지션으로 할당 => 무기 최종위치 
         weaponParentSocket.localPosition = weaponMainLocalPosition + weaponBobLocalPosition + weaponRecoilLocalPosition;

        }

        // 반동
        void UpdateWeaponRecoil()
        {
            //     밀리는 위치                    밀리는 양(-0.99)(z = -1) => 총이 밀려서 거의-1까지 밀리면
            if(weaponRecoilLocalPosition.z >= accumulateRecoil.z * 0.99f)
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, accumulateRecoil,
                                                                   recoilSharpness * Time.deltaTime);
            }
            else // 밀려서 -1까지 도착하면 제자리로 돌아가기( z = 0)
            {
                weaponRecoilLocalPosition = Vector3.Lerp(weaponRecoilLocalPosition, Vector3.zero,
                                                      recoilRepositionSharpness * Time.deltaTime);
                accumulateRecoil = weaponRecoilLocalPosition; // 뒤로 밀리는 양을 z=0위치로 맞춤 안그럼 앞으로 못나감 if에 계속 걸려서 
            }

        }

        // 카메라fov값 세팅 : 줌인, 줌아웃
        private void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCamera.fieldOfView = fov * weaponFovMultiplayer;
        }
        
        // 무기 조준에 따른 연출, 무기 위치 조정, fov값 조정 
        void UpdateWeaponAiming()
        {
            //무기를 들고 있을때만 조준 가능
            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                WeaponController activeWeapon = GetActiveWeapon();

                if (IsAiming && activeWeapon)  // 조준시 : 디폴트 -> aming 위치로 이동, fov 디폴트 -> aimZoomRatio
                {

                    weaponMainLocalPosition = Vector3.Lerp(weaponMainLocalPosition,
                        aimngWeaponPosition.localPosition + activeWeapon.aimOffset
                        , aimngAnimationSpeed * Time.deltaTime);

                    // 저격 모드 시작
                    if(isScopeOn)
                    {
                        // weaponMainLocalPosition, 목표지점까지의 거리를 구한다
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
                else         // 조준이 풀렸을때 : aming 위치 -> 디폴트로 이동 fov :aimzoomratio -> 
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

        // 이동에 의한 무기 흔들림 값 구하기
        void UpdateWeaponBob()
        {
            // 프레임이 돌면
            if(Time.deltaTime >0)
            {
                // 플레이어가 한 프레임동안 이동한 거리
                // playerCharacterController.transform.position -lastCharacterPosition
                // 현재 프레임에서 플레이어 이동속도
                Vector3 playerCharacterVelocity = 
                    (playerCharacterController.transform.position-lastCharacterPosition)/Time.deltaTime;

                float characterMovementFctor = 0f;
                if(playerCharacterController.IsGrounded)
                {
                    characterMovementFctor = Mathf.Clamp01(playerCharacterVelocity.magnitude /
                    (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }

                // 속도에 의한 흔들림 계수 
                weaponBobFactor = Mathf.Lerp(weaponBobFactor, characterMovementFctor, bobSharpness * Time.deltaTime);

                // 흔들림량 (조준시, 평상시) 
                float bobAmount =IsAiming ? aimngBobAmount :  defaultBobAmount;
                float frequency = bobFrequency;
                // 좌우 흔들림 
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;  // 값이 커질수록 빠르게 흔들림 
                // 위 아래 흔들림(좌우 흔들림의 절반)
                float vBobValue = ((Mathf.Sin(Time.time * frequency) * 0.5f)+0.5f )* bobAmount * weaponBobFactor;

                // 흔들림 최종 변수에 적용
                weaponBobLocalPosition.x = hBobValue;
                weaponBobLocalPosition.y =Mathf.Abs(vBobValue);


                // 플레이어의 현재 프레임의 마지막 위치를 저장
                lastCharacterPosition = playerCharacterController.transform.position;
            }
        }



        // 상태에 따른 무기 연출
        void UpdateWeaponSwitching()
        {
            // Lerp 시간
            float switchingTimeFactor = 0f;
            if(weaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01(Time.time - weaponSwitchTimeStarted / weaponSwitchDelay);
            }

            // 지연시간 이후 무기 상태 바꾸기
            if(switchingTimeFactor >=1f)
            {
                if(weaponSwitchState == WeaponSwitchState.PutDownPrvious)
                {
                    // 현재 무기 false, 새로운 무기 true
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
            // 지연시간동안 무기의 위치 이동
            if (weaponSwitchState == WeaponSwitchState.PutDownPrvious)
            {
                weaponMainLocalPosition = Vector3.Lerp(defaultWeaponPosition.localPosition, downWeaponPosition.localPosition, switchingTimeFactor);
            }
            else if (weaponSwitchState == WeaponSwitchState.PutUpNew)
            {
              weaponMainLocalPosition = Vector3.Lerp(downWeaponPosition.localPosition,defaultWeaponPosition.localPosition, switchingTimeFactor);
            }
        }

        // weaponSlot에 무기 프리팹으로 생성한 WeaponController 오브젝트 추가 
        public bool AddWeapon(WeaponController weaponPrefab)
        {
            // 추가하는 무기 소지 여부 체크 - 중복검사
            if (HasWeapon(weaponPrefab) != null)
            {
                Debug.Log("Has same Weapon");
                return false;
            }

            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null)
                {
                    WeaponController weaponInstance = Instantiate(weaponPrefab, weaponParentSocket); // 소캣의 자식으로 생성
                    weaponInstance.transform.localPosition = Vector3.zero; // 이 위치로
                    weaponInstance.transform.localRotation = Quaternion.identity;

                    weaponInstance.Owner = this.gameObject;
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                    weaponInstance.ShowWeapon(false);

                    // 무기장착
                    OnAddedWeapon.Invoke(weaponInstance,i);

                    weaponSlots[i] = weaponInstance;  // null이면 빈슬롯에 들어감 
                    return true;
                }
            }
            Debug.Log("weaponSlots full!");
            return false;
        }
        // WeaponSlots에 장착도니 무기 제거
        public bool RemoveWeapon(WeaponController oldWeapon)
        {
            for(int i = 0; i < weaponSlots.Length; i ++)
            {
                // 같은 무기 찾기
                if (weaponSlots[i] == oldWeapon)
                {
                    // 제거
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



        // 매개변수로 들어온 프리팹으로 만든 무기가 있는지 체크 
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

        // 현재 활성화된 웨폰
        public WeaponController GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }

        // 지정된 슬롯에 무기가 있는지 여부 체크
        public WeaponController GetWeaponAtSlotIndex(int index)
        {
            if (index >= 0 && index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }
            return null;
        }

        // 0~9  
        // 무기 바꾸기, 현재 들고 있는 무기 false, 새로운 무기 true
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1;        // 새로 액티브 할 무기 인덱스 
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
            // 새로 액티브할 무기 인덱스로 무기 교체
            SwitchToWeaponIndex(newWeaponIndex);
        }

        // 새로 액티브할 무기 인덱스로 무기 교체
        private void SwitchToWeaponIndex(int newWeaponIndex)
        {
            // newWeaponIndex 값 체크 
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

                // 현재 액티브한 무기가 있는가?
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
                    weaponSwitchState = WeaponSwitchState.PutDownPrvious; // 액티브한 총이있으면 내림
                }
            }
        }

        // 슬롯간 거리
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