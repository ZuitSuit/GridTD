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
    protected float price; // tower price or enemy payout
    protected GameObject currentTarget;
    public SphereCollider range; //collider used for tracking visible targets
    protected float fighterRange = 20f; //range of the fighter after recalculations
    protected float fighterRangeDefault = 1f; //degault range of a fighter
    protected float shotCooldown = 5f; //time between shots
    protected float timeFromLastShot = 0f;
    protected float timeFromLastTarget = 0f;
    protected bool onCooldown = false;

    //protected FighterTypes targetType;
    protected TargetingModes targetingMode = TargetingModes.Closest;
    protected AttackTypes attackType;
    protected System.Type targetType;
    protected System.Type fighterType;

    protected float distanceToDestination = 0f;

    //status effects
    protected List<StatusEffect> statusEffects = new List<StatusEffect>();
    protected List<StatusEffect> effectsOnHit = new List<StatusEffect>();

    StatusEffect effectBuffer = null;
    int statusEffectIndex = 0;

    //damage variables
    protected float damageAmount = 0;
    protected DamageMatrix damageMatrixBase; //before items and effects
    protected DamageMatrix damageMatrix; //after items and effects

    [EnumArray(typeof(DamageTypes), 1, "Damage Resistances", (int)DamageTypes.True, 1.0f)]
    public ValuesContainer damageResistances;
    [EnumArray(typeof(DamageTypes), 0, "Damage Attack", (int)DamageTypes.PhysicalBlunt, 1.0f)]
    public ValuesContainer damageAttack;

    //targeting variables
    GameObject potentialTarget;
    float comparisonValue;
    float comparisonValueCheck;
    bool firstComparison = false;
    bool comparisonMore = true;
    protected List<GameObject> targetsBuffer;
    public Transform turret;
    protected float rotationSpeed = 0.02f;
    protected Fighter fighterBuffer;
    protected Renderer fighterRendererBuffer;
    protected string fighterClassBuffer;

    protected Dictionary<string, List<GameObject>> targetsInRange = new Dictionary<string, List<GameObject>>();
    protected Dictionary<int, Fighter> visbleByFighters = new Dictionary<int, Fighter>();

    //UI
    public Image HPfill;
    public Image CDfill;

    //movement
    protected bool isFlying = false;
    protected float defaultSpeed;
    protected float terrainSpeedModifier = 1.0f;
    protected float speedModifier = 1.0f;
    public NavMeshAgent agent;

    //misc
    bool inFocus = false;
    public string fighterName;

    protected virtual void Awake()
    {

        ResetStats();
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


        if (onCooldown == true)
        {
            timeFromLastShot += Time.deltaTime;
            UpdateCooldownUI();
            onCooldown = !(timeFromLastShot > shotCooldown);
        }

        //attacks if off cooldown and has targets in sight
        if (!onCooldown && ((currentTarget != null && currentTarget.activeInHierarchy) || (attackType == AttackTypes.AoE && targetsInRange.ContainsKey(targetType.ToString()) && targetsInRange[targetType.ToString()].Count > 0)))
        {
            Attack();
            timeFromLastShot = 0;
            timeFromLastTarget = 0;
            onCooldown = true;
        }

        timeFromLastTarget += Time.deltaTime;


        if (attackType != AttackTypes.AoE)
        {
            if (currentTarget != null && currentTarget.activeInHierarchy)
            {
                if (turret != null)
                {
                    turret.transform.localRotation =
                        Quaternion.Slerp(turret.transform.localRotation, Quaternion.LookRotation(turret.transform.position - currentTarget.transform.position), rotationSpeed * timeFromLastTarget);
                }
            }
            else
            {
                Retarget();
            }
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

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        fighterBuffer = other.gameObject.GetComponent<WhereIs>().GetFighter();
        if (fighterBuffer == null) return;

        RegisterVisible(fighterBuffer, true);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        fighterBuffer = other.gameObject.GetComponent<WhereIs>().GetFighter();
        if (fighterBuffer == null) return;

        RegisterVisible(fighterBuffer, false);
    }

    public virtual bool RegisterVisible(Fighter fighter, bool toggle = true)
    {
        //show the object that it is seen by a tower
        //add to objects in range

        fighterClassBuffer = fighter.GetFighterType().ToString();
        if (toggle)
        {
            if (!targetsInRange.ContainsKey(fighterClassBuffer)) targetsInRange.Add(fighterClassBuffer, new List<GameObject>());
            targetsInRange[fighterClassBuffer].Add(fighter.gameObject);
            fighter.ToggleVisibleBy(this, true);
            
            return true;
        }

        if (fighter.gameObject == currentTarget) currentTarget = null;
        fighter.ToggleVisibleBy(this, false);
        return targetsInRange[fighterClassBuffer].Remove(fighter.gameObject);
    }
    
    public virtual void ToggleVisibleBy(Fighter fighter, bool toggle = true)
    {
        int fighterID = fighter.gameObject.GetInstanceID();
        if (visbleByFighters.ContainsKey(fighterID))
        {
            if (!toggle)
            {
                visbleByFighters.Remove(fighterID);
            }
        }
        else if (toggle)
        {
            visbleByFighters.Add(fighterID, fighter);
        }
    }

    public virtual void RemoveTarget(Fighter fighter)
    {
        if (currentTarget == fighter.gameObject) currentTarget = null;
        if (targetsInRange.ContainsKey(fighter.GetFighterType().ToString())) targetsInRange[fighter.GetFighterType().ToString()].Remove(fighter.gameObject);
    }

    public virtual void SpawnCheck()
    {
        //updates fighter vision on spawn - triggers ontrigger enter events from inside the trigger
        //sphere 1 - who sees me
        fighterRendererBuffer = null;

        //check if there are fighter vision zones in range, add this fighter to their knowledge base
        fighterRendererBuffer = gameObject.GetComponent<Renderer>();
        if(fighterRendererBuffer != null)
        {
            if(Physics.CheckBox(fighterRendererBuffer.bounds.center, fighterRendererBuffer.bounds.extents))
            {
                Collider[] colliders = Physics.OverlapBox(fighterRendererBuffer.bounds.center, fighterRendererBuffer.bounds.extents);
                foreach (Collider other in colliders)
                {
                    fighterBuffer = other.GetComponent<WhereIs>().GetFighter();
                    fighterBuffer.RegisterVisible(this, true);
                }
            }
        }

        if (Physics.CheckSphere(gameObject.transform.position, fighterRange))
        {
            Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, fighterRange);
            foreach (Collider other in colliders)
            {
                fighterBuffer = other.GetComponent<WhereIs>().GetFighter();
                fighterBuffer.RegisterVisible(this, true);
            }
        }
    }


    protected virtual void Attack()
    {
        switch (attackType)
        {
            case AttackTypes.AoE:
                foreach (GameObject target in targetsInRange[targetType.ToString()])
                {
                    if(target.activeInHierarchy) Damage(target);
                }
                break;
            case AttackTypes.RandomTarget:
                Damage(currentTarget);

                currentTarget = null;
                break;
            case AttackTypes.SingleTarget:
                
                Damage(currentTarget);
                break;
        }
    }

    public virtual void Retarget()
    {

        currentTarget = null;
        //random targeting
        if (attackType == AttackTypes.RandomTarget)
        {
            if (!targetsInRange.ContainsKey(targetType.ToString())) return;

            targetsBuffer = targetsInRange[targetType.ToString()];
            currentTarget = targetsBuffer.Count == 0 ? null : targetsBuffer[Random.Range(0, targetsBuffer.Count)];
            return;
        }

        if (targetsInRange.ContainsKey(targetType.ToString()) && targetsInRange[targetType.ToString()].Count != 0)
        {
            firstComparison = true;
            foreach (GameObject possibleTarget in targetsInRange[targetType.ToString()])
            {
                switch (targetingMode)
                {
                    case TargetingModes.Closest:
                        comparisonValueCheck = possibleTarget.GetComponent<Fighter>().GetDistance();
                        comparisonMore = true; //TODO remove from this place?
                        break;
                    case TargetingModes.Fastest:
                        comparisonValueCheck = possibleTarget.GetComponent<Fighter>().GetSpeed();
                        comparisonMore = true;
                        break;
                    case TargetingModes.MostHP:
                        comparisonValueCheck = possibleTarget.GetComponent<Fighter>().GetHealth();
                        comparisonMore = true;
                        break;
                    case TargetingModes.LeastHP:
                        comparisonValueCheck = possibleTarget.GetComponent<Fighter>().GetHealth();
                        comparisonMore = false;
                        break;
                }

                if (firstComparison || (comparisonMore ? (comparisonValueCheck > comparisonValue) : (comparisonValueCheck < comparisonValue)))
                {
                    potentialTarget = possibleTarget;
                    comparisonValue = comparisonValueCheck;
                }
            }

            //TODO choses the same target twice

            timeFromLastTarget = 0;
            firstComparison = false;
            currentTarget = potentialTarget;
        }
    }

    public virtual void ResetStats()
    {
        defaultSpeed = (agent == null) ? 0 : agent.speed; 
        timeFromLastShot = shotCooldown; //fighter starts with no cooldown
        damageMatrix = new DamageMatrix(damageAttack.Values, damageResistances.Values);

        currentHealth = maxHealth;
        targetsInRange = new Dictionary<string, List<GameObject>>();

        statusEffects = new List<StatusEffect>();
    }

    public virtual void Damage(GameObject target, bool heal = false) 
    {

        if (target.GetComponent<Fighter>() != null && target.activeInHierarchy)
        {
            fighterBuffer = target.GetComponent<Fighter>();
            fighterBuffer.GetDamaged(damageMatrix, heal);
            foreach(StatusEffect effect in effectsOnHit)
            {
                fighterBuffer.AddStatusEffect(effect);
            }
        } 
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
        //gameObject.SetActive(false);
        foreach(Fighter fighter in visbleByFighters.Values)
        {
            fighter.RemoveTarget(this);
        }

        //overriden for tower and enemy, enqueues towers/enemies and signals the towers
    }

    public virtual void HealthChanged(bool positive)
    {
        UpdateHealthUI();
    }

    public virtual void UpdateHealthUI()
    {
        if (HPfill != null) {
            HPfill.fillAmount = Mathf.Clamp(currentHealth / maxHealth, 0, 1f);  
            
        }
    }

    public virtual void UpdateCooldownUI()
    {
        if (CDfill != null) {
            CDfill.fillAmount = Mathf.Clamp(timeFromLastShot / shotCooldown, 0, 1f);
            if (inFocus)
            {
                UIManager.Instance.SetCD(CDfill.fillAmount);
            }
        }
    }

    //getters
    public virtual System.Type GetFighterType()
    {
        if (fighterType == null) fighterType = GetFighterClass();
        return fighterType;
    }

    public virtual System.Type GetFighterClass()
    {
        System.Type typeBuffer = GetType();
        List<System.Type> inheritanceOrder = new List<System.Type>();
        while(typeBuffer != null)
        {
            inheritanceOrder.Add(typeBuffer);
            typeBuffer = typeBuffer.BaseType;
        }

        Debug.Log(inheritanceOrder[Mathf.Clamp(inheritanceOrder.Count - 7, 0, inheritanceOrder.Count-1)]);

        return GetType();
    }
    public virtual string GetName()
    {
        return fighterName;
    }
    public virtual float GetHealth()
    {
        return currentHealth;
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
    public virtual void ToggleInFocus(bool toggle) { inFocus = toggle; }
    public virtual void SetTargetingMode(TargetingModes m) { targetingMode = m; }
    public virtual void SetName(string nameString) { fighterName = nameString; }
    public virtual void SetAttackType(AttackTypes t) {attackType = t; }
    public virtual void AddStatusEffect(StatusEffect effect, int amount = 1)
    {
        foreach (StatusEffect e in statusEffects)
        {
            if (e.GetType() == effect.GetType()) 
            {
                e.TickUp(amount);
                return;
            }
        }

        effect.SetFighter(this);
        statusEffects.Add(effect);
    }
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

    public virtual void RecalculateSpeed() 
    {
        if (agent != null) agent.speed = (isFlying ? 1.0f : terrainSpeedModifier)  * defaultSpeed * speedModifier;
/*        Debug.Log("terrain: " + terrainSpeedModifier);
        Debug.Log("default: " + defaultSpeed);
        Debug.Log("from towers: " + speedModifier);*/
    }
    public virtual void SetSpeedModifier(float modifier) { speedModifier = modifier; }
    public virtual void SetTerrainSpeedModifier(float cost) { terrainSpeedModifier = 1f/ cost; }
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