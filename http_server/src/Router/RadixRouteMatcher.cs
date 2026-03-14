using System.Text;

namespace http_server.Router;

internal class RadixRouteMatcher<T> : IRouteMatcher<T>, IRouteMatcherBuilder<T>
{
    private readonly char delimiter= '/';
    private RadixNode<T> root = new RadixNode<T>(default(T));
    internal RadixNode<T> Root => root;

    public bool TryAddRoute(string path, T data)
    {
        
        if (string.IsNullOrEmpty(path)) return false;
        
        // Special case for root route registering
        if (path.Equals("/", StringComparison.OrdinalIgnoreCase))
        {
            if (root.isRoute)
                return false;
            
            root.Data = data;
            root.isRoute = true;
            return true;
        }

        var node = root;
        var suffix = path.AsSpan();
        suffix = suffix.Trim(delimiter);

        while (NextSegment(suffix, out ReadOnlySpan<char> segment, out suffix))
        {
            //Main case if node with key doesnt exist create one
            if (node.Next.TryGetValue(segment.ToString(), out var parentNode))
            {
                //If uncompressed node just continue
                if (String.IsNullOrEmpty(parentNode.suffix) && !suffix.IsEmpty)
                {
                    node = parentNode;
                    continue;
                }
                NextSegment(parentNode.suffix, out var parentSegment, out var parentSuffix);
                // Compression/suffix match
                if(suffix.StartsWith(parentNode.suffix, StringComparison.Ordinal))
                {
                    if (suffix.Equals(parentNode.suffix, StringComparison.Ordinal))
                    {
                        //exact match but its a route already
                        //or update non route to a route
                        if (parentNode.isRoute)
                        {
                            return false;   
                        }
                        else
                        {
                            parentNode.Data = data;
                            parentNode.isRoute = true;
                            return true;
                        }
                    }
                    else
                    {
                        suffix = suffix[parentNode.suffix.Length..];
                        node = parentNode;
                        continue;
                    }
                }
                else
                {
                    //this is where tree diverges on insert so need to move current node to a child and then update
                    // With either parent node (insert between old and its parent) or append new sibling next to old.
                    var dict = parentNode.Next;
                    parentNode.Next = new ();

                    var movedNode = parentNode.InsertRoute(parentNode.Data, parentSegment, parentSuffix);
                    movedNode.Next = dict;

                    if (suffix.IsEmpty)
                    {
                        parentNode.isRoute = true;
                        parentNode.Data = data;
                        parentNode.suffix = default;
                        return true;
                    }
                    else
                    {
                        parentNode.isRoute = false;
                        parentNode.Data = default;
                        parentNode.suffix = default;
                        node = parentNode;
                    }
                }

            }
            else
            {
                node.InsertRoute(data, segment.ToString(), suffix.ToString());
                return true;
            }
        }

        throw new InvalidOperationException("Unreachable code in TryAddRoute.");
    }

    public IRouteMatcher<T> Compile()
    {
        return this;
    }

    //TODO add path params and wildcards
    public bool TryMatchRoute(string path, out T route)
    {
        route = default(T);
        if (string.IsNullOrEmpty(path)) return false;
        
        //Special case for root route
        if (path.Equals("/", StringComparison.OrdinalIgnoreCase) && root.isRoute)
        {
            route = root.Data;
            return true;
        }
            
        var node = root;

        ReadOnlySpan<char> pathSpan = path.AsSpan();
        while (node != null && NextSegment(pathSpan, out ReadOnlySpan<char> segment, out pathSpan))
        {
            // !!! Updating node so that if suffix match next iter exits !!! 
            //TODO consider other solutions without allocations (.ToString())
            if (!node.Next.TryGetValue(segment.ToString(), out node))
                return false;
            
            // Checking if full match with the route
            if (node.isRoute &&
                (
                    (pathSpan.IsEmpty && string.IsNullOrEmpty(node.suffix)) ||
                    (!string.IsNullOrEmpty(node.suffix) &&
                     pathSpan.Equals(node.suffix, StringComparison.Ordinal))
                ))
            {
                route = node.Data;
                return true;
            }

            //If not full match but suffix matches take node suffix and continue searching
            if (!pathSpan.StartsWith(node.suffix, StringComparison.Ordinal))
                return false;
            
            pathSpan = String.IsNullOrEmpty(node.suffix) ? pathSpan : pathSpan[node.suffix.Length..];
        }
        return false;
    }

    private bool NextSegment(ReadOnlySpan<char> path, out ReadOnlySpan<char> segment, out ReadOnlySpan<char> pathRest)
    {
        segment = default;
        pathRest = default;
        
        path = path.Trim(delimiter);
        
        if (path.IsEmpty)
            return false;
        
        var delimIndex = path.IndexOf(delimiter);
        if (delimIndex > -1)
        {
            segment = path[..delimIndex];
            pathRest = path[(delimIndex + 1)..];
            return !segment.IsEmpty;
            
        }

        segment = path;
        pathRest = ReadOnlySpan<char>.Empty;
        return !segment.IsEmpty;
    }

    public override string ToString()
    {
        return root.ToString();
    }

    internal sealed class RadixNode<T>
    {
        public T Data;
        public Dictionary<string, RadixNode<T>> Next;
        public bool isRoute;
        public string suffix;
        public RadixNode(T data)
        {
            isRoute = false;
            Data = data;
            Next = new(StringComparer.Ordinal);
        }

        public RadixNode<T> InsertRoute(T data, ReadOnlySpan<char> segment, ReadOnlySpan<char> suffix)
        {
            var newNode = new RadixNode<T>(data);
            newNode.isRoute = true;
            newNode.suffix = suffix.ToString();
            this.Next.TryAdd(segment.ToString(), newNode);
            return newNode;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"Suffix: {suffix} Is Route: {isRoute}");
            return sb.ToString();
        }

        // private void WriteTo(StringBuilder sb, string indent, bool isLast, string label)
        // {
        //     sb.Append(indent);
        //
        //     if (!string.IsNullOrEmpty(indent))
        //         sb.Append(isLast ? "\\-- " : "|-- ");
        //
        //     sb.Append(label);
        //
        //     if (!string.IsNullOrEmpty(suffix))
        //         sb.Append(" [").Append(suffix).Append(']');
        //
        //     if (isRoute)
        //         sb.Append(" *");
        //
        //     sb.AppendLine();
        //
        //     var children = Next.ToList();
        //     for (int i = 0; i < children.Count; i++)
        //     {
        //         var child = children[i];
        //         bool childIsLast = i == children.Count - 1;
        //
        //         child.Value.WriteTo(
        //             sb,
        //             indent + (string.IsNullOrEmpty(indent) ? "" : (isLast ? "    " : "|   ")),
        //             childIsLast,
        //             child.Key);
        //     }
        // }
    }
}