// 添加新的文件: ModuleDependencyResolver.cs
using System;
using System.Collections.Generic;
using System.Linq;
using SharpLoader.Modding.Models;
using SharpLoader.Utilities;

namespace SharpLoader.Core.Modding;

public class ModuleDependencyResolver
{
    private readonly LoggerService? _logger;
        
    public ModuleDependencyResolver(LoggerService? logger = null)
    {
        _logger = logger;
    }
        
    /// <summary>
    /// 解析模组依赖关系并返回加载顺序
    /// </summary>
    public List<(string, ModuleProfile)> ResolveDependencies(
        List<(string, ModuleProfile)> modules, 
        out List<string> missingDependencies,
        out List<string> versionMismatches)
    {
        missingDependencies = new List<string>();
        versionMismatches = new List<string>();
            
        // 创建模组ID到文件路径和配置的映射
        var moduleMap = modules.ToDictionary(
            m => m.Item2.Id, 
            m => (FilePath: m.Item1, Profile: m.Item2));
            
        // 构建依赖图
        var graph = new DependencyGraph();
            
        foreach (var (filePath, profile) in modules)
        {
            graph.AddNode(profile.Id);
                
            foreach (var dependency in profile.Dependencies)
            {
                // 检查依赖是否存在
                if (!moduleMap.ContainsKey(dependency.ModuleId))
                {
                    missingDependencies.Add($"{profile.Id} -> {dependency.ModuleId}");
                    continue;
                }
                    
                var dependencyProfile = moduleMap[dependency.ModuleId].Profile;
                    
                // 如果VersionRange为null，表示任意版本均可
                if (dependency.VersionRange == null)
                {
                    // 添加依赖边
                    graph.AddEdge(dependency.ModuleId, profile.Id);
                    continue;
                }
                    
                // 检查版本是否满足要求
                if (!dependency.VersionRange.IsSatisfiedBy(dependencyProfile.Version, dependencyProfile))
                {
                    versionMismatches.Add(
                        $"{profile.Id} requires {dependency.ModuleId} in range {dependency.VersionRange} " +
                        $"but found {dependencyProfile.Version}");
                    continue;
                }
                    
                // 添加依赖边
                graph.AddEdge(dependency.ModuleId, profile.Id);
            }
        }
            
        // 检查循环依赖
        if (graph.HasCycles(out var cycle))
        {
            throw new Exception($"Circular dependency detected: {string.Join(" -> ", cycle)}");
        }
            
        // 进行拓扑排序
        var sortedIds = graph.TopologicalSort();
            
        // 返回排序后的模组列表
        return sortedIds
            .Where(id => moduleMap.ContainsKey(id))
            .Select(id => (moduleMap[id].FilePath, moduleMap[id].Profile))
            .ToList();
    }
        
    /// <summary>
    /// 检查单个模组的依赖是否满足
    /// </summary>
    public bool CheckDependencies(ModuleProfile profile, Dictionary<string, ModuleProfile> availableModules)
    {
        foreach (var dependency in profile.Dependencies)
        {
            if (!availableModules.ContainsKey(dependency.ModuleId))
                return false;
                
            var dependencyProfile = availableModules[dependency.ModuleId];
                
            // 如果VersionRange为null，表示任意版本均可
            if (dependency.VersionRange == null)
                continue;
                
            // 检查版本是否满足要求
            if (!dependency.VersionRange.IsSatisfiedBy(dependencyProfile.Version, dependencyProfile))
                return false;
        }
            
        return true;
    }
}
    
/// <summary>
/// 依赖图实现，用于检测循环依赖和拓扑排序
/// </summary>
internal class DependencyGraph
{
    private readonly Dictionary<string, List<string>> _adjacencyList = new Dictionary<string, List<string>>();
    private readonly HashSet<string> _nodes = new HashSet<string>();
        
    public void AddNode(string node)
    {
        if (!_nodes.Contains(node))
        {
            _nodes.Add(node);
            _adjacencyList[node] = new List<string>();
        }
    }
        
    public void AddEdge(string from, string to)
    {
        AddNode(from);
        AddNode(to);
            
        if (!_adjacencyList[from].Contains(to))
        {
            _adjacencyList[from].Add(to);
        }
    }
        
    public bool HasCycles(out List<string> cycle)
    {
        cycle = new List<string>();
        var visited = new Dictionary<string, bool>();
        var recStack = new Dictionary<string, bool>();
            
        foreach (var node in _nodes)
        {
            visited[node] = false;
            recStack[node] = false;
        }
            
        foreach (var node in _nodes)
        {
            if (CheckCycles(node, visited, recStack, cycle))
            {
                return true;
            }
        }
            
        return false;
    }
        
    private bool CheckCycles(string node, Dictionary<string, bool> visited, Dictionary<string, bool> recStack, List<string> cycle)
    {
        if (!visited[node])
        {
            visited[node] = true;
            recStack[node] = true;
            cycle.Add(node);
                
            foreach (var neighbor in _adjacencyList.GetValueOrDefault(node, new List<string>()))
            {
                if (!visited[neighbor] && CheckCycles(neighbor, visited, recStack, cycle))
                {
                    return true;
                }
                else if (recStack[neighbor])
                {
                    cycle.Add(neighbor);
                    return true;
                }
            }
                
            cycle.RemoveAt(cycle.Count - 1);
        }
            
        recStack[node] = false;
        return false;
    }
        
    public List<string> TopologicalSort()
    {
        var result = new List<string>();
        var visited = new Dictionary<string, bool>();
            
        foreach (var node in _nodes)
        {
            visited[node] = false;
        }
            
        foreach (var node in _nodes)
        {
            if (!visited[node])
            {
                TopologicalSortUtil(node, visited, result);
            }
        }
            
        result.Reverse();
        return result;
    }
        
    private void TopologicalSortUtil(string node, Dictionary<string, bool> visited, List<string> result)
    {
        visited[node] = true;
            
        foreach (var neighbor in _adjacencyList.GetValueOrDefault(node, new List<string>()))
        {
            if (!visited[neighbor])
            {
                TopologicalSortUtil(neighbor, visited, result);
            }
        }
            
        result.Add(node);
    }
}