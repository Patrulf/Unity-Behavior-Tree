using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Action_SetRandomLocationWithinRange : Script_Action {

	private Vector3Int _location;
	private Script_IEntity _entity;
	private Script_BehaviourTree _tree;
	private blackboard_flags _locationFlag;
	private Script_Grid _grid;
	private int _range;


	public Script_Action_SetRandomLocationWithinRange(Script_BehaviourTree p_tree, blackboard_flags p_locationFlag, Script_IEntity p_entityToCalculateFrom, Script_Grid p_grid, int p_range)
	{
		_tree = p_tree;
		_entity = p_entityToCalculateFrom;
		_locationFlag = p_locationFlag;
		_grid = p_grid;
		_range = p_range;
	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode (float p_delta)
	{
		Vector3Int gridPos = _entity.GetGridLocation ();

		int x = Random.Range (gridPos.x - _range, gridPos.x + _range + 1);
		int z = Random.Range (gridPos.z - _range, gridPos.z + _range + 1);

		x = Mathf.Min(x,_grid.GetWidth()-1);
		x = Mathf.Max(x,0);

		z = Mathf.Min (z, _grid.GetHeight () - 1);
		z = Mathf.Max (z, 0);

		if (_grid.AccessGridTile (x, z).GetOccupied ()) {
			return NodeState.Running;
		}

		_location = new Vector3Int (x, 0, z);

		_tree.SetBlackboardElement(_locationFlag, _location);
		return NodeState.Success;

	}

}
