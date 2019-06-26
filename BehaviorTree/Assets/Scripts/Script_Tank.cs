using UnityEngine;
using Namespace_Config;
using UnityEngine.Assertions;

public class Script_Tank : Script_IFriendly {

	private Script_GameManager _manager;
	private Script_Grid _grid;
	private Vector3Int _gridLocation;
	private Vector3 _position;
	private Vector3 _rotation;
	private Vector3 _scale;
	private GameObject _gameObject;

	private Script_BehaviourTree _behaviourTree;

	private int _health;
	private int _threatMultiplier;

	private Material _objectMaterial;

	private EntityStats _stats;

	private blackboard_flags _locationFlag;
	private blackboard_flags _targetEntityFlag;
	private blackboard_flags _entityListFlag;
	private blackboard_flags _forwardSwingFlag;
	private blackboard_flags _getSpaceEntityListFlag;
	private blackboard_flags _selfFlag;


	private int _attackRange;
	private int _engageTargetRange;

	public Script_Tank(Script_GameManager p_manager, Script_Grid p_grid, Vector3Int p_gridLocation)
	{
		_attackRange = 1;
		_engageTargetRange = 3;

		_threatMultiplier = 2;
		_stats = new EntityStats(15,0,100,1);

		_health = _stats._maxHealth;

		_manager = p_manager;
		_grid = p_grid;

		_gridLocation = p_gridLocation;
		_position = p_gridLocation;
	

		_gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_gameObject.name = "TankObject";
		_objectMaterial = _gameObject.GetComponent<Renderer>().material;
		_objectMaterial.color = Color.black;
		_gameObject.transform.position = p_gridLocation;

	}

	public void InitializeBehaviourTree()
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


		Script_Sequence ChooseTargetSequence = new Script_Sequence(_behaviourTree);
		SetupChooseTargetSequence (ChooseTargetSequence);
		Script_Decorator_AlwaysFail alwaysFailWhileChoosingTarget = new Script_Decorator_AlwaysFail (_behaviourTree, ChooseTargetSequence);

		Script_Sequence GetFreeSpaceSequence = new Script_Sequence (_behaviourTree);
		SetupGetFreeSpaceSequence (GetFreeSpaceSequence);


		startSelector.AddTask (GetFreeSpaceSequence);
		startSelector.AddTask (alwaysFailWhileChoosingTarget);
		startSelector.AddTask (pursuitSequence);
		startSelector.AddTask (attackSequence);
		startSelector.AddTask (walkingSequence);

