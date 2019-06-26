using UnityEngine;
using Namespace_NodeState;
using Namespace_Config;


public class Script_Action_MeeleAttack : Script_Action {

	private Script_BehaviourTree _tree;
	private blackboard_flags _targetEnemyFlag;
	private blackboard_flags _locationAnchorPointFlag;
	private Script_IEntity _entityToAttack;
	private blackboard_flags _isForwardSwingFlag;
	private Script_GameManager _manager;
	private float _swingDistance;

	public Script_Action_MeeleAttack(Script_BehaviourTree p_tree, Script_GameManager p_manager, blackboard_flags p_targetEnemyFlag, blackboard_flags p_locationAnchorPointFlag, Script_IEntity p_entityToAttack, blackboard_flags p_isForwardSwingFlag)
	{
		_manager = p_manager;
		_tree = p_tree;
		_targetEnemyFlag = p_targetEnemyFlag;
		_entityToAttack = p_entityToAttack;
		_isForwardSwingFlag = p_isForwardSwingFlag;

		_swingDistance = _manager.GetTileSize() * 0.4f;

	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode(float p_delta)
	{
		Script_IEntity targetEntity = _tree.GetBlackBoardElement<Script_IEntity>(_targetEnemyFlag);
		bool isForwardSwing = _tree.GetBlackBoardElement<bool> (_isForwardSwingFlag);

		if (targetEntity == null || targetEntity.GetGameObject() == null)
			return NodeState.Failed;


		if (isForwardSwing) {
			NodeState success = ForwardSwing (p_delta, _entityToAttack.GetGridLocation (), _entityToAttack, targetEntity);
			if (success == NodeState.Success) {
				targetEntity.TakeDamage (_entityToAttack, _entityToAttack.GetStats()._damage);
				return NodeState.Success;
			}

		}
		else if (!isForwardSwing) {
			BackSwing (p_delta, _entityToAttack.GetGridLocation (), _entityToAttack, targetEntity );
		}
		return NodeState.Running;
	}

	private NodeState ForwardSwing(float p_delta,Vector3Int p_anchorPoint, Script_IEntity p_attackerEntity, Script_IEntity p_targetEntity)
	{
		GameObject attackerObject = p_attackerEntity.GetGameObject();
		GameObject targetObject = p_targetEntity.GetGameObject();

		Vector3 forwardSwingPoint = (targetObject.transform.position-attackerObject.transform.position).normalized;
		Vector3 nextPosition = attackerObject.transform.position + (forwardSwingPoint * p_delta);
		float forwardSwingSpeed = p_attackerEntity.GetStats ()._attackSpeed * 3;

		if (Vector3.Distance (p_anchorPoint, nextPosition) > _swingDistance ) {
			attackerObject.transform.position = p_anchorPoint + (forwardSwingPoint * _swingDistance);
			_tree.SetBlackboardElement(_isForwardSwingFlag,false);

			return NodeState.Success;
		}
		else {
			attackerObject.transform.position += forwardSwingPoint * p_delta * forwardSwingSpeed;
		}
		return NodeState.Running;
	}

	private NodeState BackSwing(float p_delta,Vector3Int p_anchorPoint, Script_IEntity p_attackerEntity, Script_IEntity p_targetEntity)
	{
		GameObject attackerObject = p_attackerEntity.GetGameObject();
		GameObject targetObject = p_targetEntity.GetGameObject();

		Vector3 backSwingPoint = (attackerObject.transform.position - targetObject.transform.position).normalized;
		Vector3 nextPosition = attackerObject.transform.position + (backSwingPoint * p_delta);

		float backSwingSpeed = p_attackerEntity.GetStats ()._attackSpeed;

		if (Vector3.Distance (p_anchorPoint, nextPosition) > _swingDistance) {
			attackerObject.transform.position = p_anchorPoint + (backSwingPoint * _swingDistance);
			_tree.SetBlackboardElement(_isForwardSwingFlag,true);
		}
		else {
			attackerObject.transform.position += backSwingPoint * p_delta * backSwingSpeed;
		}
		return NodeState.Running;
	}

}
