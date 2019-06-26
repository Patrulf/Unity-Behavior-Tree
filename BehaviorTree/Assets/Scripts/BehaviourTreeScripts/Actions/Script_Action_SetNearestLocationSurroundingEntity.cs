using System.Collections.Generic;
using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;
using System.Linq;


public class Script_Action_SetNearestLocationSurroundingEntity : Script_Action {

	private Script_BehaviourTree _tree;
	private Script_Grid _grid;
	private Script_IEntity _targetEntity;
	private Script_IEntity _fromEntity;
	private blackboard_flags _locationToMoveTowardsFlag;
	private blackboard_flags _entityToGetLocationAround;


	public Script_Action_SetNearestLocationSurroundingEntity(Script_BehaviourTree p_tree, Script_Grid p_grid, blackboard_flags p_entityToGetLocationAround, Script_IEntity p_fromEntity, blackboard_flags p_locationToMoveTowardsFlag)
	{
		_tree = p_tree;
		_grid = p_grid;
		_fromEntity = p_fromEntity;
		_locationToMoveTowardsFlag = p_locationToMoveTowardsFlag;
		_entityToGetLocationAround = p_entityToGetLocationAround;	
	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{
		_targetEntity = _tree.GetBlackBoardElement<Script_IEntity>(_entityToGetLocationAround);

		if (_targetEntity == null) {
			return NodeState.Failed;
		}

		if (_fromEntity == null) {
			return NodeState.Failed;
		}


		int maxRange = _grid.GetWidth() > _grid.GetHeight() ? _grid.GetWidth() : _grid.GetHeight();
		int currentRange = 1;

		List<Script_Tile> availableTiles = new List<Script_Tile> ();

		while (currentRange <= maxRange && availableTiles.Count == 0) {
			availableTiles = GetAvailableTilesSurroundingEntity (currentRange, _targetEntity);
			currentRange++;
		}

		if (availableTiles.Count == 0)
			return NodeState.Failed;

		Script_Tile tileToMoveTowards = GetNearestAvailableTile (availableTiles);
		Vector3Int location = tileToMoveTowards.GetGridPosition ();

		_tree.SetBlackboardElement (_locationToMoveTowardsFlag,location);

		return NodeState.Success;

	}

	private Script_Tile GetNearestAvailableTile(List<Script_Tile> p_tileList)
	{
		float distance = float.MaxValue;
		Script_Tile nearestTile = null;

		foreach (Script_Tile tile in p_tileList) {
			if (distance > Vector3Int.Distance(tile.GetGridPosition(),_fromEntity.GetGridLocation() )) {
				distance = Vector3Int.Distance(tile.GetGridPosition(), _fromEntity.GetGridLocation());
				nearestTile = tile;
			}
		}
		return nearestTile;
	}


	private List<Script_Tile> GetAvailableTilesSurroundingEntity(int p_range, Script_IEntity p_entity)
	{

		List<Script_Tile> surroundingTiles = new List<Script_Tile>();

		int xCurrent = p_entity.GetGridLocation().x;
		int zCurrent = p_entity.GetGridLocation().z;

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
				if (!_grid.AccessGridTile (x, z).GetOccupied() && z != _targetEntity.GetGridLocation().z && x != _targetEntity.GetGridLocation().x ) {
					surroundingTiles.Add (_grid.AccessGridTile (x, z));

				}
			}
		}
		return surroundingTiles;
	}



}
