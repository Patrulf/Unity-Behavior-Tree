using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;


public class Script_Action_MoveToLocation : Script_Action {
	
	private Vector3 _location;
	private Script_IEntity _entity;
	private Script_BehaviourTree _tree;
	private blackboard_flags _locationFlag;

	public Script_Action_MoveToLocation(Script_BehaviourTree p_tree, Script_IEntity p_entity, blackboard_flags p_locationFlag)
	{
		_locationFlag = p_locationFlag;
		_location = Vector3.zero;
		_entity = p_entity;
		_tree = p_tree;
	}

	public override NodeState RunNode (float p_delta)
	{
		_location = _tree.GetBlackBoardElement<Vector3Int>(_locationFlag);

		GameObject entObject = _entity.GetGameObject ();
		Vector3 entityPosition = entObject.transform.position;
		Vector3 entityDirection =  (_location - entObject.transform.position).normalized; 
		Vector3 nextStep = entObject.transform.position + (entityDirection * Time.deltaTime);


		if (Vector3.Distance(nextStep, _location) >= Vector3.Distance(entityPosition,_location)) {
			entObject.transform.position = _location;

			return NodeState.Success;

		}

		if (Vector3.Distance(nextStep, _location) < Vector3.Distance(entityPosition,_location)) {
			_entity.GetGameObject ().transform.position += entityDirection * Time.deltaTime;

			return NodeState.Running;
		}
		return NodeState.Failed;
	}

	public override Script_BehaviourTree GetTree ()
	{
		return _tree;
	}


}
