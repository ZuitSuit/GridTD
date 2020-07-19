using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Runtime.Serialization;

public abstract class Fighter : MonoBehaviour
{
    protected float currentHealth;
    protected float maxHealth = 100f;
    protected GameObject currentTarget;
    public SphereCollider range;

    protected float radius = 1f; //range in units
    protected float radiusDefault = 1f;
    protected float shotCooldown = 5f; //time between shots
    protected float timeSinceLastShot = 0f;

    protected float damage = 20f;

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


    /*    public float[] damageValues = new float[Enum.GetNames(typeof(DamageTypes)).Length];
        public float[] resistanceValues = new float[Enum.GetNames(typeof(DamageTypes)).Length];*/

    [EnumArray(typeof(DamageTypes), 1, "Damage Resistances", (int)DamageTypes.True, 1.0f)]
    public ValuesContainer _damageResistances;



    [EnumArray(typeof(DamageTypes), 0, "Damage Attack", (int)DamageTypes.PhysicalBlunt, 1.0f)]
    public ValuesContainer _damageAttack;

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
        //ResetStats();
    }

    protected virtual void Update()
    {
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

    public virtual void ResetStats()
    {
        currentHealth = maxHealth;
        enemiesInRange = new List<GameObject>();
        towersInRange = new List<GameObject>();
        visibleByTowers = new List<int>();

        //reset status effects
        //reset upgrades etc
    }

    public virtual void Damage(GameObject target, float amount, bool heal = false) 
    {
        if(target.GetComponentInChildren<Fighter>() != null && target.activeInHierarchy) target.GetComponentInChildren<Fighter>().GetDamaged(amount, heal);
    }

    public virtual void Heal(GameObject target, float amount)
    {
        Damage(target, amount, true);
    }

    public virtual void GetDamaged(float amount, bool heal = false)
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.GetType() == typeof(BufferStatus))
            {
                effect.TickDown(1);

                return;
            }

        }

        HealthChanged(heal);

        if (heal)
        {
            currentHealth = Mathf.Clamp(currentHealth + amount, currentHealth, maxHealth); //can't heal for negative amount or over max health
            return;
            //overheal status effect?
        }

        //calculated resistances and all that nonsense here
        currentHealth -= amount;
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
        //TODO  event that triggers on any health change
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

public class DamageMatrix : ScriptableObject
{
    Dictionary<DamageTypes, float> _damageMatrix = new Dictionary<DamageTypes, float>();
    public DamageMatrix(float[] damageModifiers)
    {
        foreach (DamageTypes type in System.Enum.GetValues(typeof(DamageTypes)))
        {
            _damageMatrix[type] = damageModifiers[(int)type];
        }
    }
}

[System.Serializable]
public class ValuesContainer
{
    public float[] Values;
}