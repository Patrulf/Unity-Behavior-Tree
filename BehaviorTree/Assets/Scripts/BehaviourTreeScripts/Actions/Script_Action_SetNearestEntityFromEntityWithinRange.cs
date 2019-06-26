using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Namespace_NodeState;
using Namespace_Config;


public class Script_Action_SetNearestEntityFromEntityWithinRange : Script_Action {

	private Script_BehaviourTree _tree;
	private blackboard_flags _entityListFlag;
	private int _range;
	private blackboard_flags _locationFlag;
	private Script_Grid _grid;
	private blackboard_flags _returnedNearestEntityFlag;

	private Vector3Int _position;
	private Script_IEntity _fromEntity;

	public Script_Action_SetNearestEntityFromEntityWithinRange(Script_BehaviourTree p_tree,Script_Grid p_grid, blackboard_flags p_entityListFlag, int p_range, Script_IEntity p_fromEntity, blackboard_flags p_returnedEntityFlag)
	{
		_grid = p_grid;
		_tree = p_tree;
		_entityListFlag = p_entityListFlag;
		_range = p_range;
		_fromEntity = p_fromEntity;
		_returnedNearestEntityFlag = p_returnedEntityFlag;


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
			
		if (_fromEntity == null)
			return NodeState.Failed;


		_position = _fromEntity.GetGridLocation ();

		List<Script_IEntity> entitiesWithinRange = GetEntitiesWithinRange (_range, entityList);

		if (entitiesWithinRange != null && entitiesWithinRange.Count > 0) {
			Script_IEntity nearestEntity = GetNearestEntity (entitiesWithinRange);
			_tree.SetBlackboardElement (_returnedNearestEntityFlag, nearestEntity);
			return NodeState.Success;
		}

		return NodeState.Failed;
	}
		
	private Script_IEntity GetNearestEntity(List<Script_IEntity> p_listToCheck)
	{
		float range = float.MaxValue;
		Script_IEntity nearestEntity = null;

		foreach (Script_IEntity entity in p_listToCheck) {
			if (Vector3Int.Distance (entity.GetGridLocation(), _position) < range) {
				range = Vector3Int.Distance (entity.GetGridLocation (), _position);
				nearestEntity = entity;
			}
		}

		return nearestEntity;

	}



	private List<Script_IEntity> GetEntitiesWithinRange(int p_range, List<Script_IEntity> p_listOfEntities)
	{
		List<Script_IEntity> entitiesWithinRange = new List<Script_IEntity>();
		int xCurrent = (int)_position.x;
		int zCurrent = (int)_position.z;

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
