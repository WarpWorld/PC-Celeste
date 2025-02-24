using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.CrowdControl;

internal static class Extensions
{
    public static Assembly CrowdControlAssembly { get; } = Assembly.GetExecutingAssembly();

    private static readonly string m_resourcePath = "Celeste.Mod.CrowdControl.";

    public static Texture2D LoadEmbeddedTexture(this GraphicsDevice graphicsDevice, string resourcePath)
    {
        resourcePath = m_resourcePath + resourcePath.Replace('\\', '.');
        using Stream? stream = CrowdControlAssembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource '{resourcePath}' not found.");

        return Texture2D.FromStream(graphicsDevice, stream);
    }

    [DebuggerStepThrough]
    public static async void Forget(this Task task)
    {
        try { await task.ConfigureAwait(false); }
        catch (Exception ex) { Log.Error(ex); }
    }

    public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        RegisteredWaitHandle registeredHandle = null;
        CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);
        try
        {
            var tcs = new TaskCompletionSource<bool>();
            registeredHandle = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut),
                tcs,
                millisecondsTimeout,
                true);
            tokenRegistration = cancellationToken.Register(
                state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
                tcs);
            return await tcs.Task;
        }
        finally
        {
            if (registeredHandle != null)
                registeredHandle.Unregister(null);
            tokenRegistration.Dispose();
        }
    }

    public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
    {
        return handle.WaitOneAsync((int)timeout.TotalMilliseconds, cancellationToken);
    }

    public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken cancellationToken)
    {
        return handle.WaitOneAsync(Timeout.Infinite, cancellationToken);
    }

    public static async Task<IDisposable> UseWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancelToken = default)
    {
        await semaphore.WaitAsync(cancelToken).ConfigureAwait(false);
        return new ReleaseWrapper(semaphore);
    }

    private class ReleaseWrapper : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        public ReleaseWrapper(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose()
        {
            if (_disposed) { return; }
            _semaphore.Release();
            _disposed = true;
        }
    }
}