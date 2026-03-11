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
            if (node.Next.TryGetValue(segment.ToString(), out var nextNode))
            {

                // Path already exists
                if (pathSpan.Length == nextNode.segment.Length && nextNode.isRoute)
                {
                    return false;   
                }
                
                //If not then take node suffix and continue searching
                pathSpan = pathSpan[nextNode.segment.Length..];
                node = nextNode;
            }
            else
            {
                var newNode = new Node<T>(data);
                newNode.isRoute = true;
                newNode.segment = pathSpan.ToString();
                node.Next.Add(segment.ToString(), newNode);
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
                && pathSpan.StartsWith(node.segment, StringComparison.Ordinal))
            {
                // Checking if full match with the route
                if (pathSpan.Length == node.segment.Length && node.isRoute)
                {
                    route = node.Data;
                    return true;   
                }
                
                //If not then take node suffix and continue searching
                pathSpan = pathSpan[node.segment.Length..];
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
        public string segment;
        public Node(T data)
        {
            isRoute = false;
            Data = data;
            Next = new(StringComparer.Ordinal);
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

            if (!string.IsNullOrEmpty(segment))
                sb.Append(" [").Append(segment).Append(']');

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