using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Runtime.Serialization;
using UnityEngine.UI;

public abstract class Fighter : MonoBehaviour
{
    protected float currentHealth;
    protected float maxHealth = 10f; //default HP value
    protected GameObject currentTarget;
    public SphereCollider range;

    protected float radius = 1f; //range in units
    protected float radiusDefault = 1f;
    protected float shotCooldown = 5f; //time between shots
    protected float timeSinceLastShot = 0f;
    protected bool onCooldown = false;

    protected List<GameObject> enemiesInRange = new List<GameObject>();
    protected List<GameObject> towersInRange = new List<GameObject>();

    protected FighterTypes targetType;
    protected TargetingModes targetingMode = TargetingModes.Closest;
    protected AttackTypes attackType;

    protected List<int> visibleByTowers = new List<int>();

    protected float distanceToDestination = 0f;

    //status effects
    protected List<StatusEffect> statusEffects = new List<StatusEffect>();
    StatusEffect effectBuffer = null;
    int statusEffectIndex = 0;


    protected float damageAmount = 0;
    protected DamageMatrix damageMatrixBase; //before items and effects
    protected DamageMatrix damageMatrix; //after items and effects

    [EnumArray(typeof(DamageTypes), 1, "Damage Resistances", (int)DamageTypes.True, 1.0f)]
    public ValuesContainer damageResistances;
    [EnumArray(typeof(DamageTypes), 0, "Damage Attack", (int)DamageTypes.PhysicalBlunt, 1.0f)]
    public ValuesContainer damageAttack;


    //UI
    public Image HPfill;
    public Image CDfill; 

    protected virtual void Awake()
    {
        
        ResetStats();
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void OnEnable()
    {
        ResetStats();
    }

    protected virtual void OnDisable()
    {
    }

    protected virtual void Update()
    {
        //attacks if off cooldown and has targets in sight
        if (timeSinceLastShot > shotCooldown && currentTarget != null && currentTarget.activeInHierarchy)
        {
            Attack();
            timeSinceLastShot = 0;
        }

        //checks and executes status effects
        if (statusEffects.Count > 0)
        {
            
            if (!statusEffects[statusEffectIndex].Tick())
            {
                statusEffects.RemoveAt(statusEffectIndex);
            }
            else
            {
                statusEffectIndex++;
            }
            if (statusEffectIndex >= statusEffects.Count) statusEffectIndex = 0;
        }


    }

    public virtual void Attack()
    {
        switch (attackType)
        {
            case AttackTypes.AoE:
                foreach (GameObject target in GetCurrentTargets())
                {
                    Damage(target);
                }
                break;
            case AttackTypes.RandomTarget:
            case AttackTypes.SingleTarget:
                Damage(currentTarget);
                break;
        }
    }

    public virtual void ResetStats()
    {
        timeSinceLastShot = shotCooldown; //fighter starts with no cooldown
        damageMatrix = new DamageMatrix(damageAttack.Values, damageResistances.Values);

        currentHealth = maxHealth;
        enemiesInRange = new List<GameObject>();
        towersInRange = new List<GameObject>();
        visibleByTowers = new List<int>();

        statusEffects = new List<StatusEffect>();
    }

    public virtual void Damage(GameObject target, bool heal = false) 
    {
        if(target.GetComponentInChildren<Fighter>() != null && target.activeInHierarchy) target.GetComponentInChildren<Fighter>().GetDamaged(damageMatrix, heal);
    }

    public virtual void Heal(GameObject target)
    {
        Damage(target, true);
    }

    public virtual void GetDamaged(DamageMatrix attackMatrix, bool heal = false)
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.GetType() == typeof(BufferStatus))
            {
                effect.TickDown(1);

                return;
            }

        }

        //foreach attack matrix type of damage that is not 0 - trigger the effect mb?
        damageAmount = attackMatrix.CalcualateDamage(attackMatrix);
        HealthChanged(heal);

        if (heal)
        {
            currentHealth = Mathf.Clamp(currentHealth + damageAmount, currentHealth, maxHealth); //can't heal for negative amount or over max health
            return;
            //overheal status effect?
        }

        //calculated resistances and all that nonsense here
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die(bool money = false)
    {
        gameObject.SetActive(false);
        //overriden for tower and enemy, enqueues towers/enemies and signals the towers
    }

    public virtual void HealthChanged(bool positive)
    {
        UpdateHealthUI();
    }

    public virtual void UpdateHealthUI()
    {

    }

    public virtual void ToggleTowerVisibility(int reference, bool toggle = true)
    {
        if (toggle)
        {
            visibleByTowers.Add(reference);
            return;
        }
        visibleByTowers.Remove(reference);
    }


    //getters

    public virtual float GetHealth()
    {
        return currentHealth;
    }

    public virtual List<GameObject> GetCurrentTargets()
    {
        switch (targetType)
        {
            case FighterTypes.Enemy:
                return enemiesInRange;
            case FighterTypes.Tower:
                return towersInRange;
            default:
                return null;
        }
    }


    public virtual float GetDistance()
    {
        //check if there is agent -> get remaining distance
        //if there is no agent get distance from object to core

        if (GetComponent<NavMeshAgent>() != null)
        {
            return distanceToDestination;
        }

        return GridManager.Instance.GetDistanceToCore(transform);
    }

    public virtual float GetSpeed()
    {
        if (GetComponent<NavMeshAgent>() != null)
        {
            return GetComponent<NavMeshAgent>().speed;
        }

        return 0f;
    }

    public virtual List<StatusEffect> GetStatusEffects()
    {
        return statusEffects;
    }
    public virtual int GetStatusStacks<T>() where T : StatusEffect
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.GetType() == typeof(T)) return effect.GetTicks();
        }

        return 0;
    }

    //setters
    public virtual void AddStatusEffect<T>(int amount) where T:StatusEffect
    {
        effectBuffer = null;
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.GetType() == typeof(T)) effectBuffer = effect;
        }

        if(effectBuffer == null)
        {
            effectBuffer = (StatusEffect)FormatterServices.GetUninitializedObject(typeof(T));
            effectBuffer.Init(this, amount);
            statusEffects.Add(effectBuffer);
        } 
        else
        {
            effectBuffer.TickUp(amount);
        }
    }
    public virtual void RemoveStatusEffect<T>(int amount) where T : StatusEffect
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.GetType() == typeof(T))
            {
                effect.TickDown(amount);
            }
        }
    }
}

