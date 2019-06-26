using UnityEngine;
using Namespace_Config;
using UnityEngine.Assertions;

public class Script_Healer : Script_IFriendly {

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
	private blackboard_flags _targetToHealFlag;
	private blackboard_flags _entityListFlag;
	private blackboard_flags _isForwardSwingingFlag;
	private blackboard_flags _targetToFollowFlag;
	private blackboard_flags _getSpaceEntityListFlag;
	private blackboard_flags _selfFlag;
	private blackboard_flags _enemyListToAttackFlag;
	private blackboard_flags _targetToAttackFlag;


	private int _threatMultiplier;

	private Material _objectMaterial;

	private EntityStats _stats;

	private int _attackRange;
	private int _engageTargetRange;


	public Script_Healer(Script_GameManager p_manager, Script_Grid p_grid, Vector3Int p_gridLocation)
	{

		_attackRange = 3;
		_engageTargetRange = 3;

		_stats = new EntityStats(10,5,100,1);

		_threatMultiplier = 2;
		_health = _stats._maxHealth;

		_manager = p_manager;
		_grid = p_grid;

		_gridLocation = p_gridLocation;
		_position = p_gridLocation;

		_gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_gameObject.name = "HealerObject";
		_objectMaterial = _gameObject.GetComponent<Renderer>().material;
		_objectMaterial.color = Color.green;
		_gameObject.transform.position = p_gridLocation;

	}

	public void InitializeBehaviourTree()
	{
		_behaviourTree = new Script_BehaviourTree ();
		SetupTreeFlags ();

		Script_Selector startSelector = new Script_Selector(_behaviourTree);

		Script_Sequence walkingSequence = new Script_Sequence(_behaviourTree);
		SetupWalkingSequence (walkingSequence);

		Script_Sequence moveNearDpsSequence = new Script_Sequence(_behaviourTree);
		SetupFollowingSequence (moveNearDpsSequence, GetDps () as Script_IEntity);

		Script_Sequence moveNearTankSequence = new Script_Sequence (_behaviourTree);
		SetupFollowingSequence (moveNearTankSequence, GetTank () as Script_IEntity);

		Script_Sequence healSequence = new Script_Sequence(_behaviourTree);
		SetupHealSequence (healSequence);

		Script_Sequence ChooseAttackingTargetSequence = new Script_Sequence (_behaviourTree);
		SetupChooseTargetToAttack (ChooseAttackingTargetSequence);

		Script_Sequence attackSequence = new Script_Sequence(_behaviourTree);
		SetupAttackSequence(attackSequence);

		Script_Sequence ChooseHealingTargetSequence = new Script_Sequence (_behaviourTree);
		SetupChooseTargetToHeal (ChooseHealingTargetSequence);

		Script_Sequence GetFreeSpaceSequence = new Script_Sequence (_behaviourTree);
		SetupGetFreeSpaceSequence (GetFreeSpaceSequence);


		startSelector.AddTask(GetFreeSpaceSequence);
		startSelector.AddTask(ChooseHealingTargetSequence);
		startSelector.AddTask(healSequence);
		startSelector.AddTask(ChooseAttackingTargetSequence);
		startSelector.AddTask(attackSequence);
		startSelector.AddTask(moveNearTankSequence);
		startSelector.AddTask(moveNearDpsSequence);
		startSelector.AddTask(walkingSequence);



		_behaviourTree.AddChild (startSelector);

	}


	private void SetupWalkingSequence(Script_Sequence p_walkingSequence )
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

		Script_Decorator_Inverter areEnemiesNorNearby = new Script_Decorator_Inverter (_behaviourTree, areEnemiesNearbyLeaf);

