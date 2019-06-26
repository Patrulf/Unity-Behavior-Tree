using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Condition_IsEntityNearby : Script_Condition {

	private Script_BehaviourTree _tree;
	private blackboard_flags _entityFlag;

	private int _range;

	private Script_Grid _grid;

	private Script_IEntity _measuredFromEntity;
	private Vector3Int _location;

	public Script_Condition_IsEntityNearby(Script_BehaviourTree p_tree,Script_Grid p_grid, blackboard_flags p_entityFlag,Script_IEntity p_measuredFromEntity, int p_range)
	{
		_tree = p_tree;
		_entityFlag = p_entityFlag;
		_grid = p_grid;

		_measuredFromEntity = p_measuredFromEntity;

		_range = p_range;
	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{
		Script_IEntity entity = _tree.GetBlackBoardElement<Script_IEntity> (_entityFlag);

		if (entity == null) {
			return NodeState.Failed;
		}

		if (_measuredFromEntity == null)
			return NodeState.Failed;


		_location = _measuredFromEntity.GetGridLocation ();


		if (IsEntityWithinRange(_range,entity) ) {
			return NodeState.Success;
		}

		return NodeState.Failed;
	}



	private bool IsEntityWithinRange(int p_range, Script_IEntity p_entity)
	{
		
		int xCurrent = (int)_location.x;
		int zCurrent = (int)_location.z;

		int xMin = xCurrent;
		int xMax = xCurrent;
		int zMin = zCurrent;
		int zMax = zCurrent;

		for (int x = xCurrent - p_range; x <= xCurrent; x++) {
			if (x < 0)
				continue;

			xMin = x;
			break;
		}
		for (int x = xCurrent + p_range; x >= xCurrent; x--) {
			if (x >= _grid.GetWidth ())
				continue;

			xMax = x;
			break;
		}
		for (int z = zCurrent - p_range; z <= zCurrent; z++) {
			if (z < 0)
				continue;

			zMin = z;
			break;
		}
		for (int z = zCurrent + p_range; z >= zCurrent; z--) {
			if (z >= _grid.GetHeight ())
				continue;

			zMax = z;
			break;
		}

		for (int z = zMin; z <= zMax; z++) {
			for (int x = xMin; x <= xMax; x++) {
					if (p_entity.GetGridLocation ().x == x && p_entity.GetGridLocation ().z == z) {
					return true;
				}
			}
		}

		return false;


	}
}
