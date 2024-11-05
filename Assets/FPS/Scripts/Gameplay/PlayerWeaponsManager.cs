using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;


namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// 플레이어가 가진 무기(weapon)를 관리하는 클래스 
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
        public UnityAction<WeaponController> OnSwitchToWeapon; // 무기 교체 시 등록된 함수 호출 

        private WeaponSwitchState weaponSwitchState;    // 무기 교체시 상태 

        // 핸들러 참조
        private PlayerInputHandler playerInputHandler;

        // 무기 교체시 계산되는 최종위치
        public Vector3 weaponMainLocalPosition;

        public Transform defaultWeaponPosition;
        public Transform downWeaponPosition;

        private int weaponSwitchNewIndex;           // 새로 바뀌는 무기 인덱스 

        private float weaponSwitchTimeStarted = 0f;
        [SerializeField]private float weaponSwitchDelay = 1f;
        #endregion

        private void Start()
        {
            // 참조
            playerInputHandler = GetComponent<PlayerInputHandler>();    

            // 초기화
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;

            //
            OnSwitchToWeapon += OnweaponSwitched;

            //지급받은 무기 장착
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon); // 무기 생성 
            }
            SwitchWeapon(true);
        }

        private void Update()
        {
            if(weaponSwitchState == WeaponSwitchState.Up || weaponSwitchState == WeaponSwitchState.Down)
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();
                if (switchWeaponInput != 0)
                {
                    bool switchUp = switchWeaponInput > 0;      // 연출 도중에는 입력해도 안바뀜
                    SwitchWeapon(switchUp);
                }
            }
        }

        private void LateUpdate()
        {
            UpdateWeaponSwitching();

            // 최종 계산 값을 소켓 로컬 포지션으로 할당 => 무기 최종위치 
            weaponParentSocket.localPosition = weaponMainLocalPosition;

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

                    weaponSlots[i] = weaponInstance;  // null이면 빈슬롯에 들어감 

                    return true;
                }
            }
            Debug.Log("weaponSlots full!");
            return false;
        }

        // 매개변수로 들어온 
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


    }
}