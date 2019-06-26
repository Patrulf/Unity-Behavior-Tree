using Namespace_Config;
using Namespace_NodeState;

public class Script_Action_SetFlag : Script_Action {


	private blackboard_flags _flag;
	private object _object;
	private Script_BehaviourTree _tree;

	public Script_Action_SetFlag(Script_BehaviourTree p_tree,object p_valueToSet, blackboard_flags p_flag)
	{
		_object = p_valueToSet;
		_flag = p_flag;
		_tree = p_tree;


	}

	public override Script_BehaviourTree GetTree()
	{
		return _tree;
	}

	public override NodeState RunNode (float p_delta)
	{
		_tree.SetBlackboardElement (_flag, _object);
		return NodeState.Success;

	}



}