		p_walkingSequence.AddTask(setEnemiesLeaf);
		p_walkingSequence.AddTask (unoccupyTileLeaf);
		p_walkingSequence.AddTask (setRandomLocationAroundSelfLeaf);
		p_walkingSequence.AddTask (occupyLocationLeaf);
		p_walkingSequence.AddTask (areEnemiesNorNearby);
		p_walkingSequence.AddTask (moveToLocationLeaf);
	}

	private void SetupFollowingSequence(Script_Sequence p_followingSequence, Script_IEntity p_entityToFollow)
	{
		Script_Action_SetFlag setTargetToFollowFlag = new Script_Action_SetFlag(_behaviourTree, p_entityToFollow as Script_IEntity, _targetToFollowFlag ); 
		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetNearestLocationSurroundingEntity setLocationNearTarget = new Script_Action_SetNearestLocationSurroundingEntity(_behaviourTree,_grid,_targetToFollowFlag,this as Script_IEntity,_locationFlag);
		Script_Action_OccupyTile occupyTile = new Script_Action_OccupyTile(_behaviourTree,_grid, _locationFlag);
		Script_Action_MoveToLocation moveToLocation = new Script_Action_MoveToLocation (_behaviourTree, this as Script_IEntity, _locationFlag);

		Script_Condition_IsEntityNull isTargetNull = new Script_Condition_IsEntityNull(_behaviourTree,_targetToFollowFlag); 

		Script_LeafTask setTargetToFollowFlagLeaf = new Script_LeafTask(_behaviourTree, setTargetToFollowFlag);
		Script_LeafTask isTargetNullLeaf = new Script_LeafTask(_behaviourTree, isTargetNull);
		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask(_behaviourTree, unoccupyTile);
		Script_LeafTask setLocationNearTargetLeaf = new Script_LeafTask (_behaviourTree,setLocationNearTarget);
		Script_LeafTask occupyTileLeaf = new Script_LeafTask (_behaviourTree, occupyTile);
		Script_LeafTask moveToLocationLeaf = new Script_LeafTask (_behaviourTree, moveToLocation);

		Script_Decorator_Inverter isTargetNotNullDecorator = new Script_Decorator_Inverter (_behaviourTree,isTargetNullLeaf);

		p_followingSequence.AddTask (setTargetToFollowFlagLeaf);
		p_followingSequence.AddTask(isTargetNotNullDecorator);
		p_followingSequence.AddTask (unoccupyTileLeaf);
		p_followingSequence.AddTask (setLocationNearTargetLeaf);
		p_followingSequence.AddTask (occupyTileLeaf);
		p_followingSequence.AddTask (moveToLocationLeaf);

	}

	private void SetupHealSequence(Script_Sequence p_healSequence)
	{
		int healThreshold = 80;

		Script_Condition_IsEntityHealthUnderThreshold isTargetUnderHealthValue = new Script_Condition_IsEntityHealthUnderThreshold(_behaviourTree, _targetToHealFlag,healThreshold);
		Script_Condition_IsEntityNearby isTargetToHealNearby = new Script_Condition_IsEntityNearby (_behaviourTree, _grid, _targetToHealFlag, this as Script_IEntity, _attackRange);
		Script_Action_SetFlag forwardSwingFlag = new Script_Action_SetFlag (_behaviourTree, false, _isForwardSwingingFlag);
		Script_Action_Heal healTarget = new Script_Action_Heal(_behaviourTree,_manager,_targetToHealFlag,_locationFlag,this as Script_IEntity,_isForwardSwingingFlag);
		Script_Condition_IsEntityNull isTargetNull = new Script_Condition_IsEntityNull (_behaviourTree, _targetToHealFlag);

		Script_LeafTask isTargetNullLeaf = new Script_LeafTask (_behaviourTree, isTargetNull);
		Script_Decorator_Inverter isTargetNotNullDecorator = new Script_Decorator_Inverter (_behaviourTree, isTargetNullLeaf);
		Script_LeafTask IsTargetUnderHealthValueLeaf = new Script_LeafTask (_behaviourTree, isTargetUnderHealthValue);
		Script_LeafTask isTargetToHealNearbyLeaf = new Script_LeafTask (_behaviourTree, isTargetToHealNearby);
		Script_LeafTask SetForwardSwingFlagLeaf = new Script_LeafTask (_behaviourTree, forwardSwingFlag);
		Script_LeafTask healTargetLeaf = new Script_LeafTask (_behaviourTree, healTarget);

		p_healSequence.AddTask(isTargetNotNullDecorator);
		p_healSequence.AddTask (IsTargetUnderHealthValueLeaf);
		p_healSequence.AddTask (isTargetToHealNearbyLeaf);
		p_healSequence.AddTask (SetForwardSwingFlagLeaf);
		p_healSequence.AddTask (healTargetLeaf);
	}

	private void SetupChooseTargetToHeal(Script_Sequence p_chooseHealingTargetSequence)
	{
		Script_Action_SetFlag setFriendlies = new Script_Action_SetFlag (_behaviourTree, _manager.GetFriendlies(), _entityListFlag);
		Script_Action_SetEntitiesWithinRange setFriendliesWithinRange = new Script_Action_SetEntitiesWithinRange(_behaviourTree, _grid,_entityListFlag,_entityListFlag,this as Script_IEntity,_attackRange);
		Script_Action_SetEntityWithLowestHealth setFriendlyWithLowestHealth = new Script_Action_SetEntityWithLowestHealth(_behaviourTree, _entityListFlag, _targetToHealFlag);

		Script_LeafTask setFriendliesLeaf = new Script_LeafTask (_behaviourTree, setFriendlies);
		Script_LeafTask setFriendliesWithinRangeLeaf = new Script_LeafTask (_behaviourTree, setFriendliesWithinRange);
		Script_LeafTask setFriendlyWithLowestHealthLeaf = new Script_LeafTask (_behaviourTree,setFriendlyWithLowestHealth);
		Script_Decorator_AlwaysFail setfriendlyWithLowestHealthAndFailDecorator = new Script_Decorator_AlwaysFail (_behaviourTree, setFriendlyWithLowestHealthLeaf);


		p_chooseHealingTargetSequence.AddTask (setFriendliesLeaf);
		p_chooseHealingTargetSequence.AddTask (setFriendliesWithinRangeLeaf);
		p_chooseHealingTargetSequence.AddTask(setfriendlyWithLowestHealthAndFailDecorator);
	}

	private void SetupChooseTargetToAttack( Script_Sequence p_chooseAttackingTargetSequence )
	{
		Script_Action_SetFlag setEnemies = new Script_Action_SetFlag (_behaviourTree, _manager.GetEnemies(), _enemyListToAttackFlag);
		Script_Action_SetEntitiesWithinRange setEnemiesWithinRange = new Script_Action_SetEntitiesWithinRange(_behaviourTree, _grid,_enemyListToAttackFlag,_enemyListToAttackFlag,this as Script_IEntity,_attackRange);
		Script_Action_SetEntityWithLowestHealth setEnemyWithLowestHealth = new Script_Action_SetEntityWithLowestHealth(_behaviourTree, _enemyListToAttackFlag, _targetToAttackFlag);

		Script_LeafTask setEnemiesLeaf = new Script_LeafTask (_behaviourTree,setEnemies);
		Script_LeafTask setEnemiesWithinRangeLeaf = new Script_LeafTask (_behaviourTree, setEnemiesWithinRange);
		Script_LeafTask setEnemyWithLowestHealthLeaf = new Script_LeafTask (_behaviourTree,setEnemyWithLowestHealth);

		Script_Decorator_AlwaysFail setEnemyWithLowestHealthAndFailDecorator = new Script_Decorator_AlwaysFail (_behaviourTree, setEnemyWithLowestHealthLeaf);

		p_chooseAttackingTargetSequence.AddTask (setEnemiesLeaf);
		p_chooseAttackingTargetSequence.AddTask (setEnemiesWithinRangeLeaf);
		p_chooseAttackingTargetSequence.AddTask(setEnemyWithLowestHealthAndFailDecorator);
	}

	private void SetupAttackSequence(Script_Sequence p_attackSequence)
	{
		Script_Condition_IsEntityNearby isTargetNearby = new Script_Condition_IsEntityNearby (_behaviourTree, _grid, _targetToAttackFlag, this as Script_IEntity, _attackRange);
		Script_Action_SetFlag setForwardSwingFlag = new Script_Action_SetFlag (_behaviourTree, false, _isForwardSwingingFlag);
		Script_Action_RangedAttack attackTarget = new Script_Action_RangedAttack(_behaviourTree,_manager,_targetToAttackFlag,_locationFlag,this as Script_IEntity,_isForwardSwingingFlag,Color.green);
		Script_Condition_IsEntityNull isTargetNull = new Script_Condition_IsEntityNull (_behaviourTree, _targetToAttackFlag);

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

	private void SetupGetFreeSpaceSequence(Script_Sequence p_getFreeSpaceSequence)
	{
		int range = 1;
		Script_Action_SetFlag setEnemies = new Script_Action_SetFlag (_behaviourTree, _manager.GetEnemies(), _getSpaceEntityListFlag);
		Script_Action_SetEntitiesWithinRange setEnemiesWithinRange = new Script_Action_SetEntitiesWithinRange(_behaviourTree,_grid,_getSpaceEntityListFlag,_getSpaceEntityListFlag,this as Script_IEntity,range);
		Script_Condition_AreAnyEntitiesAtEntityLocation isEnemyAtMyPosition = new Script_Condition_AreAnyEntitiesAtEntityLocation(_behaviourTree,_getSpaceEntityListFlag,this as Script_IEntity);
		Script_Action_SetFlag setFlagToSelf = new Script_Action_SetFlag(_behaviourTree,this as Script_IEntity,_selfFlag);

		Script_Action_UnoccupyTile unoccupyTile = new Script_Action_UnoccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_SetNearestLocationSurroundingEntity setLocationAroundSelf = new Script_Action_SetNearestLocationSurroundingEntity (_behaviourTree, _grid, _selfFlag, this as Script_IEntity, _locationFlag);
		Script_Action_OccupyTile occupyTile = new Script_Action_OccupyTile (_behaviourTree, _grid, _locationFlag);
		Script_Action_MoveToLocation moveToLocation = new Script_Action_MoveToLocation (_behaviourTree, this as Script_IEntity, _locationFlag);


		Script_LeafTask setEnemiesLeaf = new Script_LeafTask (_behaviourTree, setEnemies);
		Script_LeafTask setEnemiesWithinRangeLeaf = new Script_LeafTask (_behaviourTree, setEnemiesWithinRange);
		Script_LeafTask isEnemyAtMyPositionLeaf = new Script_LeafTask (_behaviourTree, isEnemyAtMyPosition);
		Script_LeafTask setFlagToSelfLeaf = new Script_LeafTask (_behaviourTree, setFlagToSelf);
		Script_LeafTask unoccupyTileLeaf = new Script_LeafTask(_behaviourTree,unoccupyTile);
		Script_LeafTask setLocationAroundSelfLeaf = new Script_LeafTask (_behaviourTree, setLocationAroundSelf);
		Script_LeafTask occupyTileLeaf = new Script_LeafTask(_behaviourTree,occupyTile);
		Script_LeafTask moveToLocationLeaf = new Script_LeafTask (_behaviourTree, moveToLocation);

		Script_Decorator_NeverSucceed moveToLocationAndNeverSucceedDecorator = new Script_Decorator_NeverSucceed (_behaviourTree, moveToLocationLeaf);

		p_getFreeSpaceSequence.AddTask (setEnemiesLeaf);
		p_getFreeSpaceSequence.AddTask (setEnemiesWithinRangeLeaf);
		p_getFreeSpaceSequence.AddTask (isEnemyAtMyPositionLeaf);
		p_getFreeSpaceSequence.AddTask (setFlagToSelfLeaf);
		p_getFreeSpaceSequence.AddTask (unoccupyTileLeaf);
		p_getFreeSpaceSequence.AddTask (setLocationAroundSelfLeaf);
		p_getFreeSpaceSequence.AddTask (occupyTileLeaf);
		p_getFreeSpaceSequence.AddTask (moveToLocationAndNeverSucceedDecorator);

	}


	private void SetupTreeFlags()
	{
		_locationFlag = blackboard_flags.flagOne;
		_targetToHealFlag = blackboard_flags.flagTwo;
		_entityListFlag = blackboard_flags.flagThree;
		_isForwardSwingingFlag = blackboard_flags.flagFour;
		_targetToFollowFlag = blackboard_flags.flagFive;
		_getSpaceEntityListFlag = blackboard_flags.flagSix;
		_selfFlag = blackboard_flags.flagSeven;
		_enemyListToAttackFlag = blackboard_flags.flagEight;
		_targetToAttackFlag = blackboard_flags.flagNine;

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

	public Script_Tank GetTank()
	{
		return _manager.GetTank ();

	}

	private Script_DamageDealer GetDps()
	{
		return _manager.GetDps ();
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
			_manager.RemoveHealer(this);
		
			Vector3Int pos = _behaviourTree.GetBlackBoardElement<Vector3Int> (_locationFlag);
			_grid.AccessGridTile (pos.x, pos.z).SetOccupied (false);

			_behaviourTree.RemoveBlackboardElement (_locationFlag);
			_behaviourTree.RemoveBlackboardElement(_targetToHealFlag);
			_behaviourTree.RemoveBlackboardElement (_entityListFlag);
			_behaviourTree.RemoveBlackboardElement (_isForwardSwingingFlag);
			_behaviourTree.RemoveBlackboardElement (_targetToFollowFlag);
			_behaviourTree.RemoveBlackboardElement (_getSpaceEntityListFlag);
			_behaviourTree.RemoveBlackboardElement(_selfFlag);
			_behaviourTree.RemoveBlackboardElement (_enemyListToAttackFlag);
			_behaviourTree.RemoveBlackboardElement (_targetToAttackFlag);

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