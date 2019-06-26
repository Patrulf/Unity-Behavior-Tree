using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Namespace_Config;
using UnityEngine.Assertions;

public class Script_Enemy : Script_IEnemy {

	private Script_GameManager _manager;
	private Script_Grid _grid;
	private Vector3Int _gridLocation;
	private Vector3 _position;
	private Vector3 _rotation;
	private Vector3 _scale;
	private GameObject _gameObject;

	private Script_BehaviourTree _behaviourTree;

	private int _health;

	private Dictionary<Script_IFriendly, int> _threatDictionary;

	private blackboard_flags _locationFlag;
	private blackboard_flags _targetEntityFlag;
	private blackboard_flags _entityListFlag;
	private blackboard_flags _forwardSwingFlag;
	private blackboard_flags _getSpaceEntityListFlag;
	private blackboard_flags _selfFlag;

	private Script_IEntity _targetToAttack;

	private int _threatMultiplier;

	private Material _objectMaterial;

	private EntityStats _stats;

	private int _attackRange;
	private int _engageTargetRange;

	public Script_Enemy(Script_GameManager p_manager, Script_Grid p_grid, Vector3Int p_gridLocation)
	{
		_attackRange = 1;
		_engageTargetRange = 3;
		
		_threatMultiplier = 1;
		_stats = new EntityStats(10,0,100,1.5f);

		_targetToAttack = null;

		_threatDictionary = new Dictionary<Script_IFriendly,int> ();

		_health = _stats._maxHealth;


		_manager = p_manager;
		_grid = p_grid;

		_gridLocation = p_gridLocation;
		_position = p_gridLocation;

		_gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_gameObject.name = "EnemyObject";
		_objectMaterial = _gameObject.GetComponent<Renderer>().material;
		_objectMaterial.color = Color.red;
		_gameObject.transform.position = p_gridLocation;



		InitializeThreatList();


		InitializeBehaviourTree();

	}

	private void InitializeThreatList()
	{
		List<Script_IEntity> friendlies = _manager.GetFriendlies ();
		foreach (Script_IEntity entity in friendlies) {
			Script_IFriendly friendly = entity as Script_IFriendly;
			Assert.IsNotNull (friendly);
			_threatDictionary.Add (friendly, 0);
		}
	}

	private void InitializeBehaviourTree()
	{
		_behaviourTree = new Script_BehaviourTree ();
		SetupTreeFlags ();

		Script_Selector startSelector = new Script_Selector(_behaviourTree);

		Script_Sequence walkingSequence = new Script_Sequence(_behaviourTree);
		SetupWalkingSequence(walkingSequence);

		Script_Sequence pursuitSequence = new Script_Sequence (_behaviourTree);
		SetupPursuitSequence (pursuitSequence);

		Script_Sequence attackSequence = new Script_Sequence(_behaviourTree);
		SetupAttackSequence (attackSequence);

		Script_Sequence GetHighestThreatOrNearestSequence = new Script_Sequence(_behaviourTree);
		SetupGetHighestThreatOrNearestFriendlySequence (GetHighestThreatOrNearestSequence);

		Script_Sequence GetFreeSpaceSequence = new Script_Sequence (_behaviourTree);
		SetupGetFreeSpaceSequence (GetFreeSpaceSequence);

		startSelector.AddTask (GetFreeSpaceSequence);
		startSelector.AddTask (GetHighestThreatOrNearestSequence);
		startSelector.AddTask (pursuitSequence);
		startSelector.AddTask (attackSequence);
		startSelector.AddTask (walkingSequence); 

		_behaviourTree.AddChild(startSelector);

	}

