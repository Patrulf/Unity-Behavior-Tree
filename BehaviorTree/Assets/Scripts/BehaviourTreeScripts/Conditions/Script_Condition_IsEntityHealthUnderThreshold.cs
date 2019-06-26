using Namespace_NodeState;
using Namespace_Config;

public class Script_Condition_IsEntityHealthUnderThreshold : Script_Condition {

	private Script_BehaviourTree _tree;
	private blackboard_flags _entityToCheckFlag;
	private Script_IEntity _entityToCheck;
	private int _healthThreshold;

	public Script_Condition_IsEntityHealthUnderThreshold(Script_BehaviourTree p_tree, blackboard_flags p_entityToCheckFlag, int p_healthThreshold)
	{
		_tree = p_tree;
		_entityToCheckFlag = p_entityToCheckFlag;
		_healthThreshold = p_healthThreshold;
	}

	public override NodeState RunNode(float p_delta)
	{
		_entityToCheck = _tree.GetBlackBoardElement<Script_IEntity> (_entityToCheckFlag);

		if (_entityToCheck == null) {
			return NodeState.Failed;
		}

		if (_entityToCheck.GetHealth () < _healthThreshold) {
			return NodeState.Success;
		}
			
		return NodeState.Failed;
	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}
}
