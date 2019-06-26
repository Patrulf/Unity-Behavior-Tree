using Namespace_NodeState;
using Namespace_Config;

public class Script_Condition_IsEntityNull : Script_Condition {

	private Script_BehaviourTree _tree;
	private blackboard_flags _entityFlag;
	private Script_IEntity _entity;

	public Script_Condition_IsEntityNull(Script_BehaviourTree p_tree, blackboard_flags p_entityFlag)
	{
		_tree = p_tree;
		_entityFlag = p_entityFlag;
	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{
		_entity = _tree.GetBlackBoardElement<Script_IEntity> (_entityFlag);

		if (_entity == null || _entity.GetGameObject() == null) {
			return NodeState.Success;
		}

		return NodeState.Failed;

	}


}
