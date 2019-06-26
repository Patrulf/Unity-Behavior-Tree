using Namespace_NodeState;

public class Script_LeafTask : Script_TreeNode {

	private Script_Action _action;
	private Script_Condition _condition;
	private Script_BehaviourTree _tree;

	public Script_LeafTask(Script_BehaviourTree p_tree, Script_Action p_action)
	{
		_action = p_action;
		_condition = null;
		_tree = p_tree;
	}

	public Script_LeafTask(Script_BehaviourTree p_tree, Script_Condition p_condition)
	{
		_action = null;
		_condition = p_condition;
		_tree = p_tree;
	}

	public Script_LeafTask(Script_BehaviourTree p_tree, Script_Action p_action, Script_Condition p_condition)
	{
		_tree = p_tree;
		_action = p_action;
		_condition = p_condition;
	}

	public NodeState RunCondition(float p_delta)
	{
		return _condition.RunNode(p_delta);
	}
		
	public override NodeState RunNode(float p_delta)
	{
		if (_condition != null) {			
			if (RunCondition(p_delta) == NodeState.Success ) {
				if (_action != null) {
					NodeState actionState = _action.RunNode (p_delta);
					return actionState;
				}
				return NodeState.Success;
			}
			return NodeState.Failed;
		}
	
		NodeState ActionState = _action.RunNode (p_delta);
		return ActionState;
	}

	public override Script_BehaviourTree GetTree ()
	{
		return _tree;
	}

}

