using System.Collections.Generic;
using Namespace_NodeState;
using Namespace_Config;

public class Script_BehaviourTree {

	private List<Script_TreeNode> _childrenList;


	//Blackboard is based on blackboard written by Thomas Cairns: https://bitbucket.org/snippets/CairX/8eae8z
	private Dictionary<blackboard_flags,object> _blackBoard;

	public bool SetBlackboardElement(blackboard_flags p_key, object p_object)
	{
		_blackBoard[p_key] = p_object;
		return _blackBoard.ContainsKey (p_key);
	}

	public bool RemoveBlackboardElement(blackboard_flags p_key)
	{
		if (_blackBoard.ContainsKey (p_key)) {
			_blackBoard.Remove (p_key);
			return true;
		}
		return false;
	}

	public T GetBlackBoardElement<T> (blackboard_flags p_key)
	{
		if (_blackBoard.ContainsKey (p_key)) {
			return (T)_blackBoard [p_key];
		}
		return default (T);
	}




	public Script_BehaviourTree()
	{
		_blackBoard = new Dictionary<blackboard_flags,object> ();
		_childrenList = new List<Script_TreeNode> ();
	}


	public List<Script_TreeNode> GetChildren()
	{
		return _childrenList;
	}

	public void AddChild(Script_TreeNode p_child)
	{
		_childrenList.Add (p_child);
	}

	public void RunBehaviourTree(float p_delta)
	{
		foreach (Script_TreeNode node in _childrenList) {
			node.RunNode (p_delta);
		}
	}


}
