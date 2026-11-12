namespace TaskFlow.Domain.Services;

using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Functional;

public static class DependencyGraphService
{
    // Build adjacency list from dependencies
    public static IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> BuildAdjacencyList(
        IEnumerable<TaskDependency> dependencies)
    {
        return dependencies
            .GroupBy(d => d.TaskId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<Guid>)g.Select(d => d.DependsOnTaskId).ToList().AsReadOnly());
    }
    
    // DFS cycle detection: O(V + E)
    public static bool HasCycle(IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> adjacencyList)
    {
        var visited = new HashSet<Guid>();
        var recursionStack = new HashSet<Guid>();
        
        foreach (var node in adjacencyList.Keys)
        {
            if (DetectCycleDfs(node, adjacencyList, visited, recursionStack))
                return true;
        }
        return false;
    }
    
    private static bool DetectCycleDfs(
        Guid node,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> adjacencyList,
        HashSet<Guid> visited,
        HashSet<Guid> recursionStack)
    {
        if (recursionStack.Contains(node)) return true;
        if (visited.Contains(node)) return false;
        
        visited.Add(node);
        recursionStack.Add(node);
        
        if (adjacencyList.TryGetValue(node, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                if (DetectCycleDfs(neighbor, adjacencyList, visited, recursionStack))
                    return true;
            }
        }
        
        recursionStack.Remove(node);
        return false;
    }
    
    // Validate adding a new dependency won't create a cycle
    public static Result<TaskDependency> ValidateNewDependency(
        Guid taskId, Guid dependsOnTaskId, 
        IEnumerable<TaskDependency> existingDependencies)
    {
        if (taskId == dependsOnTaskId)
            return Result<TaskDependency>.Failure("A task cannot depend on itself.");
        
        var deps = existingDependencies.ToList();
        var newDep = TaskDependency.Create(taskId, dependsOnTaskId);
        deps.Add(newDep);
        
        var adjacencyList = BuildAdjacencyList(deps);
        if (HasCycle(adjacencyList))
            return Result<TaskDependency>.Failure("Adding this dependency would create a circular dependency.");
        
        return Result<TaskDependency>.Success(newDep);
    }
    
    // Check if a task can be completed (all dependencies must be completed)
    public static Result<bool> CanCompleteTask(
        Guid taskId,
        IEnumerable<TaskDependency> dependencies,
        IEnumerable<TaskItem> allTasks)
    {
        var taskMap = allTasks.ToDictionary(t => t.Id);
        var deps = dependencies.Where(d => d.TaskId == taskId).ToList();
        
        var incompletePrereqs = deps
            .Where(d => taskMap.ContainsKey(d.DependsOnTaskId) && 
                        taskMap[d.DependsOnTaskId].Status != Enums.TaskItemStatus.Completed)
            .Select(d => taskMap[d.DependsOnTaskId].Title)
            .ToList();
        
        return incompletePrereqs.Count == 0
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(incompletePrereqs.Select(t => $"Prerequisite '{t}' is not completed."));
    }
    
    // Topological sort using Kahn's algorithm (prerequisites appear before dependents)
    public static Result<IReadOnlyList<Guid>> TopologicalSort(
        IEnumerable<Guid> taskIds,
        IEnumerable<TaskDependency> dependencies)
    {
        var depsList = dependencies.ToList();
        var cycleCheckAdj = BuildAdjacencyList(depsList);
        
        if (HasCycle(cycleCheckAdj))
            return Result<IReadOnlyList<Guid>>.Failure("Cannot sort: circular dependency detected.");
        
        var allIds = taskIds.ToHashSet();
        var inDegree = allIds.ToDictionary(id => id, _ => 0);
        
        // Build forward graph: Prerequisite -> List of Dependents
        var forwardGraph = allIds.ToDictionary(id => id, _ => new List<Guid>());
        
        foreach (var dep in depsList)
        {
            if (allIds.Contains(dep.TaskId) && allIds.Contains(dep.DependsOnTaskId))
            {
                forwardGraph[dep.DependsOnTaskId].Add(dep.TaskId);
                inDegree[dep.TaskId]++;
            }
        }
        
        var queue = new Queue<Guid>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var sorted = new List<Guid>();
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            sorted.Add(current);
            
            if (forwardGraph.TryGetValue(current, out var dependents))
            {
                foreach (var dependent in dependents)
                {
                    inDegree[dependent]--;
                    if (inDegree[dependent] == 0)
                        queue.Enqueue(dependent);
                }
            }
        }
        
        return Result<IReadOnlyList<Guid>>.Success(sorted.AsReadOnly());
    }
}
