using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Script_EnemySpawner {
	float _enemyTimer = 3.0f;
	float _enemyTimerCurrent = 0.0f;
	int _maxEnemyAmount;
	private Script_GameManager _manager;
	private Script_Grid _grid;

	public Script_EnemySpawner(Script_GameManager p_manager, Script_Grid p_grid)
	{
		_maxEnemyAmount = 3;
		_manager = p_manager;
		_grid = p_grid;
	}

	private enum Sides
	{
		UP,
		DOWN,
		LEFT,
		RIGHT,
		ENUMEND,
	}


	public void SetEnemyTimer(float p_frequency)
	{
		_enemyTimer = p_frequency;
		_enemyTimerCurrent = 0.0f;
	}

	public void CreateEnemy(Vector3Int p_location)
	{
		if (_manager.GetEnemies ().Count < _maxEnemyAmount) {
			Script_Enemy enemy = new Script_Enemy (_manager, _grid, p_location);
			_manager.AddEnemy (enemy);
		}
	}

	private bool SpawnEnemiesTimer(float p_deltaTime)
	{
		_enemyTimerCurrent += p_deltaTime;

		if (_enemyTimerCurrent >= _enemyTimer) {
			_enemyTimerCurrent -= _enemyTimer;
			return true;
		}

		return false;

	}

	public void Update(float p_deltaTime)
	{
		if (SpawnEnemiesTimer (p_deltaTime) == true) {			
			Vector3Int spawnLocation = GetRandomLocationAtEdgeOfGrid();
			CreateEnemy(spawnLocation);
		}
	}

	private Vector3Int GetRandomLocationAtEdgeOfGrid()
	{
		int side = Random.Range (0, (int)Sides.ENUMEND );
		int x = 0;
		int z = 0;

		if (side == (int)Sides.UP) {
			x = Random.Range (0, _manager.GetWidth ());
		} 
		else if (side == (int)Sides.DOWN) {
			x = Random.Range (0, _manager.GetWidth ());
			z = _manager.GetHeight () - 1;
		} 
		else if (side == (int)Sides.LEFT) {
			z = Random.Range(0, _manager.GetHeight());
		} 
		else if (side == (int)Sides.RIGHT) {
			z = Random.Range(0, _manager.GetHeight());
			x = _manager.GetWidth () - 1 ;
		}

		return new Vector3Int (x, 0, z);

	}

}
