using Namespace_NodeState;

public abstract class Script_BehaviourTreePart {
	public abstract NodeState RunNode(float p_delta);
	public abstract Script_BehaviourTree GetTree ();
}
