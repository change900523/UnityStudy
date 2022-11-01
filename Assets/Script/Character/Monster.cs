using System;
using UnityEngine;

public class Monster : BattleObject
{
    [SerializeField]
    private GameObject targetRim = null;
    [SerializeField]
    private Animator animator = null;
    [SerializeField]
    private AttackData[] attackDatas = null;
    [SerializeField]
    private MonsterData monsterData = null;
    [SerializeField]
    private HPBar hpBar = null;


    private MonsterBehaviorTree behaviorTree = null;
    private MonsterBehaviorTreeData treeData = null;
    private MonsterAttackManager attackManager = new MonsterAttackManager();
    private MonsterDieLogic dieLogic = null;
    private MonsterChaseLogic chaseLogic = null;
    private AttackAnimationLogic attackAnimationLogic = null;
    private MonsterComeBackLogic comeBackLogic = null;
    private MonsterIdleLogic idleLogic = null;

    public bool IsTarget { get; private set; } = false;

    private void Awake()
    {
        targetRim.SetActive(false);
        Tribe = monsterData.Trive;
        attack = monsterData.Attack;
        hp = monsterData.HP;
        hpBar.Initialize(hp);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        behaviorTree.Update(deltaTime);
        attackManager.Update(deltaTime);
    }

    public void Initialize(Player target, Vector3 basePosition, Action<AttackInfo, uint, BattleObject, BattleObject, float> inAttackAction)
    {
        treeData = new MonsterBehaviorTreeData(transform, monsterData.AggroDistance);
        attackAction = inAttackAction;
        SetTree(basePosition);
        attackManager.RegistAttackInfo(attackDatas);
        treeData.Target = target;
    }

    private void SetTree(Vector3 basePosition)
    {
        dieLogic = new MonsterDieLogic(animator);
        chaseLogic = new MonsterChaseLogic(treeData, animator);
        attackAnimationLogic = new AttackAnimationLogic(treeData, animator, attackManager, Attack);
        comeBackLogic = new MonsterComeBackLogic(treeData, animator, basePosition);
        idleLogic = new MonsterIdleLogic(treeData, attackManager, basePosition, monsterData.ComeBackDistance);

        behaviorTree = new MonsterBehaviorTree(treeData,
                                    dieLogic.StartLogic,
                                    chaseLogic.StartLogic,
                                    chaseLogic.UpdateLogic,
                                    attackAnimationLogic.StartLogic,
                                    attackAnimationLogic.UpdateLogic,
                                    comeBackLogic.StartLogic,
                                    comeBackLogic.UpdateLogic,
                                    idleLogic.UpdateLogic);
        behaviorTree.SetTree();
    }


    public void SetTarget(bool value)
    {
        if (IsDie() == false)
        {
            IsTarget = value;
            targetRim.SetActive(value);
        }
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        hpBar.SetHP(hp);

        DamageFont damageFont = DamageFontPool.Instance.GetDamageFont();
        damageFont.ShowDamage(damage, this.transform);
    }

    protected override void Die()
    {
        treeData.IsDie = true;
    }
}
