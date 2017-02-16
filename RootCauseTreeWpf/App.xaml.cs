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
        internal GraphSharp.Algorithms.Layout.Simple.Tree.SimpleTreeLayoutParameters algs;

        public App()
        {
            algs = new GraphSharp.Algorithms.Layout.Simple.Tree.SimpleTreeLayoutParameters();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Count() > 0)
            {
                Properties["StartupFile"] = e.Args[0].ToString();
            }

            var startupData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
            if (startupData != null && startupData.ActivationData.Count() > 0)
            {
                string fileRaw = startupData.ActivationData[0];
                var fileUri = new Uri(fileRaw);
                Properties["StartupFile"] = fileUri.LocalPath;
            }

            base.OnStartup(e);
        }

        public Graphing.RootCauseGraph GenerateGraph(Node node)
        {
            var newGraph = new Graphing.RootCauseGraph();
            if (node == null) return newGraph;
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
