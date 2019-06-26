using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Namespace_Config;
using UnityEngine.Assertions;

public class Script_DamageDealer : Script_IFriendly {
	private Script_GameManager _manager;
	private Script_Grid _grid;
	private Vector3Int _gridLocation;
	private Vector3 _position;
	private Vector3 _rotation;
	private Vector3 _scale;
	private GameObject _gameObject;

	private Script_BehaviourTree _behaviourTree;

	private int _health;

	private blackboard_flags _locationFlag; 
	private blackboard_flags _targetEntityFlag;
	private blackboard_flags _entityListFlag;
	private blackboard_flags _forwardSwingFlag;
	private blackboard_flags _entityToFollowFlag;
	private blackboard_flags _getSpaceEntityListFlag;
	private blackboard_flags _selfFlag;

	private int _threatMultiplier;

	private Material _objectMaterial;

	private EntityStats _stats;

	private int _attackRange;
	private int _engageTargetRange;


	public Script_DamageDealer(Script_GameManager p_manager, Script_Grid p_grid, Vector3Int p_gridLocation)
	{
		_attackRange = 3;
		_engageTargetRange = 3;

		_threatMultiplier = 1;
		_stats = new EntityStats(10,0,100,1.5f);

		_health = _stats._maxHealth;

		_manager = p_manager;
		_grid = p_grid;

		_gridLocation = p_gridLocation;
		_position = p_gridLocation;

		_gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_gameObject.name = "DamageDealerObject";
		_objectMaterial = _gameObject.GetComponent<Renderer>().material;
		_objectMaterial.color = Color.cyan;
		_gameObject.transform.position = p_gridLocation;

	}

	public void InitializeBehaviourTree()
	{
		_behaviourTree = new Script_BehaviourTree ();
		SetupTreeFlags ();

		Script_Selector startSelector = new Script_Selector(_behaviourTree);

		Script_Sequence walkingSequence = new Script_Sequence(_behaviourTree);
		SetupWalkingSequence(walkingSequence);

		Script_Sequence moveNearTankSequence = new Script_Sequence (_behaviourTree);
		SetupMoveToTankSequence (moveNearTankSequence);

		Script_Sequence attackSequence = new Script_Sequence(_behaviourTree);
		SetupAttackSequence (attackSequence);

		Script_Sequence ChooseTargetSequence = new Script_Sequence (_behaviourTree);
		SetupChooseTargetToAttackSequence (ChooseTargetSequence);

		Script_Sequence GetFreeSpaceSequence = new Script_Sequence (_behaviourTree);
		SetupGetFreeSpaceSequence (GetFreeSpaceSequence);


		startSelector.AddTask(GetFreeSpaceSequence);
		startSelector.AddTask(ChooseTargetSequence);
		startSelector.AddTask(attackSequence);
		startSelector.AddTask(moveNearTankSequence);
		startSelector.AddTask(walkingSequence);

		_behaviourTree.AddChild (startSelector);

	}

