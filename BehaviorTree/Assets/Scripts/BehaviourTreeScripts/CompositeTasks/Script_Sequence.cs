using System.Collections.Generic;
using System.Linq;
using Namespace_NodeState;

public class Script_Sequence : Script_CompositeNode {

	private List<Script_TreeNode> _leafList;
	private List<Script_TreeNode> _currentLeafList;

	private Script_BehaviourTree _tree;

	public Script_Sequence(Script_BehaviourTree p_tree)
	{
		_leafList = new List<Script_TreeNode> ();
		_currentLeafList = new List<Script_TreeNode> ();
		_tree = p_tree;
	}

	private void ResetNode()
	{
		_currentLeafList = new List<Script_TreeNode>(_leafList);
	}

	public override Script_BehaviourTree GetTree ()
	{
		return _tree;
	}


	public override List<Script_TreeNode> GetChildren()
	{
		return _leafList;
	}

	public void AddTask(Script_TreeNode p_task)
	{
		_leafList.Add (p_task);
		_currentLeafList.Add (p_task);
	}


	public override NodeState RunNode(float p_delta)
	{
		foreach(Script_TreeNode task in _currentLeafList.ToList())
		{
			NodeState taskReturnValue = task.RunNode (p_delta);

			if (_currentLeafList.Count == 1 && taskReturnValue != NodeState.Running) {
				ResetNode ();
				return taskReturnValue;
			}

			if (taskReturnValue == NodeState.Success) {
				_currentLeafList.Remove (task);
				continue;
			}
			if (taskReturnValue == NodeState.Running) {
				return NodeState.Running;
			}
			break;
		}
		ResetNode ();
		return NodeState.Failed;
	}

}
