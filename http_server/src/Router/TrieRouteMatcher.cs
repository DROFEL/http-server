using System.Text;

namespace http_server.Router;

public class TrieRouteMatcher<T>
{
    private readonly char delimiter= '/';
    private Node<T> root = new Node<T>(default(T));

    public bool TryAddRoute(string path, T data)
    {
        
        if (string.IsNullOrEmpty(path)) return false;
        
        // Special case for root route registering
        if (path.Equals("/", StringComparison.OrdinalIgnoreCase))
        {
            if (root.Data != null)
                return false;
            
            root.Data = data;
            root.isRoute = true;
            return true;
        }

        var node = root;
        var pathSpan = path.AsSpan();
        pathSpan = pathSpan.Trim(delimiter);

        while (NextSegment(pathSpan, out ReadOnlySpan<char> segment, out pathSpan))
        {
            if (node.Next.TryGetValue(segment.ToString(), out var parentNode))
            {
                if (String.IsNullOrEmpty(parentNode.suffix))
                {
                    node = parentNode;
                    continue;
                }
                NextSegment(parentNode.suffix, out var parentSegment, out var parentSuffix);
                if(pathSpan.StartsWith(parentNode.suffix, StringComparison.Ordinal))
                {
                    // Path already exists
                    if (pathSpan.Equals(parentNode.suffix, StringComparison.Ordinal))
                    {
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
                        node.InsertRoute(data, segment, pathSpan);
                        return true;
                    }
                
                    //If not then take node suffix and continue searching
                }
                else
                {
                    var dict = parentNode.Next;
                    parentNode.Next = new ();

                    var movedNode = parentNode.InsertRoute(parentNode.Data, parentSegment, parentSuffix);
                    movedNode.Next = dict;

                    if (pathSpan.IsEmpty)
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
                node.InsertRoute(data, segment.ToString(), pathSpan.ToString());
                return true;
            }
        }

        return false;
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
            // !!! Updating node so that if no match next iter exits !!!
            //TODO consider other solutions without allocations
            if (node.Next.TryGetValue(segment.ToString(), out node)
                && pathSpan.StartsWith(node.suffix, StringComparison.Ordinal))
            {
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
                
                //If not then take node suffix and continue searching
                pathSpan = String.IsNullOrEmpty(node.suffix) ? pathSpan : pathSpan[node.suffix.Length..];
            }
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

    private class Node<T>
    {
        public T Data;
        public Dictionary<string, Node<T>> Next;
        public bool isRoute;
        public string suffix;
        public Node(T data)
        {
            isRoute = false;
            Data = data;
            Next = new(StringComparer.Ordinal);
        }

        public Node<T> InsertRoute(T data, ReadOnlySpan<char> segment, ReadOnlySpan<char> suffix)
        {
            var newNode = new Node<T>(data);
            newNode.isRoute = true;
            newNode.suffix = suffix.ToString();
            this.Next.TryAdd(segment.ToString(), newNode);
            return newNode;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            WriteTo(sb, "", true, "root");
            return sb.ToString();
        }

        private void WriteTo(StringBuilder sb, string indent, bool isLast, string label)
        {
            sb.Append(indent);

            if (!string.IsNullOrEmpty(indent))
                sb.Append(isLast ? "\\-- " : "|-- ");

            sb.Append(label);

            if (!string.IsNullOrEmpty(suffix))
                sb.Append(" [").Append(suffix).Append(']');

            if (isRoute)
                sb.Append(" *");

            sb.AppendLine();

            var children = Next.ToList();
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                bool childIsLast = i == children.Count - 1;

                child.Value.WriteTo(
                    sb,
                    indent + (string.IsNullOrEmpty(indent) ? "" : (isLast ? "    " : "|   ")),
                    childIsLast,
                    child.Key);
            }
        }
    }
}