public enum FighterTypes
{
    Tower,
    Enemy
}

public enum TargetingModes
{
    Closest,
    Fastest,
    MostHP,
    LeastHP
}

public enum AttackTypes
{
    SingleTarget,
    RandomTarget,
    AoE
}

public enum DamageTypes
{
    PhysicalBlunt,
    PhysicalPiercing,
    PhysicalSlashing,
    Electric,
    Fire,
    Ice,
    Mental,
    Poison,
    True
}

public class DamageMatrix
{
    public Dictionary<DamageTypes, float> attackMatrix = new Dictionary<DamageTypes, float>();
    public Dictionary<DamageTypes, float> defenseMatrix = new Dictionary<DamageTypes, float>();
    float calucalatedDamage = 0;

    public DamageMatrix(float[] attackModifiers, float[] defenseModifiers)
    {
        foreach (DamageTypes type in System.Enum.GetValues(typeof(DamageTypes)))
        {
            attackMatrix[type] = attackModifiers[(int)type];
            defenseMatrix[type] = defenseModifiers[(int)type];
        }
    }

    public Dictionary<DamageTypes, float> GetAttackMatrix() { return attackMatrix;}
    public Dictionary<DamageTypes, float> GetDefenseMatrix() { return defenseMatrix; }

    public float CalcualateDamage(DamageMatrix attacker)
    {
        calucalatedDamage = 0;

        foreach (DamageTypes type in System.Enum.GetValues(typeof(DamageTypes)))
        {
            calucalatedDamage +=  attacker.attackMatrix[type] * defenseMatrix[type];
        }

        return calucalatedDamage;
    }

}

[System.Serializable]
public class ValuesContainer
{
    public float[] Values;
}