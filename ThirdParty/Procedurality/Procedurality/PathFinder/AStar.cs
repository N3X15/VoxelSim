using System;
using System.Collections.Generic;

namespace Theodis.Algorithm
{
    /// <summary>
    /// An ordered queue.
    /// </summary>
    /// <typeparam name="T">The type of the items in the queue.</typeparam>
    public class PriorityQueue<T>
    {
        private List<T> L;
        private Comparison<T> cmp;

        private void Fix(int i)
        {
            int child = i + 1;
            T item = L[i];
            while (child < L.Count)
            {
                if (child + 1 < L.Count && cmp(L[child], L[child + 1]) > 0)
                    child++;
                if (cmp(item, L[child]) > 0)
                {
                    L[i] = L[child];
                    i = child;
                }
                else
                    break;
                child = i + 1;
            }
            L[i] = item;
        }
        /// <summary>
        /// Removes an item at index i in the queue.
        /// </summary>
        /// <param name="i">Index of the item to be removed.</param>
        public void Remove(int i)
        {
            L[i] = L[L.Count - 1];
            L.RemoveAt(L.Count - 1);
            if (i < L.Count - 1)
                Fix(i);
        }

        public bool Empty { get { return L.Count == 0; } }
        public T this[int i] { get { return L[i]; } }
        public int Count { get { return L.Count; } }
        /// <summary>
        /// Adds an item to the queue.
        /// </summary>
        /// <param name="d">Item to be added.</param>
        public void Enqueue(T d)
        {
            int i, j;
            L.Add(d);
            i = L.Count - 1;
            j = i >> 1;
            while (i > 0 && cmp(L[j], L[i]) > 0)
            {
                T tmp = L[i];
                L[i] = L[j];
                L[j] = tmp;
                i = j;
                j >>= 1;
            }
        }

        /// <summary>
        /// Removes the item at the front of the queue.
        /// </summary>
        /// <returns>Item at the front of the queue.</returns>
        public T Dequeue()
        {
            T ret = L[0];
            Remove(0);
            return ret;
        }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="compare">The function that determines the order of the items on the queue.</param>
        public PriorityQueue(Comparison<T> compare)
        {
            L = new List<T>();
            cmp = compare;
        }

        /// <summary>
        /// Cuts the length of the queue by removing items at the end of the line.
        /// </summary>
        /// <param name="newlen">The new length of the queue.</param>
        public void Cut(int newlen)
        {
            if(newlen < L.Count)
                L.RemoveRange(newlen, L.Count - newlen);
        }

    }

    class PathNode<T>
    {
        public T source;
        public PathNode<T> prevNode;
        public double f;
        public double g;
        public double h;
        public int nt;

         public PathNode(T s, PathNode<T> p, double pg, double ph, int pnt, Dictionary<T, double> bestF)
        {
            source = s;
            prevNode = p;
            g = pg;
            h = ph;
            f = g + h;
            nt = pnt;
            if (bestF.ContainsKey(s))
                bestF[s] = f;
            else
                bestF.Add(s, f);
        }
    }
    /// <summary>
    /// NOT_FOUND - This node is not a node being searched for
    /// FINISHED - This node is a node being searched for and
    ///            no more nodes are being searched for.
    /// ADD_PATH - This node is a node being searched for but
    ///            continue looking for more.
    /// </summary>
    public enum FinishedFlags
    {
        NOT_FOUND = 0,
        FINISHED = 1,
        ADD_PATH = 2
    }

    /// <summary>
    /// INTERIOR - All the nodes leading to the finishing nodes.
    /// FINISHED - The nodes with the terminating condition.
    /// </summary>
    public enum DijkstraIncludeFlags
    {
        INTERIOR = 1,
        FINISHED = 2,
        BOTH = 3
    }


    public class Pathfinder<T>
    {
        public delegate FinishedFlags Finished(T node, double g);
        public delegate List<T> Adjacent(T node);
        public delegate double Distance(T a, T b);
        public delegate int NodeDist(T a, T b);

        private static int PathWeightCompare(PathNode<T> a, PathNode<T> b)
        {
            return (int)(a.f - b.f);
        }