	private void SetupWalkingSequence(Script_Sequence p_walkingSequence)
	{
		Script_Action_SetFlag setEnemies = new Script_Action_SetFlag (_behaviourTree, _manager.GetEnemies(), _entityListFlag);
		Script_Condition_IsEntityTypeNearby isEntityTypeNearby = new Script_Condition_IsEntityTypeNearby (_behaviourTree,_grid, _entityListFlag,this as Script_IEntity, _engageTargetRange);

		int walkRange = 1;

		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetRandomLocationWithinRange setRandomLocationAroundSelf = new Script_Action_SetRandomLocationWithinRange (_behaviourTree, _locationFlag, this as Script_IEntity, _grid, walkRange);
		Script_Action_MoveToLocation moveToLocation = new Script_Action_MoveToLocation(_behaviourTree, this as Script_IEntity, _locationFlag);
		Script_Action_OccupyTile occupyLocation = new Script_Action_OccupyTile (_behaviourTree, _grid, _locationFlag);

		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask (_behaviourTree, unoccupyTile);
		Script_LeafTask setRandomLocationAroundSelfLeaf = new Script_LeafTask(_behaviourTree,setRandomLocationAroundSelf);
		Script_LeafTask setEnemiesLeaf = new Script_LeafTask (_behaviourTree, setEnemies);
		Script_LeafTask moveToLocationLeaf = new Script_LeafTask(_behaviourTree,moveToLocation);
		Script_LeafTask occupyLocationLeaf = new Script_LeafTask (_behaviourTree, occupyLocation);

		Script_LeafTask areEnemiesNearby = new Script_LeafTask (_behaviourTree, isEntityTypeNearby);

		Script_Decorator_Inverter areEnemiesNotNearbyDecorator = new Script_Decorator_Inverter (_behaviourTree, areEnemiesNearby);

		p_walkingSequence.AddTask(setEnemiesLeaf);
		p_walkingSequence.AddTask (unoccupyTileLeaf);
		p_walkingSequence.AddTask (setRandomLocationAroundSelfLeaf);
		p_walkingSequence.AddTask (occupyLocationLeaf);
		p_walkingSequence.AddTask (areEnemiesNotNearbyDecorator);
		p_walkingSequence.AddTask (moveToLocationLeaf);
	}

	private void SetupMoveToTankSequence(Script_Sequence p_moveNearTankSequence)
	{
		Script_Action_SetFlag setTankFlag = new Script_Action_SetFlag(_behaviourTree, _manager.GetTank() as Script_IEntity, _entityToFollowFlag ); 
		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetNearestLocationSurroundingEntity setLocationNearTank = new Script_Action_SetNearestLocationSurroundingEntity(_behaviourTree,_grid,_entityToFollowFlag,this as Script_IEntity,_locationFlag);
		Script_Action_OccupyTile occupyTile = new Script_Action_OccupyTile(_behaviourTree,_grid, _locationFlag);
		Script_Action_MoveToLocation moveToLocation = new Script_Action_MoveToLocation (_behaviourTree, this as Script_IEntity, _locationFlag);

		Script_Condition_IsEntityNull isTankNull = new Script_Condition_IsEntityNull(_behaviourTree,_entityToFollowFlag); 

		Script_LeafTask setTankFlagLeaf = new Script_LeafTask(_behaviourTree, setTankFlag);
		Script_LeafTask isTankNullLeaf = new Script_LeafTask(_behaviourTree, isTankNull);
		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask(_behaviourTree, unoccupyTile);
		Script_LeafTask setLocationNearTankLeaf = new Script_LeafTask (_behaviourTree,setLocationNearTank);
		Script_LeafTask occupyTileLeaf = new Script_LeafTask (_behaviourTree, occupyTile);
		Script_LeafTask moveToLocationLeaf = new Script_LeafTask (_behaviourTree, moveToLocation);

		Script_Decorator_Inverter isTankNotNullDecorator = new Script_Decorator_Inverter (_behaviourTree,isTankNullLeaf);

		p_moveNearTankSequence.AddTask (setTankFlagLeaf);
		p_moveNearTankSequence.AddTask(isTankNotNullDecorator);
		p_moveNearTankSequence.AddTask (unoccupyTileLeaf);
		p_moveNearTankSequence.AddTask (setLocationNearTankLeaf);
		p_moveNearTankSequence.AddTask (occupyTileLeaf);
		p_moveNearTankSequence.AddTask (moveToLocationLeaf);
	}

