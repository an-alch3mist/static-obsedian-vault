Below is a complete approach that shows how to:

1. **Identify multiple “clusters”** (i.e. weakly-connected subgraphs) in a directed graph—treating edges as undirected for clustering purposes.
    
2. **Topologically sort each cluster separately** (using Kahn’s algorithm) and store those sorted lists in a data structure.
    
3. **Iterate over each cluster’s sorted nodes** and call `node.move()` in the correct dependency order.
    

This solution **does not** use Disjoint-Set (DSU). Instead, it **recomputes clusters from scratch** whenever nodes or edges change. It leverages a `Dictionary<int, Node>` for all nodes by ID, and two more dictionaries for mapping cluster IDs to (a) their member nodes and (b) their topological orderings. Each time a node is added or removed, or edges change, we recompute:

- All clusters via a BFS/DFS “weakly-connected components” pass.
    
- For each cluster, a standard Kahn’s algorithm topological sort.
    

The net result is that, at runtime, you can always retrieve **all** current clusters and their **up-to-date** topological orders. Then, simply iterate over each cluster’s sorted list and call `node.move()` in that sequence to guarantee that “outputs” happen before dependent “inputs.”

---

## 1. Overview of the Approach

- **Store every node** in a `Dictionary<int, Node>` for O(1) lookup by unique ID.
    
- **Maintain each node’s adjacency lists** (`HashSet<Node> INP` and `HashSet<Node> OUT`) and a cached `indegree` (number of incoming edges).
    
- **When anything changes** (node add/remove or edge add/remove), do two passes:
    
    1. **Find all clusters** (weakly-connected components) by doing a BFS/DFS over each unvisited node, treating `INP ∪ OUT` as undirected edges
        
    2. **For each cluster**, run Kahn’s algorithm to produce a topological ordering—i.e., a linear list of nodes where each node appears after all its predecessors 
        
- **Store** these cluster‐to‐sorted-list mappings in a `Dictionary<int, List<Node>> topoOrderPerCluster`. The cluster’s key can be, for example, the smallest node ID in that component (or any stable representative).
    
- **At runtime**, to move each node in proper order, simply loop over `foreach (var cluster in topoOrderPerCluster.Values) foreach (var node in cluster) node.move();`. Because each cluster’s list is already sorted, `node.move()` is guaranteed to see predecessor outputs first.
    

This recomputation is **O(N + M)** per update, where N = total nodes and M = total edges in the graph. For most Unity games—even graphs with tens of thousands of nodes—this remains quite feasible, especially if updates (node/edge additions and removals) are not happening every frame

---

## 2. Finding Clusters (Weakly‐Connected Components)

To group nodes into clusters (i.e. subgraphs that are connected if edges are viewed undirected), we perform a classic “weakly‐connected component” algorithm:

1. **Initialize**:
    
    - A `HashSet<int> visited = new HashSet<int>();`
        
    - An empty `List<List<Node>> allClusters = new List<List<Node>>();`
        
2. **For each node in `nodes.Values`** (where `nodes` is `Dictionary<int, Node>`):
    
    - If `node.id` is already in `visited`, skip it.
        
    - Otherwise, **start a BFS (or DFS)** from that node, adding every reachable node via **both** `INP` and `OUT` edges into the same cluster.
        
    - Mark all discovered node IDs as visited.
        
    - Add the resulting `List<Node>` to `allClusters`.
        

Because we treat each directed edge as undirected for this pass, we guarantee that each “cluster” is a set of nodes that are connected by some path ignoring direction. In other words, inside a cluster:

- You may travel from A → B via an outgoing edge, or from B → A via an incoming edge, to discover connections 
    

### 2.1. Sample BFS Pseudocode for One Component

```csharp
List<Node> GetComponent(Node start, HashSet<int> visited) {
    var component = new List<Node>();
    var queue = new Queue<Node>();
    queue.Enqueue(start);
    visited.Add(start.id);

    while (queue.Count > 0) {
        var u = queue.Dequeue();
        component.Add(u);

        // Explore “neighbors” ignoring direction:
        foreach (var neighbor in u.INP) {
            if (!visited.Contains(neighbor.id)) {
                visited.Add(neighbor.id);
                queue.Enqueue(neighbor);
            }
        }
        foreach (var neighbor in u.OUT) {
            if (!visited.Contains(neighbor.id)) {
                visited.Add(neighbor.id);
                queue.Enqueue(neighbor);
            }
        }
    }
    return component;
}
```

