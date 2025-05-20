

- **Use a `class Node`**, not a struct. Structs copy by value and become expensive once you include a `HashSet<Node>` or other mutable data.
    
- Store each node’s neighbors in a `HashSet<Node>`, overriding `Equals` and `GetHashCode` on `node_id`. This gives O(1) membership checks/additions/removals for neighbor lists.
    
- Keep all nodes in a `Dictionary<int, Node>` inside `NodeManager`. That shows you an O(1) way to fetch any node by `id`.
    
- **For node removal, do not use per-node events** to propagate unlinking; instead, inside `RemoveNode`, loop over `removed.Neighbors` and call `nbr.Neighbors.Remove(removed)`—all O(1) average calls. This approach is simpler, uses less memory, and avoids delegate invocation overhead at the scale of thousands to tens of thousands of nodes.