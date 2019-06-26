using UnityEngine.Assertions;
using Namespace_Config;
using Namespace_NodeState;

public class Script_Action_SetTargetToAttack : Script_Action {

	private Script_BehaviourTree _tree;

	private blackboard_flags _targetToAttackFlag;
	private Script_IEntity _enemyToSet;

	public Script_Action_SetTargetToAttack(Script_BehaviourTree p_tree, blackboard_flags p_targetToAttackFlag, Script_IEntity p_enemyToSet){
		_tree = p_tree;

		_targetToAttackFlag = p_targetToAttackFlag;
		_enemyToSet = p_enemyToSet;

	}


	public override NodeState RunNode(float p_delta)
	{
		Script_IEntity targetToAttack = _tree.GetBlackBoardElement<Script_IEntity> (_targetToAttackFlag);

		if (targetToAttack == null) {
			return NodeState.Failed;
		}
			
		Script_IEnemy enemyToSet = _enemyToSet as Script_IEnemy;
		enemyToSet.SetTargetToAttack (targetToAttack);

		return NodeState.Success;
	}
		
	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

}
