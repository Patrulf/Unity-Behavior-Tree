using System.Collections.Generic;
using System.Linq;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Condition_IsThreatOverThreshold : Script_Condition {

	private Script_BehaviourTree _tree;

	private blackboard_flags _entityListFlag;
	private Script_IEnemy _threatReceiver;
	int _threatThreshold;

	public Script_Condition_IsThreatOverThreshold(Script_BehaviourTree p_tree, blackboard_flags p_entityListFlag, Script_IEnemy p_threatReceiver, int p_threshold)
	{
		_tree = p_tree;
		_entityListFlag = p_entityListFlag;
		_threatReceiver = p_threatReceiver;
		_threatThreshold = p_threshold;
	}

	public override NodeState RunNode(float p_delta)
	{
		List<Script_IEntity> entityList = _tree.GetBlackBoardElement<List<Script_IEntity>> (_entityListFlag);

		if (entityList.Count > 0) {
			foreach (Script_IEntity entity in entityList.ToList() ) {
				if (_threatReceiver.GetThreat (entity) > _threatThreshold) {
					return NodeState.Success;
				}

			}
		}
		return NodeState.Failed;
	}		

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

}