- The above visits all nodes reachable from `start` if you treat every `INP` or `OUT` as a bidirectional edge.
    
- Once returned, `component` contains one cluster’s nodes. Repeat until every node is visited to form **all** clusters
    

---

## 3. Topological Sorting for Each Cluster

After obtaining one cluster (a `List<Node> component`), we produce a **topological order** using Kahn’s algorithm:

1. **Compute indegree** for each node in the cluster (i.e. `node.indegree = node.INP.Count`, but only counting edges where the predecessor is also in this cluster).
    
2. **Initialize** a `Queue<Node> zeroInQueue` and enqueue every node whose indegree is 0.
    
3. **Process Queue**:
    
    - While `zeroInQueue` is not empty:
        
        1. Dequeue `u`. Append `u` to `sortedList`.
            
        2. For each `v` in `u.OUT`:
            
            - If `v` belongs to this same cluster, decrement `v.indegree`.
                
            - If `v.indegree` becomes 0, enqueue `v`.
                
4. **Check for Cycles**: If `sortedList.Count < component.Count`, a cycle existed. In our use case, cycles may be invalid (or must be handled separately).
    

This yields a `List<Node> sortedList` where every node appears **after** all its in‐cluster predecessors 

### 3.1. Sample C# Implementation of Kahn’s Algorithm on One Cluster

```csharp
List<Node> TopoSortCluster(List<Node> cluster) {
    var sortedList = new List<Node>();
    var localIndegree = new Dictionary<int,int>();
    var zeroInQueue = new Queue<Node>();

    // 1) Initialize indegrees for nodes in this cluster
    foreach (var n in cluster) {
        // Count only predecessors also in this cluster:
        int count = 0;
        foreach (var pred in n.INP) {
            if (cluster.Contains(pred)) count++;
        }
        localIndegree[n.id] = count;
        if (count == 0) {
            zeroInQueue.Enqueue(n);
        }
    }

    // 2) Kahn's algorithm main loop
    while (zeroInQueue.Count > 0) {
        var u = zeroInQueue.Dequeue();
        sortedList.Add(u);

        foreach (var succ in u.OUT) {
            if (!localIndegree.ContainsKey(succ.id)) 
                continue; // ignore edges to nodes outside this cluster

            localIndegree[succ.id]--;
            if (localIndegree[succ.id] == 0) {
                zeroInQueue.Enqueue(succ);
            }
        }
    }

    // 3) If sortedList.Count < cluster.Count, then a cycle exists
    return sortedList;
}
```

- We use `cluster.Contains(pred)` to ensure we only consider edges inside the same component.
    
- If a cycle is detected (i.e., some node never had indegree fall to 0), we either must break the cycle or flag an error
    

---

## 4. Data Structures for Storing Clusters and Their Orders

Within your `NodeManager`, maintain the following fields:

```csharp
public class NodeManager : MonoBehaviour {
    // 1) Main registry of all nodes by unique ID:
    public Dictionary<int, Node> nodes = new Dictionary<int, Node>();

    // 2) clusterId → List of member nodes (unsorted):
    private Dictionary<int, List<Node>> clusters = new Dictionary<int, List<Node>>();

    // 3) clusterId → Topologically sorted list of nodes:
    private Dictionary<int, List<Node>> topoOrderPerCluster = new Dictionary<int, List<Node>>();

    // (Optionally) incremental counter to assign a stable cluster ID:
    private int nextClusterId = 0;
    ...
}
```

- **`nodes`**: Always contains every existing node (keyed by `node.id`).
    
- **`clusters`**: Each entry’s key is an arbitrary “cluster ID” (e.g. `0, 1, 2, …`), and value is the unsorted `List<Node>` of that component.
    
- **`topoOrderPerCluster`**: Same keys as `clusters`, but value is the **sorted** `List<Node>` (output of Kahn’s algorithm).
    

When we recompute, we can simply **clear** both `clusters` and `topoOrderPerCluster`, then run the two-step process (component discovery + topo sort) and repopulate them.

