using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using com.PorcupineSupernova.RootCauseTreeCore;

namespace com.PorcupineSupernova.RootCauseTreeWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public Graphing.RootCauseGraph GenerateGraph(Node node)
        {
            var newGraph = new Graphing.RootCauseGraph();
            var vertices = new Dictionary<long, Graphing.RootCauseVertex>();
            var problem = node;
            bool vertexExists;

            vertices.Add(problem.NodeId, new Graphing.RootCauseVertex(problem));
            newGraph.AddVertex(vertices[problem.NodeId]);

            Func<Node, bool> recurseNodes = null;
            recurseNodes = (Node parent) =>
            {
                foreach (var child in parent.ChildNodes)
                {
                    vertexExists = vertices.ContainsKey(child.NodeId);

                    if (!vertexExists)
                    {
                        vertices.Add(child.NodeId, new Graphing.RootCauseVertex(child));
                        newGraph.AddVertex(vertices[child.NodeId]);
                    }

                    newGraph.AddEdge(new Graphing.RootCauseEdge(vertices[parent.NodeId], vertices[child.NodeId]));
                    if (!vertexExists) recurseNodes(child);
                }
                return true;
            };

            recurseNodes(problem);
            return newGraph;
        }
    }
}
