namespace Namespace_Config {

	public enum blackboard_flags{
		flagOne,
		flagTwo,
		flagThree,
		flagFour,
		flagFive,
		flagSix,
		flagSeven,
		flagEight,
		flagNine,
		flagTen,
	};

	public struct EntityStats{
		public int _damage;
		public int _healPower;
		public int _maxHealth;
		public float _attackSpeed;

		public EntityStats(int p_damage,int p_healPower,int p_maxHealth, float p_attackSpeed)
		{
			_damage = p_damage;
			_healPower = p_healPower;
			_maxHealth = p_maxHealth;
			_attackSpeed = p_attackSpeed;

		}
	}

}
