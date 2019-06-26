using UnityEngine;

public class Script_Tile {
	
	private Vector3Int _position;
	private Vector3 _scale;
	private Vector3 _rotation;

	private Script_GameManager _manager;

	private GameObject _tileObject;

	private bool _isOccupied;

	private Material _objectMaterial;

	public Script_Tile(Vector3Int p_position)
	{
		_position = p_position;

		_tileObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
		_tileObject.name = "HealerObject";
		_objectMaterial = _tileObject.GetComponent<Renderer>().material;
		_objectMaterial.color = Color.white;
		_tileObject.transform.position = p_position;
		_tileObject.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
		_isOccupied = false;
	}




	public Vector3Int GetGridPosition()
	{
		return _position;
	}


	public bool GetOccupied()
	{
		return _isOccupied;
	}

	public void SetOccupied (bool p_occupation)
	{
		_isOccupied = p_occupation;
	}



}
