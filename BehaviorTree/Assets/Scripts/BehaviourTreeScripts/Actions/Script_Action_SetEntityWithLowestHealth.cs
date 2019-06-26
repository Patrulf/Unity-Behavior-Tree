using System.Collections.Generic;
using System.Linq;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Action_SetEntityWithLowestHealth : Script_Action {

	private Script_BehaviourTree _tree;

	private blackboard_flags _entityListFlag;
	private blackboard_flags _entityWithLowestHealthFlag;

	public Script_Action_SetEntityWithLowestHealth(Script_BehaviourTree p_tree,blackboard_flags p_entityListFlag, blackboard_flags p_entityWithLowestHealthFlag)
	{
		
		_tree = p_tree;

		_entityListFlag = p_entityListFlag;
		_entityWithLowestHealthFlag = p_entityWithLowestHealthFlag;

	}

	public override NodeState RunNode(float p_delta)
	{
		
		List<Script_IEntity> entityList = _tree.GetBlackBoardElement<List<Script_IEntity>> (_entityListFlag);

		if (entityList.Count == 0 || entityList == null) {
			return NodeState.Failed;
		}

		Script_IEntity lowestHealthEntity = GetEntityWithLowestHealth (entityList);

		if (lowestHealthEntity == null) {
			return NodeState.Failed;
		}

		_tree.SetBlackboardElement (_entityWithLowestHealthFlag, lowestHealthEntity);
		return NodeState.Success;


	}

	private Script_IEntity GetEntityWithLowestHealth(List<Script_IEntity> p_listOfEntities)
	{
		int health = int.MaxValue;
		Script_IEntity lowestHealthEntity = null;

		foreach (Script_IEntity entity in p_listOfEntities.ToList())
		{
			if (entity.GetHealth () < health) {

				health = entity.GetHealth ();
				lowestHealthEntity = entity;

			}
		}
		return lowestHealthEntity;

	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}


}
