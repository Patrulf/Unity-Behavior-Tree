using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Condition_IsEntityAtLocation : Script_Condition {
	
	private Script_BehaviourTree _tree;
	private blackboard_flags _locationFlag;
	private Script_IEntity _entity;


	public Script_Condition_IsEntityAtLocation(Script_BehaviourTree p_tree, blackboard_flags p_locationFlag, Script_IEntity p_entity)
	{
		_entity = p_entity;
		_tree = p_tree;
		_locationFlag = p_locationFlag;
	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{
		GameObject entObject = _entity.GetGameObject ();
		Vector3 entityPosition = entObject.transform.position;

		if (_tree.GetBlackBoardElement<Vector3> (_locationFlag) == entityPosition) {
			return NodeState.Success;
		}
		return NodeState.Failed;
	}

}
