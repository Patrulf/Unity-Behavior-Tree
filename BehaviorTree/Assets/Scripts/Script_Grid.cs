using UnityEngine;

public class Script_Grid {

	Script_Tile[] _grid = new Script_Tile[0];

	private int _width = 0;
	private int _height = 0;
	private int _gridDepthPosition = 0;

	public Script_Grid()
	{
	}




	public Script_Tile AccessGridTile(int p_x, int p_z)
	{
		return _grid[p_z * _width + p_x];
	}

	private void SetGridTile(int p_x, int p_z, Script_Tile p_tile)
	{
		_grid [p_z * _width + p_x] = p_tile;
	}




	public void InstantiateGrid (int p_width, int p_height, int p_gridDepthPosition )
	{
		_width = p_width;
		_height = p_height;
		_gridDepthPosition = p_gridDepthPosition;

		System.Array.Resize (ref _grid, p_width * p_height);

		for (int z = 0; z < _height; z++) {
			for (int x = 0; x < _width; x++) {
				Vector3Int tilePosition = new Vector3Int (x, _gridDepthPosition, z);
				Script_Tile myTile = new Script_Tile (tilePosition);
				SetGridTile (x, z, myTile);
			}
		}			
	}

	public int GetWidth()
	{
		return _width;
	}

	public int GetHeight()
	{
		return _height;
	}
		
	public int GetGridDepthPosition()
	{
		return _gridDepthPosition;
	}


}
