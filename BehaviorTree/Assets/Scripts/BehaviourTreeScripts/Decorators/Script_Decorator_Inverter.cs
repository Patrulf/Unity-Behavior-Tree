using Namespace_NodeState;
using UnityEngine.Assertions;

public class Script_Decorator_Inverter : Script_TreeNode {

	private Script_BehaviourTree _tree;

	private Script_TreeNode _childNode;

	public Script_Decorator_Inverter(Script_BehaviourTree p_tree, Script_TreeNode p_child)
	{
		_tree = p_tree;
		_childNode = p_child;
	}

	public override NodeState RunNode(float p_delta)
	{		
		Assert.IsNotNull (_childNode); 
		NodeState childState = _childNode.RunNode(p_delta);

		if (childState == NodeState.Failed)
			return NodeState.Success;
		if (childState == NodeState.Success)
			return NodeState.Failed;


		return NodeState.Running;
	}
		
	public override Script_BehaviourTree GetTree ()
	{
		return _tree;
	}

}
