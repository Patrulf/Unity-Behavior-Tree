using System.Collections.Generic;
using System.Linq;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Action_SetHighestThreatTargetFromList : Script_Action {

	private Script_BehaviourTree _tree;
	private blackboard_flags _entityListFlag;
	private Script_IEnemy _threatReceiver;

	private blackboard_flags _entityWithHighestThreatFlag;


	public Script_Action_SetHighestThreatTargetFromList(Script_BehaviourTree p_tree, blackboard_flags p_entityListFlag, Script_IEnemy p_threatReceiver, blackboard_flags p_entityWithHighestThreatFlag)
	{
		_tree = p_tree;
		_entityListFlag = p_entityListFlag;
		_threatReceiver = p_threatReceiver;
		_entityWithHighestThreatFlag = p_entityWithHighestThreatFlag;

	}

	public override NodeState RunNode(float p_delta)
	{
		List<Script_IEntity> entityList = _tree.GetBlackBoardElement <List<Script_IEntity>>(_entityListFlag);
		Script_IEntity entityWithHighestThreat = GetHighestThreat (entityList);
		if (entityWithHighestThreat == null) {
			return NodeState.Failed;
		}

		_tree.SetBlackboardElement (_entityWithHighestThreatFlag, entityWithHighestThreat);

		return NodeState.Success;

	}

	private Script_IEntity GetHighestThreat(List<Script_IEntity> p_entityList)
	{
		int threat = 0;
		Script_IEntity highestThreatEntity = null;

		foreach (Script_IEntity entity in p_entityList.ToList()) {			
					
			Script_IFriendly friendly = entity as Script_IFriendly;

			if (_threatReceiver.GetThreat(friendly) > threat) {
				highestThreatEntity = entity;
				threat = _threatReceiver.GetThreat (friendly);				
			}
		}

		return highestThreatEntity;

	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

}

