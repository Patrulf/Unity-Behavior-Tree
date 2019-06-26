using Namespace_NodeState;
using UnityEngine.Assertions;

public class Script_Decorator_NeverSucceed : Script_TreeNode {

	private Script_BehaviourTree _tree;
	private Script_TreeNode _parent;
	private Script_TreeNode _childNode;

	public Script_Decorator_NeverSucceed(Script_BehaviourTree p_tree, Script_TreeNode p_child)
	{
		_tree = p_tree;
		_childNode = p_child;
	}

	public override NodeState RunNode(float p_delta)
	{		
		Assert.IsNotNull (_childNode);

		NodeState returnValue = _childNode.RunNode(p_delta);
		if (returnValue == NodeState.Success)
			return NodeState.Failed;

		return returnValue;
	}

	public override Script_BehaviourTree GetTree ()
	{
		return _tree;
	}



}