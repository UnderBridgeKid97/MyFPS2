using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.FPS.Game
{/// <summary>
///  ���� ���� �ȿ� �ִ� �ݶ��̴� ������Ʈ ������ �ֱ� 
/// </summary>

    public class DamageArea : MonoBehaviour
    {
        #region Variables

        [SerializeField]private float areaOfEffectDistance = 10f;
        [SerializeField]private AnimationCurve damageRatioOverDistance;
      //  public AnimationCurve damageRtioDistance;

        #endregion

        public void InflicDamageArea(float damage,Vector3 center, LayerMask layers,
                                     QueryTriggerInteraction interaction, GameObject owner)
        {
            Dictionary<Health,Damageable> uniqueDamagedHealth = new Dictionary<Health, Damageable> ();

            Collider[] affectedColliders = Physics.OverlapSphere(center,areaOfEffectDistance,layers,interaction);   
             foreach ( Collider collider in affectedColliders)
            {
                Damageable damageable = collider.GetComponent<Damageable>(); 
                if(damageable)
                {
                    Health health = damageable.GetComponentInParent<Health>();
                    if(health != null && uniqueDamagedHealth.ContainsKey(health) == false )
                    {
                        uniqueDamagedHealth.Add(health, damageable);
                    }
                }
            }
             // ������ �ֱ�
             foreach( var uniqueDamageable in uniqueDamagedHealth.Values)
            {
                float distance = Vector3.Distance(uniqueDamageable.transform.position,center);
                float curveDamage =damage * damageRatioOverDistance.Evaluate(distance);
                Debug.Log($"curveDamage:{curveDamage}");

                uniqueDamageable.InflictDamage(damage,true,owner);
            }

        }
    }
}