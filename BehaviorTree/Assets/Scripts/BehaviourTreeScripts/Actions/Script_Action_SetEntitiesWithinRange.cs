using System.Collections.Generic;
using Namespace_NodeState;
using Namespace_Config;
using System.Linq;

public class Script_Action_SetEntitiesWithinRange : Script_Action {

	private Script_BehaviourTree _tree;
	private blackboard_flags _listOfEntitiesFlag;
	private blackboard_flags _listOfEntitiesWithinRangeFlag;

	private Script_IEntity _entityToCalculateRangeFrom;
	private Script_Grid _grid;

	private int _range;

	public Script_Action_SetEntitiesWithinRange(Script_BehaviourTree p_tree, Script_Grid p_grid, blackboard_flags p_listOfEntitiesFlag,
		blackboard_flags p_listOfEntitiesWithinRangeFlag, Script_IEntity p_entityToCalculateRangeFrom, int p_range)
	{
		_tree = p_tree;
		_range = p_range;
		_entityToCalculateRangeFrom = p_entityToCalculateRangeFrom;
		_listOfEntitiesFlag = p_listOfEntitiesFlag;
		_listOfEntitiesWithinRangeFlag = p_listOfEntitiesWithinRangeFlag;
		_grid = p_grid;
	}

	public override NodeState RunNode(float p_delta)
	{
		List<Script_IEntity> listOfEntities = _tree.GetBlackBoardElement<List<Script_IEntity>> (_listOfEntitiesFlag);

		if (listOfEntities.Count == 0 || listOfEntities == null) {

			return NodeState.Failed;
		}

		List<Script_IEntity> entitiesWithinRange = GetEntitiesWithinRange (_range, listOfEntities);

		if (entitiesWithinRange.Count == 0 || entitiesWithinRange == null) {
			return NodeState.Failed;
		}			

		_tree.SetBlackboardElement (_listOfEntitiesWithinRangeFlag, entitiesWithinRange);
		return NodeState.Success;



	}

	private List<Script_IEntity> GetEntitiesWithinRange(int p_range, List<Script_IEntity> p_listOfEntities)
	{
		List<Script_IEntity> entitiesWithinRange = new List<Script_IEntity>();
		int xCurrent = _entityToCalculateRangeFrom.GetGridLocation ().x;
		int zCurrent = _entityToCalculateRangeFrom.GetGridLocation ().z;

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



	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}


}
