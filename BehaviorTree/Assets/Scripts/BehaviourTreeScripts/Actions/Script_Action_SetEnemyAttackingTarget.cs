using System.Collections.Generic;
using Namespace_Config;
using Namespace_NodeState;
using System.Linq;
using UnityEngine.Assertions;

public class Script_Action_SetEnemyAttackingTarget : Script_Action {

	private Script_BehaviourTree _tree;

	private blackboard_flags _enemyListFlag;
	private Script_IEntity _targetEntity;
	private blackboard_flags _targetToAttackFlag;

	public Script_Action_SetEnemyAttackingTarget(Script_BehaviourTree p_tree, blackboard_flags p_enemyListFlag, Script_IEntity p_targetEntity, blackboard_flags p_targetToAttackFlag) {
		_tree = p_tree;
		_enemyListFlag = p_enemyListFlag;
		_targetEntity = p_targetEntity;
		_targetToAttackFlag = p_targetToAttackFlag;

	}

	public override NodeState RunNode(float p_delta)
	{
		List<Script_IEntity> enemyList = _tree.GetBlackBoardElement<List<Script_IEntity>> (_enemyListFlag);

		if (_targetEntity == null)
			return NodeState.Failed;

		if (enemyList.Count == 0 || enemyList == null) {
			return NodeState.Failed;

		}

		foreach (Script_IEntity enemyEntity in enemyList.ToList()) {
			Script_IEnemy enemy = enemyEntity as Script_IEnemy;
			Assert.IsNotNull (enemy);
			if (enemy.GetTargetToAttack () == _targetEntity) {
				_tree.SetBlackboardElement(_targetToAttackFlag,enemy);
				return NodeState.Success;
			}
		}
		return NodeState.Failed;
	}


	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

}
