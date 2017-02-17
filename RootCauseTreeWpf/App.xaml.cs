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
            DispatcherUnhandledException += App_DispatcherUnhandledException;
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

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ErrorLogger.GetInstance().LogError(e.Exception);
            e.Handled = true;
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

        private class ErrorLogger
        {
            static ErrorLogger logger;
            private ErrorLogger() { }
            internal static ErrorLogger GetInstance()
            {
                if (logger == null)
                    logger = new ErrorLogger();
                return logger;
            }

            internal void LogError(Exception e)
            {
                var stream = new System.IO.StreamWriter("Error Log.txt", true);
                stream.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")} LOGGED EXCEPTION: {e.Message}");
                var originalE = e;

                while (e.InnerException != null)
                {
                    e = e.InnerException;
                    stream.WriteLine($"INNER EXCEPTION: {e.Message}");
                }
                stream.WriteLine($"STACK TRACE: {originalE.StackTrace}\n");

                stream.Flush();
                stream.Dispose();
                stream = null;

                MessageBox.Show($"The following error was encountered: {originalE.Message}","Error");
            }
        }
    }
}