        /// <summary>
        /// A* pathfinder.
        /// </summary>
        /// <param name="start">The node to begin the search at.</param>
        /// <param name="end">The node to find from the start.</param>
        /// <param name="adj">A function which returns nodes adjacent to the passed in node.</param>
        /// <param name="dist">A function that gives the distance and estimated distance between nodes.</param>
        /// <param name="maxnodes">The maximum number of nodes to keep on the open list.  0 for unlimited.</param>
        /// <param name="maxnodedepth">The maximum path length that can be made.  0 for unlimited.</param>
        /// <param name="mintargetdist">The minimum acceptable distance to the target before ending the search.</param>
        /// <param name="minnodedist">A function that returns the minimum number of node transitions between 2 nodes.  Pass in null if no such function is availible.  Optimizes search time in conjunction with a passed in minnodedepth.</param>
        /// <returns>A list of nodes going from start to end or null in the event no path could be found.</returns>
        public static List<T> AStar(T start, T end, Adjacent adj, Distance dist, int maxnodes, int maxnodedepth, int mintargetdist, NodeDist minnodedist)
        {
            Comparison<PathNode<T>> pwc = new Comparison<PathNode<T>>(PathWeightCompare);
            PriorityQueue<PathNode<T>> open = new PriorityQueue<PathNode<T>>(pwc);
            Dictionary<T, double> bestF = new Dictionary<T, double>();
            List<T> path = null;

            open.Enqueue(new PathNode<T>(start, null, 0, dist(start, end), 0, bestF));
            while (!open.Empty)
            {
                PathNode<T> cur = open.Dequeue();
                bool closeenough = false;
                if (mintargetdist > 0)
                    closeenough = dist(cur.source, end) <= mintargetdist;
                if (cur.source.Equals(end) || closeenough)
                {
                    Stack<T> s = new Stack<T>();
                    path = new List<T>();
                    s.Push(cur.source);
                    while (cur.prevNode != null)
                    {
                        cur = cur.prevNode;
                        s.Push(cur.source);
                    }
                    while (s.Count > 0)
                        path.Add(s.Pop());
                    break;
                }
                List<T> L = adj(cur.source);
                if (minnodedist != null && maxnodedepth != 0)
                {
                    if (minnodedist(cur.source, end) + cur.nt >= maxnodedepth)
                        continue;
                }
                else if (maxnodedepth != 0)
                    if (cur.nt >= maxnodedepth)
                        continue;
                foreach (T d in L)
                {
                    double ng = cur.g + dist(cur.source, d);
                    if (bestF.ContainsKey(d))
                    {
                        if (ng + dist(d, end) < bestF[d])
                        {
                            for (int i = 0; i < open.Count; i++)
                                if (open[i].source.Equals(d))
                                {
                                    open.Remove(i);
                                    break;
                                }
                        }
                        else
                            continue;
                    }
                    open.Enqueue(new PathNode<T>(d, cur, ng, dist(d, end), cur.nt + 1, bestF));
                }
                if (maxnodes != 0 && open.Count > maxnodes)
                    open.Cut(maxnodes);
            }
            return path;
        }
        /// <summary>
        /// Dijkstra pathfinder.
        /// </summary>
        /// <param name="start">The node to begin the search at.</param>
        /// <param name="adj">A function which returns nodes adjacent to the passed in node.</param>
        /// <param name="dist">A function that gives the distance between nodes.</param>
        /// <param name="fin">A function that returns whether or not the node passed in is the end of the search.</param>
        /// <param name="maxnodedepth">The maximum path length.</param>
        /// <returns>A list of paths to the different finishing nodes found.</returns>
        public static List<List<T>> Dijkstra(T start,Adjacent adj, Distance dist, Finished fin, int maxnodedepth)
        {
            Comparison<PathNode<T>> pwc = new Comparison<PathNode<T>>(PathWeightCompare);
            PriorityQueue<PathNode<T>> open = new PriorityQueue<PathNode<T>>(pwc);
            Dictionary<T, double> bestF = new Dictionary<T, double>();
            List<List<T>> path = new List<List<T>>();

            open.Enqueue(new PathNode<T>(start, null, 0, 0, 0, bestF));
            while (!open.Empty)
            {
                PathNode<T> cur = open.Dequeue();
                FinishedFlags isDone = fin(cur.source, cur.g);
                if (isDone != 0)
                {
                    Stack<T> s = new Stack<T>();
                    s.Push(cur.source);
                    while (cur.prevNode != null)
                    {
                        cur = cur.prevNode;
                        s.Push(cur.source);
                    }
                    path.Add(new List<T>());
                    while (s.Count > 0)
                        path[path.Count-1].Add(s.Pop());
                }
                if ((isDone & FinishedFlags.FINISHED) != 0)
                    break;

                List<T> L = adj(cur.source);

                if (maxnodedepth != 0 && cur.nt >= maxnodedepth)
                    continue;

                foreach (T d in L)
                {
                    double ng = cur.g + dist(cur.source, d);
                    if (bestF.ContainsKey(d))
                    {
                        if (ng < bestF[d])
                        {
                            for (int i = 0; i < open.Count; i++)
                                if (open[i].source.Equals(d))
                                {
                                    open.Remove(i);
                                    break;
                                }
                        }
                        else
                            continue;
                    }
                    open.Enqueue(new PathNode<T>(d, cur, ng, 0, cur.nt + 1, bestF));
                }

            }
            return path;
        }
        /// <summary>
        /// Retrieves a field of nodes that were on the way to or the finish nodes themselves.
        /// </summary>
        /// <param name="start">The node to begin the search on.</param>
        /// <param name="adj">A function which returns nodes adjacent to the passed in node.</param>
        /// <param name="dist">A function that gives the distance between nodes.</param>
        /// <param name="fin">A function that returns whether or not the node passed in is an end point.</param>
        /// <param name="include">A set of flags for what to return.  INTERIOR are the nodes that lead to the FINISHED nodes.  BOTH returns botht he INTERIOR and FINISHED nodes.</param>
        /// <param name="maxnodedepth">The maximum length path.</param>
        /// <returns>A field of nodes that lead to the finishing nodes.</returns>
        public static List<T> DijkstraNodeInRange(T start, Adjacent adj, Distance dist, Finished fin, DijkstraIncludeFlags include, int maxnodedepth)
        {
            Comparison<PathNode<T>> pwc = new Comparison<PathNode<T>>(PathWeightCompare);
            PriorityQueue<PathNode<T>> open = new PriorityQueue<PathNode<T>>(pwc);
            Dictionary<T, double> bestF = new Dictionary<T, double>();
            List<T> closed = new List<T>();
            List<T> finishedL = new List<T>();

            open.Enqueue(new PathNode<T>(start, null, 0, 0, 0, bestF));
            while (!open.Empty)
            {
                PathNode<T> cur = open.Dequeue();
                closed.Add(cur.source);
                if (fin(cur.source, cur.g) != 0)
                {
                    finishedL.Add(cur.source);
                    continue;
                }

                List<T> L = adj(cur.source);

                if (maxnodedepth != 0 && cur.nt >= maxnodedepth)
                    continue;

                foreach (T d in L)
                {
                    double ng = cur.g + dist(cur.source, d);
                    if (bestF.ContainsKey(d))
                    {
                        if (ng < bestF[d])
                        {
                            for (int i = 0; i < open.Count; i++)
                                if (open[i].source.Equals(d))
                                {
                                    open.Remove(i);
                                    break;
                                }
                        }
                        else
                            continue;
                    }
                    open.Enqueue(new PathNode<T>(d, cur, ng, 0, cur.nt + 1, bestF));
                }

            }

            switch (include)
            {
                case DijkstraIncludeFlags.INTERIOR:
                    return closed;
                case DijkstraIncludeFlags.FINISHED:
                    return finishedL;
                case DijkstraIncludeFlags.BOTH:
                    foreach (T t in finishedL)
                        closed.Add(t);
                    return closed;
            }
            return null;

        }

    }
}