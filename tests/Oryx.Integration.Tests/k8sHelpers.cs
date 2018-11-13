using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;

namespace Oryx.Integration.Tests
{
    static class k8sHelpers
    {
        private static readonly TimeSpan OBJECT_WAIT_TIMEOUT = TimeSpan.FromMinutes(4);

        internal static bool IsPodRunning(V1Pod pod)
        {
            return string.Equals(pod.Status?.Phase, "Running");
        }

        /// <summary>
        /// Synchronously executes an arbitrary command in the first container of given pod.
        /// </summary>
        /// <param name="ops">Kubernetes client</param>
        /// <param name="pod">Kubernetes pod to execute command in</param>
        /// <param name="@namespace">Pod's namespace in cluster</param>
        /// <param name="commands">Shell commands to execute</param>
        /// <returns>Output of commands</returns>
        internal async static Task<string> ExecInPodAsync(IKubernetes ops, V1Pod pod, string @namespace, string[] commands)
        {
            var webSockTask = ops.WebSocketNamespacedPodExecAsync(pod.Metadata.Name, @namespace, commands, pod.Spec.Containers[0].Name,
                stderr: true, stdin: false, stdout: true, tty: false);

            using (var webSock = await webSockTask)
            using (var demux = new StreamDemuxer(webSock))
            {
                demux.Start();

                using (var demuxStream = demux.GetStream(1, 1))
                using (StreamReader reader = new StreamReader(demuxStream))
                    return await reader.ReadToEndAsync();
            }
        }

        internal async static Task<V1Pod> CreatePodAndWait(IKubernetes ops, V1Pod spec, string @namespace, Func<V1Pod, bool> stopPredicate)
        {
            var newPod = ops.CreateNamespacedPod(spec, @namespace);
            Console.WriteLine("Created pod \"{0}\"", newPod.Metadata.Name);

            try
            {
                newPod = await WaitForNamespacedObject(ops, newPod, @namespace, stopPredicate);
            }
            catch (IllegalClusterStateException exc)
            {
                // At this point, either the object hasn't gotten to the required state, or the watcher missed a change.
                // To ensure the former, we'll read the object from the cluster again.
                Console.WriteLine("Watcher failed; refreshing object \"{0}\"...", newPod.Metadata.Name);
                newPod = ops.ReadNamespacedPod(spec.Metadata.Name, @namespace);
                if (!stopPredicate(newPod))
                    throw exc;
            }

            return newPod;
        }

        internal async static Task<V1Service> CreateServiceAndWait(IKubernetes ops, V1Service spec, string @namespace, Func<V1Service, bool> stopPredicate)
        {
            var newSvc = ops.CreateNamespacedService(spec, @namespace);
            Console.WriteLine("Created service \"{0}\"", newSvc.Metadata.Name);

            try
            {
                newSvc = await WaitForNamespacedObject(ops, newSvc, @namespace, stopPredicate);
            }
            catch (IllegalClusterStateException exc)
            {
                // At this point, either the object hasn't gotten to the required state, or the watcher missed a change.
                // To ensure the former, we'll read the object from the cluster again.
                Console.WriteLine("Watcher failed; refreshing object \"{0}\"...", newSvc.Metadata.Name);
                newSvc = ops.ReadNamespacedService(spec.Metadata.Name, @namespace);
                if (!stopPredicate(newSvc))
                    throw exc;
            }

            return newSvc;
        }

        internal async static Task<V1Deployment> CreateDeploymentAndWait(IKubernetes ops, V1Deployment spec, string @namespace, Func<V1Deployment, bool> stopPredicate)
        {
            var newDeployment = ops.CreateNamespacedDeployment(spec, @namespace);
            Console.WriteLine("Created deployment \"{0}\"", newDeployment.Metadata.Name);

            try
            {
                newDeployment = await WaitForNamespacedObject(ops, newDeployment, @namespace, stopPredicate);
            }
            catch (IllegalClusterStateException exc)
            {
                // At this point, either the object hasn't gotten to the required state, or the watcher missed a change.
                // To ensure the former, we'll read the object from the cluster again.
                Console.WriteLine("Watcher failed; refreshing object \"{0}\"...", newDeployment.Metadata.Name);
                newDeployment = ops.ReadNamespacedDeployment(spec.Metadata.Name, @namespace);
                if (!stopPredicate(newDeployment))
                    throw exc;
            }

            return newDeployment;
        }

        /// <summary>
        /// Watches the given object, blocking the current thread until the object is at a required state.
        /// </summary>
        /// <param name="ops">Kubernetes client</param>
        /// <param name="obj">Kubernetes object to watch</param>
        /// <param name="@namespace">Namespace in which `obj` was created</param>
        /// <param name="stopPredicate">A function that should return `true` when `obj` is at the required state</param>
        /// <returns>Most recent version of the object, as received from the Kubernetes API server</returns>
        private async static Task<T> WaitForNamespacedObject<T>(IKubernetes ops, T obj, string @namespace, Func<T, bool> stopPredicate) where T : IKubernetesObject
        {
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            bool waitHandleSignalled;
            string path = GetObjectPath(obj, @namespace);
            Console.WriteLine("Watching object @ \"{0}\"", path);

            T mostRecentKobject = default(T);
            using (var watcher = await ops.WatchObjectAsync<T>(
                path,
                timeoutSeconds: (int)OBJECT_WAIT_TIMEOUT.TotalSeconds,
                onEvent: (et, updatedObj) =>
                {
                    Console.WriteLine("Received {0} event for object @ \"{1}\"", et, path);
                    mostRecentKobject = updatedObj;

                    if (stopPredicate(updatedObj))
                    {
                        Console.WriteLine("Predicate returned true; signalling wait handle");
                        waitHandle.Set();
                    }
                },
                onError: exc => throw new IllegalClusterStateException("Error while watching object", exc)
            ))
            {
                waitHandleSignalled = waitHandle.WaitOne(OBJECT_WAIT_TIMEOUT);
            }

            if (!waitHandleSignalled)
                throw new IllegalClusterStateException(string.Format("Wait handle was not signalled for object @ \"{0}\"", path));

            return mostRecentKobject;
        }

        private static string GetObjectPath(IKubernetesObject obj, string @namespace)
        {
            string prefix = "api", kind;
            V1ObjectMeta meta;

            if (obj is V1Pod)
            {
                kind = "pods";
                meta = ((V1Pod)obj).Metadata;
            }
            else if (obj is V1Service)
            {
                kind = "services";
                meta = ((V1Service)obj).Metadata;
            }
            else if (obj is V1Deployment)
            {
                kind = "deployments";
                prefix = "apis/apps";
                meta = ((V1Deployment)obj).Metadata;
            }
            else
            {
                throw new NotSupportedException("GetObjectPath only supports Pods, Services and Deployments.");
            }

            // Path structure validated against https://github.com/kubernetes-client/csharp/blob/master/src/KubernetesClient/generated/Kubernetes.Watch.cs
            return $"{prefix}/v1/watch/namespaces/{@namespace}/{kind}/{meta.Name}";
        }

        public class IllegalClusterStateException : Exception
        {
            public IllegalClusterStateException(string msg) : base(msg) { }
            public IllegalClusterStateException(string msg, Exception inner) : base(msg, inner) { }
        }
    }
}