	private void SetupAttackSequence(Script_Sequence p_attackSequence)
	{
		Script_Condition_IsEntityNearby isTargetNearby = new Script_Condition_IsEntityNearby (_behaviourTree, _grid, _targetEntityFlag, this as Script_IEntity, _attackRange);
		Script_Action_SetFlag forwardSwingFlag = new Script_Action_SetFlag (_behaviourTree, false, _forwardSwingFlag);
		Script_Action_RangedAttack attackEnemy = new Script_Action_RangedAttack(_behaviourTree,_manager,_targetEntityFlag,_locationFlag,this as Script_IEntity,_forwardSwingFlag,Color.cyan);

		Script_Condition_IsEntityNull isTargetNull = new Script_Condition_IsEntityNull (_behaviourTree, _targetEntityFlag);

		Script_LeafTask isTargetNullLeaf = new Script_LeafTask (_behaviourTree, isTargetNull);
		Script_Decorator_Inverter IsTargetNotNullDecorator = new Script_Decorator_Inverter (_behaviourTree, isTargetNullLeaf);
		Script_LeafTask isTargetNearbyLeaf = new Script_LeafTask (_behaviourTree, isTargetNearby);
		Script_LeafTask setForwardSwingFlagLeaf = new Script_LeafTask (_behaviourTree, forwardSwingFlag);
		Script_LeafTask attackTargetLeaf = new Script_LeafTask (_behaviourTree, attackEnemy);

		p_attackSequence.AddTask(IsTargetNotNullDecorator);
		p_attackSequence.AddTask (isTargetNearbyLeaf);
		p_attackSequence.AddTask (setForwardSwingFlagLeaf);
		p_attackSequence.AddTask (attackTargetLeaf);
	}

	private void SetupChooseTargetToAttackSequence(Script_Sequence p_chooseTargetSequence)
	{
		Script_Action_SetFlag setEnemies = new Script_Action_SetFlag (_behaviourTree, _manager.GetEnemies(), _entityListFlag);
		Script_Action_SetEntitiesWithinRange setEnemiesWithinRange = new Script_Action_SetEntitiesWithinRange(_behaviourTree, _grid,_entityListFlag,_entityListFlag,this as Script_IEntity,_attackRange);
		Script_Action_SetEntityWithLowestHealth setEnemyWithLowestHealth = new Script_Action_SetEntityWithLowestHealth(_behaviourTree, _entityListFlag, _targetEntityFlag);

		Script_LeafTask setEnemiesLeaf = new Script_LeafTask (_behaviourTree, setEnemies);
		Script_LeafTask setEnemiesWithinRangeLeaf = new Script_LeafTask (_behaviourTree, setEnemiesWithinRange);
		Script_LeafTask setEnemyWithLowestHealthLeaf = new Script_LeafTask (_behaviourTree,setEnemyWithLowestHealth);

		Script_Decorator_AlwaysFail setEnemyWithLowestHealthAndFailDecorator = new Script_Decorator_AlwaysFail (_behaviourTree, setEnemyWithLowestHealthLeaf);

		p_chooseTargetSequence.AddTask (setEnemiesLeaf);
		p_chooseTargetSequence.AddTask (setEnemiesWithinRangeLeaf);
		p_chooseTargetSequence.AddTask(setEnemyWithLowestHealthAndFailDecorator);
	}

