using UnityEngine;

public class Script_Projectile : Script_IProjectile {


	private GameObject _projectileObject;
	private Script_IEntity _targetEntity;
	private Script_GameManager _manager;
	private Script_IEntity _startEntity;


	private float _speed;

	private Material _objectMaterial;

	public Script_Projectile(Script_GameManager p_manager, Script_IEntity p_startEntity, Script_IEntity p_targetEntity, Color p_color)
	{
		_manager = p_manager;
		_targetEntity = p_targetEntity;
		_startEntity = p_startEntity; 

		_speed = 3.0f;

		_projectileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_projectileObject.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
		_projectileObject.name = "DamageDealerProjectileObject";
		_objectMaterial = _projectileObject.GetComponent<Renderer>().material;
		_objectMaterial.color = p_color;
		_projectileObject.transform.position = _startEntity.GetGridLocation();
	}

	public override void Update(float p_delta)
	{

		if (_targetEntity != null && _startEntity != null) {
			MoveTowardsEntity (_startEntity,_targetEntity, p_delta);
		} else {
			Destruction ();
		}
	}
	private void Destruction()
	{
		_manager.DestroyMaterial (_objectMaterial);
		_manager.DestroyGameObject (_projectileObject);
		_manager.DestroyProjectile(this as Script_IProjectile);
	}
		
	private void MoveTowardsEntity(Script_IEntity p_attackerEntity,Script_IEntity p_toEntity,float p_delta)
	{
		Vector3 movementDirection = (p_toEntity.GetPosition () - _projectileObject.transform.position ).normalized;
		Vector3 nextStep = _projectileObject.transform.position + (movementDirection * _speed * Time.deltaTime);

		if (Vector3.Distance(nextStep, p_toEntity.GetPosition() ) >= Vector3.Distance(_projectileObject.transform.position,p_toEntity.GetPosition() )) {			
			_projectileObject.transform.position = p_toEntity.GetPosition ();
			p_toEntity.TakeDamage (p_attackerEntity, _startEntity.GetStats()._damage);
			Destruction();
		}

		if (Vector3.Distance(nextStep, p_toEntity.GetPosition()) < Vector3.Distance(_projectileObject.transform.position , p_toEntity.GetPosition() )) {
			_projectileObject.transform.position = nextStep;
		}





	}		

}
