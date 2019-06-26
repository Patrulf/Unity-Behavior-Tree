using System.Collections.Generic;

public abstract class Script_CompositeNode : Script_TreeNode {
	public abstract List<Script_TreeNode> GetChildren();
}
