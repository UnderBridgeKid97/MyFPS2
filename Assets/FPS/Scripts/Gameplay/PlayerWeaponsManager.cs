using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;


namespace Unity.FPS.Game
{
    /// <summary>
    /// 
    /// �÷��̾ ���� ����(weapon)�� �����ϴ� Ŭ���� 
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
        public UnityAction<WeaponController> OnSwitchToWeapon; // ���� ��ü �� ��ϵ� �Լ� ȣ�� 

        private WeaponSwitchState weaponSwitchState;    // ���� ��ü�� ���� 

        // �ڵ鷯 ����
        private PlayerInputHandler playerInputHandler;

        // ���� ��ü�� ���Ǵ� ������ġ
        public Vector3 weaponMainLocalPosition;

        public Transform defaultWeaponPosition;
        public Transform downWeaponPosition;

        private int weaponSwitchNewIndex;           // ���� �ٲ�� ���� �ε��� 

        private float weaponSwitchTimeStarted = 0f;
        [SerializeField]private float weaponSwitchDelay = 1f;
        #endregion

        private void Start()
        {
            // ����
            playerInputHandler = GetComponent<PlayerInputHandler>();    

            // �ʱ�ȭ
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;

            //
            OnSwitchToWeapon += OnweaponSwitched;

            //���޹��� ���� ����
            foreach (var weapon in startingWeapons)
            {
                AddWeapon(weapon); // ���� ���� 
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
                    bool switchUp = switchWeaponInput > 0;      // ���� ���߿��� �Է��ص� �ȹٲ�
                    SwitchWeapon(switchUp);
                }
            }
        }

        private void LateUpdate()
        {
            UpdateWeaponSwitching();

            // ���� ��� ���� ���� ���� ���������� �Ҵ� => ���� ������ġ 
            weaponParentSocket.localPosition = weaponMainLocalPosition;

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

                    weaponSlots[i] = weaponInstance;  // null�̸� �󽽷Կ� �� 

                    return true;
                }
            }
            Debug.Log("weaponSlots full!");
            return false;
        }

        // �Ű������� ���� 
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


    }
}