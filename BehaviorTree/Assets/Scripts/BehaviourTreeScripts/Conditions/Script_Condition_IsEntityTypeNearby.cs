using System.Collections.Generic;
using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;
using System.Linq;


public class Script_Condition_IsEntityTypeNearby : Script_Condition {

	private Script_BehaviourTree _tree;
	private blackboard_flags _entityListFlag;

	private int _range;

	private Script_Grid _grid;

	private Script_IEntity _targetEntity;
	private Vector3Int _location;

	public Script_Condition_IsEntityTypeNearby (Script_BehaviourTree p_tree,Script_Grid p_grid, blackboard_flags p_entityListFlag,Script_IEntity p_targetEntity, int p_range)
	{
		_tree = p_tree;
		_entityListFlag = p_entityListFlag;
		_grid = p_grid;

		_targetEntity = p_targetEntity;

		_range = p_range;

	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{
		List<Script_IEntity> entityList = _tree.GetBlackBoardElement<List<Script_IEntity>> (_entityListFlag);

		if (entityList.Count <= 0 || entityList == null) {
			return NodeState.Failed;
		}

		if (_targetEntity == null)
			return NodeState.Failed;


		_location = _targetEntity.GetGridLocation ();

		List<Script_IEntity> entitiesWithinRange = GetEntitiesWithinRange (_range, entityList);

		if (entitiesWithinRange.Count > 0) {
			return NodeState.Success;
		}

		return NodeState.Failed;
	}



	private List<Script_IEntity> GetEntitiesWithinRange(int p_range, List<Script_IEntity> p_listOfEntities)
	{
		List<Script_IEntity> entitiesWithinRange = new List<Script_IEntity>();
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
				foreach (Script_IEntity entity in p_listOfEntities.ToList()) {
					if (entity.GetGridLocation ().x == x && entity.GetGridLocation ().z == z) {
						entitiesWithinRange.Add (entity);
					}
				}
			}
		}

		return entitiesWithinRange;


	}



}
