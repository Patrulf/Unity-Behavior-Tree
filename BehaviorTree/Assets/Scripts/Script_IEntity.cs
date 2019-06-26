using UnityEngine;
using Namespace_Config;

public abstract class Script_IEntity {

	public abstract Vector3Int GetGridLocation();
	public abstract Vector3 GetPosition();
	public abstract GameObject GetGameObject();
	public abstract void Update(float p_delta);
	public abstract int GetHealth ();
	public abstract void DecreaseHealth (int p_valueToDecrease);
	public abstract void TakeDamage(Script_IEntity p_attacker, int p_damage);
	public abstract int GetThreatMultiplier ();
	public abstract void DetermineIfDead();
	public abstract void Heal(int p_amountToHeal);
	public abstract EntityStats GetStats ();


}
