using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Script_GameManager : MonoBehaviour {


	private Camera _camera;

	private Vector3 _cameraForward;
	private Vector3 _cameraUp;
	private Vector3 _cameraOffset;

	private float _tileSize = 1.0f;
	private int _width = 10;
	private int _height = 10;

	private float _cameraXPosition;
	private float _cameraZPosition;
	private float _cameraYPosition;

	private Script_Grid _grid;

	private Script_Tank _tank;
	private Script_DamageDealer _damageDealer;
	private Script_Healer _healer;

	private List<Script_IEntity> _enemyList;
	private List<Script_IEntity> _friendlyList;
	private List<Script_IProjectile> _projectileList;
	private List<Script_IVisual> _visualEffectsList;

	private Script_EnemySpawner _enemySpawner;



	void Start () {

		_cameraXPosition =  _width * 0.5f - _tileSize * 0.5f;
		_cameraZPosition = _height * 0.5f - _tileSize * 0.5f;
		_cameraYPosition = 10.0f;

		_cameraForward = new Vector3 (0, -1, 0);
		_cameraUp = new Vector3 (0, 0, 1);

		_camera = Camera.main;
		_camera.transform.position = new Vector3 (_cameraXPosition, _cameraYPosition, _cameraZPosition);
		_camera.transform.rotation = Quaternion.LookRotation (_cameraForward, _cameraUp);

		_grid = new Script_Grid ();
		_grid.InstantiateGrid (10, 10, 0);

		_friendlyList = new List<Script_IEntity> ();  
		_enemyList = new List<Script_IEntity> ();
		_projectileList = new List<Script_IProjectile>();
		_visualEffectsList = new List<Script_IVisual> ();

		_damageDealer = new Script_DamageDealer(this,_grid,new Vector3Int(6,0,5));
		_friendlyList.Add (_damageDealer);
		_healer = new Script_Healer (this, _grid, new Vector3Int (4, 0, 5));
		_friendlyList.Add (_healer);
		_tank = new Script_Tank (this,_grid,new Vector3Int(5,0,5));
		_friendlyList.Add (_tank);

		_tank.InitializeBehaviourTree ();
		_healer.InitializeBehaviourTree ();
		_damageDealer.InitializeBehaviourTree ();

		_enemySpawner = new Script_EnemySpawner (this, _grid);
	}
		
	void Update () {

		foreach (Script_IEntity friendly in _friendlyList.ToList())
		{
			friendly.Update (Time.deltaTime);
		}

		foreach (Script_IEntity enemy in _enemyList.ToList() ) {
			enemy.Update (Time.deltaTime);
		}

		foreach(Script_IProjectile projectile in _projectileList.ToList())
		{
			projectile.Update (Time.deltaTime);
		}

		foreach(Script_IVisual visual in _visualEffectsList.ToList())
		{
			visual.Update (Time.deltaTime);
		}
			
		foreach (Script_IEntity enemy in _enemyList.ToList()) {
			enemy.DetermineIfDead ();
		}

		foreach (Script_IEntity friendly in _friendlyList.ToList()) {
			friendly.DetermineIfDead ();
		}

		_enemySpawner.Update (Time.deltaTime);

	}

	public void DestroyGameObject(GameObject p_object)
	{
		Destroy (p_object);
	}

	public void DestroyTextMesh(TextMesh p_textMesh)
	{
		Destroy (p_textMesh);
	}

	public int GetWidth()
	{
		return _width;
	}

	public int GetHeight()
	{
		return _height;
	}


	public void AddEnemy(Script_Enemy p_enemy)
	{
			_enemyList.Add (p_enemy);
	}

	public Script_Tank GetTank()
	{
		return _tank;
	}

	public Script_DamageDealer GetDps()
	{
		return _damageDealer;
	}

	public Script_Healer GetHealer()
	{
		return _healer;
	}

	public List<Script_IEntity> GetEnemies()
	{
		return _enemyList;
	}

	public List<Script_IEntity> GetFriendlies()
	{
		return _friendlyList;
	}

	public void CreateProjectile(Script_IProjectile p_projectile)
	{
		_projectileList.Add (p_projectile);
	}

	public void DestroyProjectile(Script_IProjectile p_projectile)
	{
		_projectileList.Remove (p_projectile);
	}

	public void CreateVisual(Script_IVisual p_visual)
	{
		_visualEffectsList.Add (p_visual);
	}

	public void RemoveVisual(Script_IVisual p_visual)
	{
		_visualEffectsList.Remove (p_visual);
	}

	public void RemoveEnemy(Script_IEntity p_entity)
	{
		_enemyList.Remove (p_entity);
		p_entity = null;
	}

	public void RemoveTank(Script_IEntity p_entity)
	{
		_friendlyList.Remove (p_entity);
		p_entity = null;
		_tank = null;
	}

	public void RemoveDps(Script_IEntity p_entity)
	{
		_friendlyList.Remove (p_entity);
		p_entity = null;
		_damageDealer = null;
	}

	public void RemoveHealer(Script_IEntity p_entity)
	{
		_friendlyList.Remove (p_entity);
		p_entity = null;
		_healer = null;
	}

	public void DestroyMaterial(Material p_objectToDestroy)
	{
		Destroy (p_objectToDestroy);
	}

	public float GetTileSize()
	{
		return _tileSize;
	}

	public Script_Grid GetGrid()
	{
		return _grid;
	}

}
