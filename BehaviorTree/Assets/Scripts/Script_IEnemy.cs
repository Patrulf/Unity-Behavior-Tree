public abstract class Script_IEnemy : Script_IEntity {
	
	public abstract int GetThreat (Script_IEntity p_entity);
	public abstract void  SetTargetToAttack (Script_IEntity p_targetToAttack);
	public abstract Script_IEntity GetTargetToAttack();

}