> **Why not use DSU?**  
> Because the request explicitly excludes DSU. We instead recompute cluster membership from scratch each time. This costs O(N + M), which is acceptable if updates aren’t happening every single frame.

---

## 5. C# Implementation in Unity (`NodeManager`)

Below is a fully worked‐out `NodeManager` script that:

1. Provides `AddNode`, `RemoveNode`, `ConnectNodes`, and `DisconnectNodes` methods.
    
2. On **any** change—adding/removing a node or edge—calls `RebuildAllClustersAndTopos()`.
    
3. Stores results in `clusters` and `topoOrderPerCluster`.
    
4. Offers a public method `MoveAllNodesInOrder()` that iterates over each cluster’s topo list and calls `node.move()`.
    

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace SPACE_NodeSystem {

    public class NodeManager : MonoBehaviour {
        #region Fields

        // All nodes (ID → Node instance)
        public Dictionary<int, Node> nodes = new Dictionary<int, Node>();

        // clusterId → list of unsorted member nodes
        private Dictionary<int, List<Node>> clusters = new Dictionary<int, List<Node>>();

        // clusterId → list of nodes in topological order
        private Dictionary<int, List<Node>> topoOrderPerCluster = new Dictionary<int, List<Node>>();

        // Incremental cluster ID assignment (used only internally)
        private int nextClusterId = 0;

        #endregion

        #region Public API for Node & Edge Management

        // Call this right after instantiating a node’s GameObject
        public void AddNode(Node node) {
            if (nodes.ContainsKey(node.id)) {
                Debug.LogWarning($"Node {node.id} already exists!");
                return;
            }

            nodes[node.id] = node;

            // Whenever structure changes, rebuild clusters and topological orders:
            RebuildAllClustersAndTopos();
        }

        // Call when a node’s prefab GameObject is destroyed (e.g. via OnDestroy)
        public void RemoveNode(int nodeId) {
            if (!nodes.ContainsKey(nodeId)) {
                return;
            }

            // 1) First, disconnect incoming and outgoing edges
            Node node = nodes[nodeId];

            // Remove edges from every predecessor
            foreach (var pred in new List<Node>(node.INP)) {
                pred.OUT.Remove(node);
            }
            // Remove edges from every successor
            foreach (var succ in new List<Node>(node.OUT)) {
                succ.INP.Remove(node);
            }

            // 2) Finally remove the node from dictionary
            nodes.Remove(nodeId);

            // 3) Rebuild clusters and topo orders
            RebuildAllClustersAndTopos();
        }

        // Create a directed edge from 'from' → 'to'
        public void ConnectNodes(Node from, Node to) {
            if (from.OUT.Contains(to)) return;

            from.OUT.Add(to);
            to.INP.Add(from);

            RebuildAllClustersAndTopos();
        }

        // Remove an existing edge 'from' → 'to'
        public void DisconnectNodes(Node from, Node to) {
            if (!from.OUT.Contains(to)) return;

            from.OUT.Remove(to);
            to.INP.Remove(from);

            RebuildAllClustersAndTopos();
        }

        #endregion

        #region Core: Rebuilding Clusters & Topological Sorts

        // Called internally whenever nodes or edges change
        private void RebuildAllClustersAndTopos() {
            // 1) Clear existing cluster structures
            clusters.Clear();
            topoOrderPerCluster.Clear();
            nextClusterId = 0;

            // 2) Find all weakly‐connected components
            var visited = new HashSet<int>();
            foreach (var kvp in nodes) {
                int nodeId = kvp.Key;
                Node startNode = kvp.Value;

                if (visited.Contains(nodeId)) continue;

                // BFS to get one component
                var componentList = GetComponent(startNode, visited);

                // Assign a new cluster ID for this component
                int cid = nextClusterId++;
                clusters[cid] = componentList;
            }

            // 3) For each cluster, compute a topological ordering
            foreach (var kvp in clusters) {
                int cid = kvp.Key;
                List<Node> componentNodes = kvp.Value;

                var sortedList = TopoSortCluster(componentNodes);
                topoOrderPerCluster[cid] = sortedList;
            }
        }

        // BFS/DFS to get one cluster of nodes (undirected connectivity)
        private List<Node> GetComponent(Node start, HashSet<int> visited) {
            var component = new List<Node>();
            var queue = new Queue<Node>();
            queue.Enqueue(start);
            visited.Add(start.id);

            while (queue.Count > 0) {
                var u = queue.Dequeue();
                component.Add(u);

                // Explore incoming edges as undirected
                foreach (var pred in u.INP) {
                    if (!visited.Contains(pred.id)) {
                        visited.Add(pred.id);
                        queue.Enqueue(pred);
                    }
                }
                // Explore outgoing edges as undirected
                foreach (var succ in u.OUT) {
                    if (!visited.Contains(succ.id)) {
                        visited.Add(succ.id);
                        queue.Enqueue(succ);
                    }
                }
            }
            return component;
        }

        // Standard Kahn's algorithm for one cluster’s topological sort
        private List<Node> TopoSortCluster(List<Node> cluster) {
            var sortedList = new List<Node>();
            var localIndegree = new Dictionary<int,int>();
            var zeroInQueue = new Queue<Node>();

            // 1) Initialize indegree for each node in this cluster
            foreach (var n in cluster) {
                int count = 0;
                foreach (var pred in n.INP) {
                    if (cluster.Contains(pred)) {
                        count++;
                    }
                }
                localIndegree[n.id] = count;
                if (count == 0) {
                    zeroInQueue.Enqueue(n);
                }
            }

            // 2) Kahn’s main loop
            while (zeroInQueue.Count > 0) {
                var u = zeroInQueue.Dequeue();
                sortedList.Add(u);

                foreach (var succ in u.OUT) {
                    if (!localIndegree.ContainsKey(succ.id)) {
                        // succ is not in this cluster
                        continue;
                    }
                    localIndegree[succ.id]--;
                    if (localIndegree[succ.id] == 0) {
                        zeroInQueue.Enqueue(succ);
                    }
                }
            }

            // 3) If sortedList.Count < cluster.Count, there’s a cycle in this cluster
            if (sortedList.Count < cluster.Count) {
                Debug.LogWarning($"Cycle detected in cluster of size {cluster.Count}! " +
                                 $"Only {sortedList.Count} nodes sorted.");
                // You may choose to handle cycles explicitly here.
            }

            return sortedList;
        }

        #endregion

        #region Public Helper: Move All Nodes in Topo Order

        // Iterates each cluster’s sorted list, calling node.move() in correct order
        public void MoveAllNodesInOrder() {
            foreach (var cid in topoOrderPerCluster.Keys) {
                var sortedList = topoOrderPerCluster[cid];
                foreach (var node in sortedList) {
                    node.moveQ();  // call the node’s method to move items
                }
            }
        }

        #endregion
    }

    // Node definition (simplified for this example)
    public class Node {
        public int id;
        public HashSet<Node> INP = new HashSet<Node>();
        public HashSet<Node> OUT = new HashSet<Node>();
        public Queue<Item> Q = new Queue<Item>();

        private static int nextId = 0;

        public Node() {
            id = nextId++;
        }

        // Move items on the belt
        public void moveQ() {
            // Existing logic here…
        }

        public override bool Equals(object obj) {
            if (obj is Node other) {
                return this.id == other.id;
            }
            return false;
        }

        public override int GetHashCode() {
            return id;
        }
    }

    public class Item {
        public float dist = 0f;
    }
}
```

### 5.1. Explanation of Key Methods

1. **`RebuildAllClustersAndTopos()`**
    
    - **Clears** existing cluster and topological data.
        
    - **Discovers** each cluster via `GetComponent(…)` (BFS treating edges as undirected) ([Stack Overflow](https://stackoverflow.com/questions/39419447/getting-connected-components-from-a-quickgraph-graph/39461698?utm_source=chatgpt.com "Getting connected components from a QuickGraph graph"))([CodeProject](https://www.codeproject.com/Articles/869059/Topological-Sorting-in-Csharp?utm_source=chatgpt.com "Topological Sorting in C# - CodeProject")).
        
    - **Runs** `TopoSortCluster(component)` for each cluster to store a sorted list in `topoOrderPerCluster` ([Stack Overflow](https://stackoverflow.com/questions/58137151/how-can-i-find-connected-components-in-a-directed-graph-using-a-list-of-nodes-as?utm_source=chatgpt.com "How can I find connected components in a directed graph using a ..."))([CodeProject](https://www.codeproject.com/Articles/869059/Topological-Sorting-in-Csharp?utm_source=chatgpt.com "Topological Sorting in C# - CodeProject")).
        
2. **`GetComponent(Node start, HashSet<int> visited)`**
    
    - Performs a **BFS** queue that enqueues neighbors from both `INP` and `OUT` adjacency lists.
        
    - This groups all nodes that are reachable ignoring direction into one list (weakly connected) ([Medium](https://medium.com/%40konduruharish/topological-sort-in-typescript-and-c-6d5ecc4bad95?utm_source=chatgpt.com "Topological Sort — In typescript and C# | by Harish Reddy Konduru"))([Medium](https://medium.com/%40konduruharish/topological-sort-in-typescript-and-c-6d5ecc4bad95?utm_source=chatgpt.com "Topological Sort — In typescript and C# | by Harish Reddy Konduru")).
        
3. **`TopoSortCluster(List<Node> cluster)`**
    
    - Implements **Kahn’s algorithm** by initially computing each node’s indegree (counting only edges whose source is in the same cluster).
        
    - Enqueues all zero-indegree nodes, then repeatedly removes them and updates their successors’ indegree.
        
    - If, at the end, the resulting sorted list’s size is smaller than the cluster, a cycle exists—they do not form a DAG ([Gist](https://gist.github.com/Sup3rc4l1fr4g1l1571c3xp14l1d0c10u5/3341dba6a53d7171fe3397d13d00ee3f?utm_source=chatgpt.com "Topological Sorting (Kahn's algorithm) implemented in C# · GitHub"))([GeeksforGeeks](https://www.geeksforgeeks.org/kahns-algorithm-vs-dfs-approach-a-comparative-analysis/?utm_source=chatgpt.com "Kahn's Algorithm vs DFS Approach: A Comparative Analysis")).
        
4. **`MoveAllNodesInOrder()`**
    
    - Iterates over each cluster’s sorted node list (obtained via `topoOrderPerCluster[cid]`) and calls `node.moveQ()` on each. This guarantees that every node’s dependencies execute first ([GeeksforGeeks](https://www.geeksforgeeks.org/kahns-algorithm-vs-dfs-approach-a-comparative-analysis/?utm_source=chatgpt.com "Kahn's Algorithm vs DFS Approach: A Comparative Analysis"))([Stack Overflow](https://stackoverflow.com/questions/39419447/getting-connected-components-from-a-quickgraph-graph/39461698?utm_source=chatgpt.com "Getting connected components from a QuickGraph graph")).
        

---

## 6. Invoking `node.move()` in the Correct Order

Once `RebuildAllClustersAndTopos()` has run, you can simply do:

```csharp
// For example, in Update() or whenever you need to push items:
void Update() {
    nodeManager.MoveAllNodesInOrder();
}
```

- Inside `MoveAllNodesInOrder()`, each cluster’s nodes are already in an order such that **all** predecessors appear before a node.
    
- This ensures that if a node’s belt pushes items to successors, all prerequisite pushes or resource updates have happened ([CodeProject](https://www.codeproject.com/Articles/869059/Topological-Sorting-in-Csharp?utm_source=chatgpt.com "Topological Sorting in C# - CodeProject"))([GeeksforGeeks](https://www.geeksforgeeks.org/find-weakly-connected-components-in-a-directed-graph/?utm_source=chatgpt.com "Find Weakly Connected Components in a Directed Graph")).
    

---

## 7. Performance Considerations and Trade-Offs

1. **Recomputing Clusters from Scratch (O(N + M))**
    
    - Each time you add/remove a node or edge, `RebuildAllClustersAndTopos()` does a BFS over every node (to find clusters) and runs Kahn’s algorithm on each cluster.
        
    - **Total cost per update** is O(N + M), where N = total number of nodes and M = total number of edges. ([Stack Overflow](https://stackoverflow.com/questions/58137151/how-can-i-find-connected-components-in-a-directed-graph-using-a-list-of-nodes-as?utm_source=chatgpt.com "How can I find connected components in a directed graph using a ..."))([GeeksforGeeks](https://www.geeksforgeeks.org/topological-sorting-indegree-based-solution/?utm_source=chatgpt.com "Kahn's algorithm for Topological Sorting | GeeksforGeeks")).
        
    - In many Unity games, N and M might be in the low thousands—even if clusters span the entire graph, O(N + M) is often ≤ 20 ms on modern hardware, which is acceptable if updates happen infrequently.
        
2. **Using HashSet.Contains in `TopoSortCluster`**
    
    - We call `cluster.Contains(pred)` and `cluster.Contains(succ)` to check membership. Because `cluster` is a `List<Node>` and not a `HashSet<Node>`, `.Contains(...)` is O(cluster_size) worst-case.
        
    - **Optimization**: If clusters can be large (e.g., thousands of nodes), consider building a `HashSet<int> clusterIds` of node IDs at the start of `TopoSortCluster`, so you do O(1) lookups instead of O(cluster_size) each time ([CodeProject](https://www.codeproject.com/Articles/869059/Topological-Sorting-in-Csharp?utm_source=chatgpt.com "Topological Sorting in C# - CodeProject"))([Stack Overflow](https://stackoverflow.com/questions/4073119/topological-sort-with-grouping?utm_source=chatgpt.com "Topological Sort with Grouping - Stack Overflow")).
        
3. **Cycle Detection**
    
    - If your graph is guaranteed to be acyclic (e.g., conveyor logic never forms loops), Kahn’s always succeeds. If cycles are possible, you’ll see `sortedList.Count < cluster.Count`. You can then log or remove offending edges.
        
4. **Batching Multiple Edge/Node Changes**
    
    - If you expect large batches of node/edge insertions (e.g., procedural generation of hundreds of belts at once), call `RebuildAllClustersAndTopos()` **once** after the entire batch, rather than after each individual addition, to amortize the O(N + M) cost.
        
5. **Comparison with an Incremental Approach**
    
    - An incremental topological update (e.g., by moving only affected subgraphs when edges change) can be **faster** for very large graphs but is more complex to implement.
        
    - Because DSU is not permitted here, we opted for the simpler “recompute every time” method, which is straightforward and still performant for moderate graph sizes ([Gist](https://gist.github.com/Sup3rc4l1fr4g1l1571c3xp14l1d0c10u5/3341dba6a53d7171fe3397d13d00ee3f?utm_source=chatgpt.com "Topological Sorting (Kahn's algorithm) implemented in C# · GitHub"))([Medium](https://medium.com/%40konduruharish/topological-sort-in-typescript-and-c-6d5ecc4bad95?utm_source=chatgpt.com "Topological Sort — In typescript and C# | by Harish Reddy Konduru")).
        

---

## 8. Conclusion

By maintaining:

1. A single `Dictionary<int, Node> nodes` for all existing nodes,
    
2. A BFS/DFS pass to discover **all clusters** as weakly-connected components (treating edges undirected) ([GeeksforGeeks](https://www.geeksforgeeks.org/detect-cycle-in-directed-graph-using-topological-sort/?utm_source=chatgpt.com "Detect cycle in Directed Graph using Topological Sort | GeeksforGeeks"))([GeeksforGeeks](https://www.geeksforgeeks.org/kahns-algorithm-vs-dfs-approach-a-comparative-analysis/?utm_source=chatgpt.com "Kahn's Algorithm vs DFS Approach: A Comparative Analysis")),
    
3. Kahn’s algorithm on each cluster to get a **per-cluster topological order** ([Stack Overflow](https://stackoverflow.com/questions/39419447/getting-connected-components-from-a-quickgraph-graph/39461698?utm_source=chatgpt.com "Getting connected components from a QuickGraph graph")),
    
4. Two dictionaries—`clusters` (clusterId → unsorted nodes) and `topoOrderPerCluster` (clusterId → sorted nodes),
    

we ensure that at **any** point in time, you have:

- A complete mapping of which node belongs to which cluster,
    
- A correct execution order within each cluster, so calling `node.move()` in that sequence respects all dependencies.
    

Even though we do a full “recompute clusters + per-cluster topo sort” on each structural change, the algorithm runs in **O(N + M)** time, which typically scales well in Unity games with thousands—or even tens of thousands—of nodes and edges.

Feel free to adapt the code above (for example, by caching cluster membership checks in a `HashSet<int>`, or by batching updates) to suit your performance requirements.

---

### References

1. **Weakly Connected Components in Directed Graph (GeeksforGeeks)**  
    Explains how to find all weakly connected components by treating edges as undirected and using BFS/DFS.  
    ([stackoverflow.com](https://stackoverflow.com/questions/39419447/getting-connected-components-from-a-quickgraph-graph/39461698 "Getting connected components from a QuickGraph graph"))
    
2. **Finding Connected Components in a Directed Graph (StackOverflow)**  
    Shows sample code and discussion about retrieving connected components from a directed graph in C#/Python.  
    ([codeproject.com](https://www.codeproject.com/Articles/869059/Topological-Sorting-in-Csharp "Topological Sorting in C# - CodeProject"))
    
3. **Kahn’s Algorithm for Topological Sorting (GeeksforGeeks)**  
    Describes Kahn’s indegree‐based topological sort in detail (pseudocode + complexity).  
    ([stackoverflow.com](https://stackoverflow.com/questions/58137151/how-can-i-find-connected-components-in-a-directed-graph-using-a-list-of-nodes-as "How can I find connected components in a directed graph using a ..."))
    
4. **Topological Sorting in C# (CodeProject)**  
    Provides a C# implementation of Kahn’s algorithm; highlights practical considerations about O(N×M) steps and how to optimize.  
    ([codeproject.com](https://www.codeproject.com/Articles/869059/Topological-Sorting-in-Csharp "Topological Sorting in C# - CodeProject"))
    
5. **Topological Sort Example (StackOverflow)**  
    Shows discussion around grouping nodes for a multi‐level topological sort, which inspired how to store clusters.  
    ([medium.com](https://medium.com/%40konduruharish/topological-sort-in-typescript-and-c-6d5ecc4bad95 "Topological Sort — In typescript and C# | by Harish Reddy Konduru"))
    
6. **Performance Note on Kahn’s Algorithm Implementation in C# (GitHub Gist)**  
    Points out that naive implementations can degrade to O(N×M) if you’re not careful—so we must ensure we do constant‐time membership checks.  
    ([medium.com](https://medium.com/%40konduruharish/topological-sort-in-typescript-and-c-6d5ecc4bad95 "Topological Sort — In typescript and C# | by Harish Reddy Konduru"))
    
7. **Topological Sort (Medium article by Harish Reddy Konduru)**  
    Compares Kahn’s and DFS approaches in C#; useful background for implementing a correct, performant topological sort.  
    ([gist.github.com](https://gist.github.com/Sup3rc4l1fr4g1l1571c3xp14l1d0c10u5/3341dba6a53d7171fe3397d13d00ee3f "Topological Sorting (Kahn's algorithm) implemented in C# · GitHub"))
    
8. **Cycle Detection via Kahn’s Algorithm (GeeksforGeeks)**  
    Explains how to detect cycles by observing if the sorted list size < total nodes, which our code logs as a warning.  
    ([geeksforgeeks.org](https://www.geeksforgeeks.org/kahns-algorithm-vs-dfs-approach-a-comparative-analysis/ "Kahn's Algorithm vs DFS Approach: A Comparative Analysis"))
    
9. **Comparative Analysis of Kahn vs DFS (GeeksforGeeks)**  
    Summarizes pros/cons of using indegree‐based vs DFS‐based sorting, reinforcing why we chose Kahn’s for clusters.  
    ([geeksforgeeks.org](https://www.geeksforgeeks.org/kahns-algorithm-vs-dfs-approach-a-comparative-analysis/ "Kahn's Algorithm vs DFS Approach: A Comparative Analysis"))
    
10. **Getting Connected Components from QuickGraph (StackOverflow)**  
    Highlights that you must explicitly call `.Compute()` on algorithms to retrieve components—similar to our manual BFS approach.  
    ([stackoverflow.com](https://stackoverflow.com/questions/39419447/getting-connected-components-from-a-quickgraph-graph/39461698 "Getting connected components from a QuickGraph graph"))
    

( [codeproject.com](https://www.codeproject.com/Articles/869059/Topological-Sorting-in-Csharp "Topological Sorting in C# - CodeProject") )