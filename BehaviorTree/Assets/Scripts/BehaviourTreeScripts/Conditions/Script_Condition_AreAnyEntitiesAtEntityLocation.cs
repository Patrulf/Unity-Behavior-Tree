using System.Collections.Generic;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Condition_AreAnyEntitiesAtEntityLocation : Script_Condition {

	private Script_BehaviourTree _tree;
	private Script_IEntity _entityToCalculateFrom;
	private blackboard_flags _entityListFlag;

	public Script_Condition_AreAnyEntitiesAtEntityLocation (Script_BehaviourTree p_tree, blackboard_flags p_EntityListFlag, Script_IEntity p_entityToCalculateFrom)
	{
		_tree = p_tree;
		_entityToCalculateFrom = p_entityToCalculateFrom;
		_entityListFlag = p_EntityListFlag;
	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{
		List<Script_IEntity> entityList = _tree.GetBlackBoardElement<List<Script_IEntity>> (_entityListFlag);

		if (entityList.Count == 0)
			return NodeState.Failed;

		foreach (Script_IEntity entity in entityList) {
			if (entity.GetGridLocation () == _entityToCalculateFrom.GetGridLocation ()) {
				return NodeState.Success;
			}	
		}
		return NodeState.Failed;
	}

}
