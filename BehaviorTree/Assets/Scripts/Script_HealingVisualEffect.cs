using UnityEngine;

public class Script_HealingVisualEffect : Script_IVisual {

	private GameObject _effectObject;
	private TextMesh _textMesh;

	private Vector3 _directionToMove;

	private float _lifeTimerCurrent;
	private float _lifeTimer;

	private Script_GameManager _manager;


	public Script_HealingVisualEffect(Script_GameManager p_manager,Script_IEntity p_entityToRecieveEffect)
	{
		_lifeTimer = 1.0f;
		_lifeTimerCurrent = 0.0f;
		_manager = p_manager;

		_effectObject = new GameObject();
		_effectObject.name = "SheepObject";
		_effectObject.transform.position = p_entityToRecieveEffect.GetPosition();

		_textMesh = _effectObject.AddComponent<TextMesh> ();
		_textMesh.text = "Healing Target!";

		_textMesh.fontSize = 8;
		_textMesh.color = Color.green;
		_textMesh.transform.rotation = Quaternion.Euler (90, 0, 0);

		_directionToMove = new Vector3 (0.0f, 1.0f, 0.0f);

	}

	public override void Update(float p_delta)
	{
		_effectObject.transform.position += _directionToMove * p_delta;

		_lifeTimerCurrent += p_delta;

		if (_lifeTimerCurrent >= _lifeTimer) {
			Destruction ();
		}
	}

	public override void Destruction()
	{
		_manager.DestroyTextMesh (_textMesh);
		_manager.DestroyGameObject (_effectObject);
		_textMesh = null;
		_manager.RemoveVisual (this);
	}

}