	private void SetupWalkingSequence (Script_Sequence p_walkingSequence)
	{
		Script_Action_SetFlag setFriendlies = new Script_Action_SetFlag (_behaviourTree, _manager.GetFriendlies(), _entityListFlag);
		Script_Condition_IsEntityTypeNearby isEntityTypeNearby = new Script_Condition_IsEntityTypeNearby (_behaviourTree,_grid, _entityListFlag,this as Script_IEntity, _engageTargetRange);

		int walkRange = 1;

		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetRandomLocationWithinRange setRandomlocationAroundSelf = new Script_Action_SetRandomLocationWithinRange (_behaviourTree, _locationFlag, this as Script_IEntity, _grid, walkRange);
		Script_Action_MoveToLocation moveToLocationNode = new Script_Action_MoveToLocation(_behaviourTree, this as Script_IEntity, _locationFlag);
		Script_Action_OccupyTile occupyLocation = new Script_Action_OccupyTile (_behaviourTree, _grid, _locationFlag);


		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask (_behaviourTree, unoccupyTile);
		Script_LeafTask setRandomLocationAroundSelfLeaf = new Script_LeafTask(_behaviourTree,setRandomlocationAroundSelf);
		Script_LeafTask setFriendliesLeaf = new Script_LeafTask (_behaviourTree, setFriendlies);
		Script_LeafTask moveTowardsRandomLocationLeaf = new Script_LeafTask(_behaviourTree,moveToLocationNode);
		Script_LeafTask occupyLocationLeaf = new Script_LeafTask (_behaviourTree, occupyLocation);

		Script_LeafTask areFriendliesNearby = new Script_LeafTask (_behaviourTree, isEntityTypeNearby);

		Script_Decorator_Inverter areFriendliesNotNearbyDecorator = new Script_Decorator_Inverter (_behaviourTree, areFriendliesNearby);

		p_walkingSequence.AddTask(setFriendliesLeaf);
		p_walkingSequence.AddTask (unoccupyTileLeaf);
		p_walkingSequence.AddTask (setRandomLocationAroundSelfLeaf);
		p_walkingSequence.AddTask (occupyLocationLeaf);
		p_walkingSequence.AddTask (areFriendliesNotNearbyDecorator);
		p_walkingSequence.AddTask (moveTowardsRandomLocationLeaf);
	}

	private void SetupPursuitSequence(Script_Sequence p_pursuitSequence)
	{
		
		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree,_grid,_locationFlag);
		Script_Action_MoveTowardsEntity moveTowardsTarget = new Script_Action_MoveTowardsEntity (_behaviourTree, _grid, _targetEntityFlag, this as Script_IEntity);
		Script_Action_SetNearestLocationSurroundingEntity setLocationAroundTarget = new Script_Action_SetNearestLocationSurroundingEntity (_behaviourTree, _grid, _targetEntityFlag, this as Script_IEntity, _locationFlag);
		Script_Action_MoveToLocation moveToLocation = new Script_Action_MoveToLocation (_behaviourTree, this as Script_IEntity, _locationFlag);
		Script_Action_OccupyTile occupyTile = new Script_Action_OccupyTile(_behaviourTree, _grid, _locationFlag);

		Script_Condition_IsEntityNull isTargetNull = new Script_Condition_IsEntityNull (_behaviourTree, _targetEntityFlag);


		Script_Condition_IsEntityNearby isTargetAdjacent = new Script_Condition_IsEntityNearby(_behaviourTree,_grid,_targetEntityFlag, this as Script_IEntity, _attackRange);

		Script_LeafTask isTargetNullLeaf = new Script_LeafTask (_behaviourTree, isTargetNull);
		Script_LeafTask isTargetAdjacentLeaf = new Script_LeafTask (_behaviourTree, isTargetAdjacent);
		Script_LeafTask occupyTileLeaf = new Script_LeafTask (_behaviourTree,occupyTile);
		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask (_behaviourTree, unoccupyTile);
		Script_LeafTask moveTowardsTargetLeaf = new Script_LeafTask (_behaviourTree, moveTowardsTarget); 
		Script_LeafTask setLocationAroundTargetLeaf = new Script_LeafTask (_behaviourTree, setLocationAroundTarget);
		Script_LeafTask moveToLocationLeaf = new Script_LeafTask (_behaviourTree, moveToLocation);

		Script_Decorator_Inverter isTargetNotAdjacentDecorator = new Script_Decorator_Inverter (_behaviourTree,isTargetAdjacentLeaf);
		Script_Decorator_Inverter isTargetNotNullDecorator = new Script_Decorator_Inverter (_behaviourTree,isTargetNullLeaf);