	private void SetupGetFreeSpaceSequence(Script_Sequence p_getFreeSpaceSequence)
	{
		int range = 1;

		Script_Action_SetFlag setEnemies = new Script_Action_SetFlag (_behaviourTree, _manager.GetEnemies(), _getSpaceEntityListFlag);
		Script_Action_SetEntitiesWithinRange setEnemiesWithinRange = new Script_Action_SetEntitiesWithinRange(_behaviourTree,_grid,_getSpaceEntityListFlag,_getSpaceEntityListFlag,this as Script_IEntity,range);
		Script_Condition_AreAnyEntitiesAtEntityLocation isEnemyAtMyLocation = new Script_Condition_AreAnyEntitiesAtEntityLocation(_behaviourTree,_getSpaceEntityListFlag,this as Script_IEntity);
		Script_Action_SetFlag setFlagToSelf = new Script_Action_SetFlag(_behaviourTree,this as Script_IEntity,_selfFlag);

		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetNearestLocationSurroundingEntity setLocationAroundSelf = new Script_Action_SetNearestLocationSurroundingEntity (_behaviourTree, _grid, _selfFlag, this as Script_IEntity, _locationFlag);
		Script_Action_OccupyTile occupyTile = new Script_Action_OccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_MoveToLocation moveToLocationAroundSelf = new Script_Action_MoveToLocation (_behaviourTree, this as Script_IEntity, _locationFlag);

		Script_LeafTask setEnemiesLeaf = new Script_LeafTask (_behaviourTree, setEnemies);
		Script_LeafTask setEnemiesWithinRangeLeaf = new Script_LeafTask (_behaviourTree, setEnemiesWithinRange);
		Script_LeafTask isEnemyAtMyLocationLeaf = new Script_LeafTask (_behaviourTree, isEnemyAtMyLocation);
		Script_LeafTask setFlagToSelfLeaf = new Script_LeafTask (_behaviourTree, setFlagToSelf);
		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask(_behaviourTree,unoccupyTile);
		Script_LeafTask setLocationAroundSelfLeaf = new Script_LeafTask (_behaviourTree, setLocationAroundSelf);
		Script_LeafTask occupyTileLeaf = new Script_LeafTask(_behaviourTree,occupyTile);
		Script_LeafTask moveToLocationAroundSelfLeaf = new Script_LeafTask (_behaviourTree, moveToLocationAroundSelf);

		Script_Decorator_NeverSucceed moveToLocationAndNeverSucceed = new Script_Decorator_NeverSucceed (_behaviourTree, moveToLocationAroundSelfLeaf);

		p_getFreeSpaceSequence.AddTask (setEnemiesLeaf);
		p_getFreeSpaceSequence.AddTask (setEnemiesWithinRangeLeaf);
		p_getFreeSpaceSequence.AddTask (isEnemyAtMyLocationLeaf);
		p_getFreeSpaceSequence.AddTask (setFlagToSelfLeaf);
		p_getFreeSpaceSequence.AddTask (unoccupyTileLeaf);
		p_getFreeSpaceSequence.AddTask (setLocationAroundSelfLeaf);
		p_getFreeSpaceSequence.AddTask (occupyTileLeaf);
		p_getFreeSpaceSequence.AddTask (moveToLocationAndNeverSucceed);
	}

	private void SetupTreeFlags()
	{
		_locationFlag = blackboard_flags.flagOne;
		_targetEntityFlag = blackboard_flags.flagTwo;
		_entityListFlag = blackboard_flags.flagThree;
		_forwardSwingFlag = blackboard_flags.flagFour;
		_entityToFollowFlag = blackboard_flags.flagFive;
		_getSpaceEntityListFlag = blackboard_flags.flagSix;
		_selfFlag = blackboard_flags.flagSeven;


		int startPosX = Mathf.RoundToInt(_gameObject.transform.position.x);
		int startPosY = Mathf.RoundToInt(_gameObject.transform.position.y);
		int startPosZ = Mathf.RoundToInt(_gameObject.transform.position.z);
		Vector3Int startPos = new Vector3Int(startPosX,startPosY,startPosZ);
		_behaviourTree.SetBlackboardElement (_forwardSwingFlag, startPos);

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
			_manager.RemoveDps(this);

			Vector3Int pos = _behaviourTree.GetBlackBoardElement<Vector3Int> (_locationFlag);
			_grid.AccessGridTile (pos.x, pos.z).SetOccupied (false);

			_behaviourTree.RemoveBlackboardElement (_locationFlag);
			_behaviourTree.RemoveBlackboardElement(_targetEntityFlag);
			_behaviourTree.RemoveBlackboardElement (_entityListFlag);
			_behaviourTree.RemoveBlackboardElement (_forwardSwingFlag);
			_behaviourTree.RemoveBlackboardElement (_entityToFollowFlag);
			_behaviourTree.RemoveBlackboardElement (_getSpaceEntityListFlag);

			_behaviourTree = null;
		}
	}

	public override void TakeDamage(Script_IEntity p_attacker, int p_damage)
	{
		
		Script_IEnemy attacker = p_attacker as Script_IEnemy;
		Assert.IsNotNull (attacker);

		_health -= p_damage;

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