		_behaviourTree.AddChild (startSelector);

	}

	private void SetupWalkingSequence(Script_Sequence p_walkingSequence)
	{
		Script_Action_SetFlag setEnemies = new Script_Action_SetFlag (_behaviourTree, _manager.GetEnemies(), _entityListFlag);
		Script_Condition_IsEntityTypeNearby areEnemiesNearby = new Script_Condition_IsEntityTypeNearby (_behaviourTree,_grid, _entityListFlag,this as Script_IEntity, _engageTargetRange);


		int walkRange = 1;

		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetRandomLocationWithinRange setRandomLocationAroundSelf = new Script_Action_SetRandomLocationWithinRange (_behaviourTree, _locationFlag, this as Script_IEntity, _grid,walkRange);
		Script_Action_MoveToLocation moveToLocation = new Script_Action_MoveToLocation(_behaviourTree, this as Script_IEntity, _locationFlag);
		Script_Action_OccupyTile occupyLocation = new Script_Action_OccupyTile (_behaviourTree, _grid, _locationFlag);


		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask (_behaviourTree, unoccupyTile);
		Script_LeafTask setRandomLocationAroundSelfLeaf = new Script_LeafTask(_behaviourTree,setRandomLocationAroundSelf);
		Script_LeafTask setEnemiesLeaf = new Script_LeafTask (_behaviourTree, setEnemies);
		Script_LeafTask moveToLocationLeaf = new Script_LeafTask(_behaviourTree,moveToLocation);
		Script_LeafTask occupyLocationLeaf = new Script_LeafTask (_behaviourTree, occupyLocation);

		Script_LeafTask areEnemiesNearbyLeaf = new Script_LeafTask (_behaviourTree, areEnemiesNearby);

		Script_Decorator_Inverter areEnemiesNotNearbyDecorator = new Script_Decorator_Inverter (_behaviourTree, areEnemiesNearbyLeaf);

		p_walkingSequence.AddTask(setEnemiesLeaf);
		p_walkingSequence.AddTask (unoccupyTileLeaf);
		p_walkingSequence.AddTask (setRandomLocationAroundSelfLeaf);
		p_walkingSequence.AddTask (occupyLocationLeaf);
		p_walkingSequence.AddTask (areEnemiesNotNearbyDecorator);
		p_walkingSequence.AddTask (moveToLocationLeaf);
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
		Script_Action_MeeleAttack attackEnemy = new Script_Action_MeeleAttack(_behaviourTree,_manager,_targetEntityFlag,_locationFlag,this as Script_IEntity,_forwardSwingFlag);

		Script_Condition_IsEntityNull isTargetNull = new Script_Condition_IsEntityNull (_behaviourTree, _targetEntityFlag);

		Script_LeafTask isTargetNullLeaf = new Script_LeafTask (_behaviourTree, isTargetNull);
		Script_Decorator_Inverter isTargetNotNullDecorator = new Script_Decorator_Inverter (_behaviourTree, isTargetNullLeaf);
		Script_LeafTask isTargetNearbyLeaf = new Script_LeafTask (_behaviourTree, isTargetNearby);
		Script_LeafTask setForwardSwingFlagLeaf = new Script_LeafTask (_behaviourTree, setForwardSwingFlag);
		Script_LeafTask attackEnemyLeaf = new Script_LeafTask (_behaviourTree, attackEnemy);

		p_attackSequence.AddTask(isTargetNotNullDecorator);
		p_attackSequence.AddTask (isTargetNearbyLeaf);
		p_attackSequence.AddTask (setForwardSwingFlagLeaf);
		p_attackSequence.AddTask (attackEnemyLeaf);
	}

	private void SetupChooseTargetSequence(Script_Sequence p_chooseTargetSequence)
	{
		Script_Action_SetFlag setEnemies= new Script_Action_SetFlag (_behaviourTree, _manager.GetEnemies(), _entityListFlag);
		Script_Action_SetEntitiesWithinRange setEnemiesWithinRange = new Script_Action_SetEntitiesWithinRange(_behaviourTree, _grid,_entityListFlag,_entityListFlag,this as Script_IEntity,_engageTargetRange);

		Script_Action_SetNearestEntityFromEntityWithinRange setNearestEnemy = new Script_Action_SetNearestEntityFromEntityWithinRange (_behaviourTree, _grid, _entityListFlag, _engageTargetRange, this as Script_IEntity , _targetEntityFlag);
		Script_Action_SetEnemyAttackingTarget setEnemyAttackingDps = new Script_Action_SetEnemyAttackingTarget(_behaviourTree,_entityListFlag, _manager.GetDps() as Script_IEntity ,_targetEntityFlag);
		Script_Action_SetEnemyAttackingTarget setEnemyAttackingHealer = new Script_Action_SetEnemyAttackingTarget(_behaviourTree,_entityListFlag, _manager.GetHealer() as Script_IEntity ,_targetEntityFlag);

		Script_Selector chooseTargetToAidSelector = new Script_Selector (_behaviourTree);

		Script_LeafTask setEnemiesLeaf = new Script_LeafTask (_behaviourTree, setEnemies);
		Script_LeafTask setEnemiesWithinRangeLeaf = new Script_LeafTask (_behaviourTree, setEnemiesWithinRange);
		Script_LeafTask setNearestEnemyLeaf = new Script_LeafTask(_behaviourTree,setNearestEnemy);

		Script_LeafTask setEnemyAttackingHealerLeaf = new Script_LeafTask(_behaviourTree,setEnemyAttackingHealer);
		Script_LeafTask setEnemyAttackingDpsLeaf = new Script_LeafTask(_behaviourTree,setEnemyAttackingDps);

		chooseTargetToAidSelector.AddTask (setEnemyAttackingHealerLeaf);
		chooseTargetToAidSelector.AddTask (setEnemyAttackingDpsLeaf);

		p_chooseTargetSequence.AddTask (setEnemiesLeaf); 
		p_chooseTargetSequence.AddTask (setEnemiesWithinRangeLeaf);
		p_chooseTargetSequence.AddTask (setNearestEnemyLeaf);
		p_chooseTargetSequence.AddTask (chooseTargetToAidSelector);


	}

	private void SetupGetFreeSpaceSequence(Script_Sequence p_getFreeSpaceSequence)
	{
		int range = 1;
		Script_Action_SetFlag setEnemies = new Script_Action_SetFlag (_behaviourTree, _manager.GetEnemies(), _getSpaceEntityListFlag);
		Script_Action_SetEntitiesWithinRange setEnemiesWithinRange = new Script_Action_SetEntitiesWithinRange(_behaviourTree,_grid,_getSpaceEntityListFlag,_getSpaceEntityListFlag,this as Script_IEntity,range);
		Script_Condition_AreAnyEntitiesAtEntityLocation isEnemyAtLocation = new Script_Condition_AreAnyEntitiesAtEntityLocation(_behaviourTree,_getSpaceEntityListFlag,this as Script_IEntity);

		Script_Action_SetFlag setFlagToSelf = new Script_Action_SetFlag(_behaviourTree,this as Script_IEntity,_selfFlag);

		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetNearestLocationSurroundingEntity getLocationAroundSelf = new Script_Action_SetNearestLocationSurroundingEntity (_behaviourTree, _grid, _selfFlag, this as Script_IEntity, _locationFlag);
		Script_Action_OccupyTile occupyTile = new Script_Action_OccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_MoveToLocation moveToLocation = new Script_Action_MoveToLocation (_behaviourTree, this as Script_IEntity, _locationFlag);


		Script_LeafTask setEnemiesLeaf = new Script_LeafTask (_behaviourTree, setEnemies);
		Script_LeafTask setEnemiesWithinRangeLeaf = new Script_LeafTask (_behaviourTree, setEnemiesWithinRange);
		Script_LeafTask isEnemyAtLocationLeaf = new Script_LeafTask (_behaviourTree, isEnemyAtLocation);
		Script_LeafTask setFlagToSelfLeaf = new Script_LeafTask (_behaviourTree, setFlagToSelf);
		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask(_behaviourTree,unoccupyTile);
		Script_LeafTask getLocationAroundSelfLeaf = new Script_LeafTask (_behaviourTree, getLocationAroundSelf);
		Script_LeafTask occupyTileLeaf = new Script_LeafTask(_behaviourTree,occupyTile);
		Script_LeafTask moveToLocationleaf = new Script_LeafTask (_behaviourTree, moveToLocation);

		Script_Decorator_NeverSucceed moveToLocationAndNeverSucceedDecorator = new Script_Decorator_NeverSucceed (_behaviourTree, moveToLocationleaf);

		p_getFreeSpaceSequence.AddTask (setEnemiesLeaf);
		p_getFreeSpaceSequence.AddTask (setEnemiesWithinRangeLeaf);
		p_getFreeSpaceSequence.AddTask (isEnemyAtLocationLeaf);
		p_getFreeSpaceSequence.AddTask (setFlagToSelfLeaf);
		p_getFreeSpaceSequence.AddTask (unoccupyTileLeaf);
		p_getFreeSpaceSequence.AddTask (getLocationAroundSelfLeaf);
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
			_manager.RemoveTank(this);

			Vector3Int pos = _behaviourTree.GetBlackBoardElement<Vector3Int> (_locationFlag);
			_grid.AccessGridTile (pos.x, pos.z).SetOccupied (false);

			_behaviourTree.RemoveBlackboardElement (_locationFlag);
			_behaviourTree.RemoveBlackboardElement(_targetEntityFlag);
			_behaviourTree.RemoveBlackboardElement (_entityListFlag);
			_behaviourTree.RemoveBlackboardElement (_forwardSwingFlag);
			_behaviourTree.RemoveBlackboardElement (_getSpaceEntityListFlag);
			_behaviourTree.RemoveBlackboardElement (_selfFlag);

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