		p_pursuitSequence.AddTask (isTargetNotNullDecorator);
		p_pursuitSequence.AddTask(isTargetNotAdjacentDecorator);
		p_pursuitSequence.AddTask (unoccupyTileLeaf);
		p_pursuitSequence.AddTask (moveTowardsTargetLeaf);
		p_pursuitSequence.AddTask (setLocationAroundTargetLeaf);
		p_pursuitSequence.AddTask (occupyTileLeaf);
		p_pursuitSequence.AddTask (moveToLocationLeaf);
	}

	private void SetupAttackSequence(Script_Sequence p_attackSequence)
	{

		Script_Condition_IsEntityNearby isTargetNearby = new Script_Condition_IsEntityNearby (_behaviourTree, _grid, _targetEntityFlag, this as Script_IEntity, _attackRange);
		Script_Action_SetFlag setForwardSwingFlag = new Script_Action_SetFlag (_behaviourTree, false, _forwardSwingFlag);
		Script_Action_MeeleAttack attackTarget = new Script_Action_MeeleAttack(_behaviourTree,_manager,_targetEntityFlag,_locationFlag,this as Script_IEntity,_forwardSwingFlag);

		Script_Condition_IsEntityNull isTargetNull = new Script_Condition_IsEntityNull (_behaviourTree, _targetEntityFlag);

		Script_LeafTask isTargetNullLeaf = new Script_LeafTask (_behaviourTree, isTargetNull);
		Script_Decorator_Inverter isTargetNotNullDecorator = new Script_Decorator_Inverter (_behaviourTree, isTargetNullLeaf);
		Script_LeafTask isTargetNearbyLeaf = new Script_LeafTask (_behaviourTree, isTargetNearby);
		Script_LeafTask setForwardSwingFlagLeaf = new Script_LeafTask (_behaviourTree, setForwardSwingFlag);
		Script_LeafTask attackTargetLeaf = new Script_LeafTask (_behaviourTree, attackTarget);


		p_attackSequence.AddTask(isTargetNotNullDecorator);
		p_attackSequence.AddTask (isTargetNearbyLeaf);
		p_attackSequence.AddTask (setForwardSwingFlagLeaf);
		p_attackSequence.AddTask (attackTargetLeaf);
	}

	private void SetupGetHighestThreatOrNearestFriendlySequence(Script_Sequence p_getHighestThreatOrNearestSequence)
	{
		int threatThreshold = 0;
		Script_Action_SetFlag setFriendlies = new Script_Action_SetFlag (_behaviourTree, _manager.GetFriendlies(), _entityListFlag);
		Script_Action_SetNearestEntityFromEntityWithinRange setNearestFriendly = new Script_Action_SetNearestEntityFromEntityWithinRange (_behaviourTree, _grid,_entityListFlag, _engageTargetRange, this as Script_IEntity , _targetEntityFlag);
		Script_Condition_IsThreatOverThreshold isThreatOfAnyFriendlyOverThreshold = new Script_Condition_IsThreatOverThreshold (_behaviourTree, _entityListFlag, this as Script_IEnemy, threatThreshold);
		Script_Action_SetHighestThreatTargetFromList setHighestThreatTarget = new Script_Action_SetHighestThreatTargetFromList (_behaviourTree, _entityListFlag, this as Script_IEnemy, _targetEntityFlag);
		Script_Action_SetTargetToAttack setFriendlyToAttack = new Script_Action_SetTargetToAttack(_behaviourTree,_targetEntityFlag,this as Script_IEntity);

		Script_LeafTask setFriendliesLeaf = new Script_LeafTask (_behaviourTree, setFriendlies);
		Script_LeafTask setNearestFriendlyLeaf = new Script_LeafTask(_behaviourTree,setNearestFriendly);
		Script_LeafTask setHighestThreatTargetIfOverThreshold = new Script_LeafTask (_behaviourTree,setHighestThreatTarget,isThreatOfAnyFriendlyOverThreshold);
		Script_LeafTask setFriendlyToAttackLeaf = new Script_LeafTask (_behaviourTree, setFriendlyToAttack);

		Script_Decorator_AlwaysFail setTargetToAttackAndFailDecorator = new Script_Decorator_AlwaysFail (_behaviourTree,setFriendlyToAttackLeaf);

		p_getHighestThreatOrNearestSequence.AddTask (setFriendliesLeaf);
		p_getHighestThreatOrNearestSequence.AddTask (setNearestFriendlyLeaf);
		p_getHighestThreatOrNearestSequence.AddTask (setFriendlyToAttackLeaf);
		p_getHighestThreatOrNearestSequence.AddTask (setHighestThreatTargetIfOverThreshold);
		p_getHighestThreatOrNearestSequence.AddTask (setTargetToAttackAndFailDecorator);
	}

	private void SetupGetFreeSpaceSequence(Script_Sequence p_getFreeSpaceSequence)
	{
		int range = 1;
		Script_Action_SetFlag setFriendlies = new Script_Action_SetFlag (_behaviourTree, _manager.GetFriendlies(), _getSpaceEntityListFlag);
		Script_Action_SetEntitiesWithinRange setFriendliesWithinRange = new Script_Action_SetEntitiesWithinRange(_behaviourTree,_grid,_getSpaceEntityListFlag,_getSpaceEntityListFlag,this as Script_IEntity,range);
		Script_Condition_AreAnyEntitiesAtEntityLocation isFriendlyAtMyLocation = new Script_Condition_AreAnyEntitiesAtEntityLocation(_behaviourTree,_getSpaceEntityListFlag,this as Script_IEntity);
		Script_Action_SetFlag setFlagToSelf = new Script_Action_SetFlag(_behaviourTree,this as Script_IEntity,_selfFlag);

		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetNearestLocationSurroundingEntity setLocationAroundSelf = new Script_Action_SetNearestLocationSurroundingEntity (_behaviourTree, _grid, _selfFlag, this as Script_IEntity, _locationFlag);
		Script_Action_OccupyTile occupyTile = new Script_Action_OccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_MoveToLocation moveToLocation = new Script_Action_MoveToLocation (_behaviourTree, this as Script_IEntity, _locationFlag);


		Script_LeafTask setFriendliesLeaf = new Script_LeafTask (_behaviourTree, setFriendlies);
		Script_LeafTask setFriendliesWithinRangeLeaf = new Script_LeafTask (_behaviourTree, setFriendliesWithinRange);
		Script_LeafTask isFriendlyAtMyLocationLeaf = new Script_LeafTask (_behaviourTree, isFriendlyAtMyLocation);
		Script_LeafTask setFlagToSelfLeaf = new Script_LeafTask (_behaviourTree, setFlagToSelf);
		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask(_behaviourTree,unoccupyTile);
		Script_LeafTask setLocationAroundSelfLeaf = new Script_LeafTask (_behaviourTree, setLocationAroundSelf);
		Script_LeafTask occupyTileLeaf = new Script_LeafTask(_behaviourTree,occupyTile);
		Script_LeafTask moveToLocationLeaf = new Script_LeafTask (_behaviourTree, moveToLocation);

		Script_Decorator_NeverSucceed moveToLocationAndNeverSucceedDecorator = new Script_Decorator_NeverSucceed (_behaviourTree, moveToLocationLeaf);

		p_getFreeSpaceSequence.AddTask (setFriendliesLeaf);
		p_getFreeSpaceSequence.AddTask (setFriendliesWithinRangeLeaf);
		p_getFreeSpaceSequence.AddTask (isFriendlyAtMyLocationLeaf);
		p_getFreeSpaceSequence.AddTask (setFlagToSelfLeaf);
		p_getFreeSpaceSequence.AddTask (unoccupyTileLeaf);
		p_getFreeSpaceSequence.AddTask (setLocationAroundSelfLeaf);
		p_getFreeSpaceSequence.AddTask (occupyTileLeaf);
		p_getFreeSpaceSequence.AddTask (moveToLocationAndNeverSucceedDecorator);
	}

	private void SetupTreeFlags()
	{
		_locationFlag = blackboard_flags.flagOne;
		_targetEntityFlag = blackboard_flags.flagTwo;
		_entityListFlag = blackboard_flags.flagThree;
		_forwardSwingFlag = blackboard_flags.flagFour;
		_getSpaceEntityListFlag = blackboard_flags.flagSix;
		_selfFlag = blackboard_flags.flagSeven;

		int startPosX = Mathf.RoundToInt(_gameObject.transform.position.x);
		int startPosY = Mathf.RoundToInt(_gameObject.transform.position.y);
		int startPosZ = Mathf.RoundToInt(_gameObject.transform.position.z);
		Vector3Int startPos = new Vector3Int(startPosX,startPosY,startPosZ);
		_behaviourTree.SetBlackboardElement (_locationFlag, startPos);

	}


	public override void Heal (int p_amountToHeal)
	{
		_health = Mathf.Min (_health + p_amountToHeal, _stats._maxHealth);
	}


	public override GameObject GetGameObject()
	{
		return _gameObject;
	}


	public override void Update(float p_delta)
	{
		_behaviourTree.RunBehaviourTree (p_delta);
		UpdatePosition ();
		UpdateGridLocation ();
	}

	private void UpdatePosition()
	{
		_position = _gameObject.transform.position;
	}

	private void UpdateGridLocation()
	{
		int x = Mathf.RoundToInt(_gameObject.transform.position.x);
		int y = Mathf.RoundToInt (_gameObject.transform.position.y);
		int z = Mathf.RoundToInt (_gameObject.transform.position.z);
		_gridLocation = new Vector3Int(x,y,z);
	}


	public override Vector3Int GetGridLocation()
	{
		return _gridLocation;
	}

	public override Vector3 GetPosition()
	{
		return _position;
	}

	public override void DecreaseHealth(int p_valueToDecrease)
	{
		_health -= p_valueToDecrease;
		_health = Mathf.Clamp (_health, 0, _stats._maxHealth);
	}

	public override int GetHealth()
	{
		return _health;
	}

	public override void DetermineIfDead()
	{
		if (_health <= 0) {

			_manager.DestroyMaterial (_objectMaterial);
			_manager.DestroyGameObject (_gameObject);
			_manager.RemoveEnemy(this);

			Vector3Int pos = _behaviourTree.GetBlackBoardElement<Vector3Int> (_locationFlag);
			_grid.AccessGridTile (pos.x, pos.z).SetOccupied (false);

			_behaviourTree.RemoveBlackboardElement (_locationFlag);
			_behaviourTree.RemoveBlackboardElement(_targetEntityFlag);
			_behaviourTree.RemoveBlackboardElement (_entityListFlag);
			_behaviourTree.RemoveBlackboardElement (_forwardSwingFlag);
			_behaviourTree.RemoveBlackboardElement (_getSpaceEntityListFlag);

			_behaviourTree = null;

		}
	}

	public override void SetTargetToAttack(Script_IEntity p_targetToAttack)
	{
		_targetToAttack = p_targetToAttack;
	}


	public override Script_IEntity GetTargetToAttack()
	{
		return _targetToAttack;
	}


	public override void TakeDamage(Script_IEntity p_attacker, int p_damage)
	{
		Script_IFriendly attacker = p_attacker as Script_IFriendly;
		Assert.IsNotNull (attacker);
		int threatMultiplier = attacker.GetThreatMultiplier ();
		if (!_threatDictionary.ContainsKey (attacker)) {
			_threatDictionary.Add (attacker, p_damage*threatMultiplier);
		} else {
			int threat = _threatDictionary[attacker] + p_damage*threatMultiplier;
			_threatDictionary [attacker] = threat;
		}
		_health -= p_damage;
	}


	public override int GetThreat(Script_IEntity p_entity)
	{
		Script_IFriendly friendly = p_entity as Script_IFriendly;
		Assert.IsNotNull (friendly);
		return _threatDictionary [friendly];
	}

	public override int GetThreatMultiplier()
	{
		return _threatMultiplier;
	}

	public override EntityStats GetStats()
	{
		return _stats;
	}

}
