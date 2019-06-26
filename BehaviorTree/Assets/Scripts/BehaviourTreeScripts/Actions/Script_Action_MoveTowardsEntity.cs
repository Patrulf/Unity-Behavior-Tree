using System.Collections.Generic;
using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;

public class Script_Action_MoveTowardsEntity : Script_Action {

	private Script_BehaviourTree _tree;
	private Script_Grid _grid;

	private blackboard_flags _entityFlag;
	private Script_IEntity _targetEntity;
	private Script_IEntity _entityToMove;

	public Script_Action_MoveTowardsEntity(Script_BehaviourTree p_tree, Script_Grid p_grid, blackboard_flags p_entityFlag, Script_IEntity p_entityToMove)
	{
		_tree = p_tree;
		_grid = p_grid;
		_entityFlag = p_entityFlag;
		_entityToMove = p_entityToMove;

	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{

		if (_entityToMove == null) {
			return NodeState.Failed;
		}

		_targetEntity = _tree.GetBlackBoardElement<Script_IEntity>(_entityFlag); 

		if (_targetEntity == null || _targetEntity.GetGameObject() == null) {
			return NodeState.Failed;
		}
			

		int maxRange = _grid.GetWidth() > _grid.GetHeight() ? _grid.GetWidth() : _grid.GetHeight();
		int currentRange = 1;

		List<Script_Tile> availableTiles = new List<Script_Tile> ();
		while (currentRange <= maxRange && availableTiles.Count == 0) {
			availableTiles = GetAvailableTilesSurroundingEntity (currentRange, _targetEntity);
		}

		if (availableTiles.Count == 0)
			return NodeState.Failed;

		foreach (Script_Tile tile in availableTiles)
		{
			if (_entityToMove.GetGridLocation () == tile.GetGridPosition () || _entityToMove.GetGridLocation() == _targetEntity.GetGridLocation() ) {
				return NodeState.Success;
			}
		} 


		GameObject entityObject = _entityToMove.GetGameObject ();
		GameObject targetEntityObject = _targetEntity.GetGameObject ();

		entityObject.transform.position += (targetEntityObject.transform.position - entityObject.transform.position).normalized * p_delta;
		return NodeState.Running;
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
					surroundingTiles.Add (_grid.AccessGridTile (x, z));
			}
		}
		return surroundingTiles;
	}


}
