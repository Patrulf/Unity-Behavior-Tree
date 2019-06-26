using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Action_UnoccupyTile : Script_Action{

	private Script_BehaviourTree _tree;
	private Script_Grid _grid;
	private blackboard_flags _flag;
	private Script_Tile _tile;

	private Vector3Int _position;

	public Script_Action_UnoccupyTile(Script_BehaviourTree p_tree, Script_Grid p_grid, blackboard_flags p_flag)
	{
		_tree = p_tree;
		_grid = p_grid;
		_flag = p_flag;
	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{
		_tile = _grid.AccessGridTile(_tree.GetBlackBoardElement<Vector3Int>(_flag).x,_tree.GetBlackBoardElement<Vector3Int>(_flag).z);
		_tile.SetOccupied (false);
		return NodeState.Success;
	}


}
