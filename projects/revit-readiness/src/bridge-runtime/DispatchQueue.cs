using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BimAgentLab.RevitBridge.Contract;

namespace BimAgentLab.RevitBridge.Runtime;

public enum DispatchSignalResult { Accepted, Pending, Denied, TimedOut }
// The adapter only schedules a later host callback; Raise never executes a reader.
public interface IDispatchSignal { DispatchSignalResult Raise(); }

public sealed class DispatchQueue
{
    readonly string session;
    readonly IDispatchSignal signal;
    readonly object gate = new object();
    readonly LinkedList<Entry> waiting = new LinkedList<Entry>();
    Entry? executing;
    bool stopped, draining, raising, signaled;
    long generation;
    sealed class Entry
    {
        internal readonly BridgeExecutionContext Context;
        internal readonly string RequestId;
        internal readonly TaskCompletionSource<BridgeResponse> Response = new TaskCompletionSource<BridgeResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly TaskCompletionSource<bool> Quiesced = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        internal CancellationTokenRegistration Registration;
        internal LinkedListNode<Entry>? Node;
        internal bool AdmissionDone, Finished, Retiring;
        internal Entry(BridgeExecutionContext context) { Context = context; RequestId = context.Request.RequestId; }
        internal DispatchTicket Ticket => new DispatchTicket(Response.Task, Quiesced.Task);
    }
    public DispatchQueue(string bridgeSessionId, IDispatchSignal signal)
    {
        if (string.IsNullOrWhiteSpace(bridgeSessionId)) throw new ArgumentException("A bridge session is required.", nameof(bridgeSessionId));
        session = bridgeSessionId; this.signal = signal ?? throw new ArgumentNullException(nameof(signal));
    }
    public DispatchTicket Enqueue(BridgeExecutionContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        var entry = new Entry(context);
        // Registration may invoke synchronously. AdmissionDone prevents premature
        // Quiesced before registration ownership has been assigned and admission ends.
        entry.Registration = context.TransportCancellation.Register(() => Cancel(entry));
        lock (gate)
        {
            if (!entry.Finished)
            {
                if (stopped) FinishError(entry, "BridgeUnavailable", "The bridge dispatcher is stopped.");
                else if (!SetInactive(entry))
                {
                    if (waiting.Count >= 4) FinishError(entry, "Busy", "The Revit request queue is full. Retry later.");
                    else entry.Node = waiting.AddLast(entry);
                }
            }
            entry.AdmissionDone = true;
        }
        Retire(entry);
        Signal(false);
        return entry.Ticket;
    }
    public void Drain(int maxBatch, Func<BridgeExecutionContext, BridgeResponse> execute)
    {
        if (maxBatch <= 0) throw new ArgumentOutOfRangeException(nameof(maxBatch));
        if (execute == null) throw new ArgumentNullException(nameof(execute));
        lock (gate)
        {
            if (stopped || draining) return;
            draining = true; signaled = false; generation++;
        }
        try
        {
            for (int count = 0; count < Math.Min(4, maxBatch); count++)
            {
                Entry entry;
                lock (gate)
                {
                    if (stopped || waiting.First == null) break;
                    entry = waiting.First.Value; waiting.RemoveFirst(); entry.Node = null; executing = entry;
                }
                try
                {
                    bool run;
                    lock (gate) { run = !stopped && !entry.Response.Task.IsCompleted && !SetInactive(entry); }
                    if (run)
                    {
                        entry.Context.ThrowIfInactive();
                        BridgeResponse response = execute(entry.Context); // API context belongs to the caller; no gate held
                        if (response == null || response.ContractVersion != 1 || response.RequestId != entry.RequestId || response.BridgeSessionId != session)
                            throw new InvalidOperationException("Executor returned an invalid response envelope.");
                        lock (gate)
                        {
                            if (!stopped && !entry.Response.Task.IsCompleted)
                                entry.Context.PublishIfActive(entry.Response, response);
                        }
                    }
                }
                catch (OperationCanceledException) { lock (gate) entry.Response.TrySetCanceled(); }
                catch (BridgeException error)
                {
                    lock (gate) entry.Response.TrySetResult(Error(entry, error.Error.Code, error.Error.Message, error.Error.Retryable));
                }
                catch (Exception)
                {
                    lock (gate) entry.Response.TrySetResult(Error(entry, "InternalError", "The Revit request could not be completed.", false));
                }
                finally
                {
                    lock (gate) { executing = null; entry.Finished = true; }
                    Retire(entry);
                }
            }
        }
        finally
        {
            lock (gate) draining = false;
            Signal(false); // Idling retries if Raise is still in flight or reports Pending.
        }
    }
    public void RearmIfNeeded() => Signal(true);
    public void CancelAll()
    {
        List<Entry> removed;
        lock (gate)
        {
            stopped = true; signaled = false;
            removed = new List<Entry>(waiting); waiting.Clear();
            foreach (var entry in removed) { entry.Node = null; entry.Finished = true; entry.Response.TrySetCanceled(); }
            executing?.Response.TrySetCanceled();
        }
        foreach (var entry in removed) Retire(entry);
    }
    void Cancel(Entry entry)
    {
        lock (gate)
        {
            if (entry.Finished) return;
            // Only bounded bookkeeping on the private control signal. No reader,
            // public-token notification, Raise, or lifetime disposal runs here.
            if (!SetInactive(entry)) entry.Response.TrySetCanceled();
            if (entry != executing)
            {
                if (entry.Node != null) waiting.Remove(entry.Node);
                entry.Node = null; entry.Finished = true;
            }
        }
        Retire(entry);
    }
    bool SetInactive(Entry entry)
    {
        try { entry.Context.ThrowIfInactive(); return false; }
        catch (OperationCanceledException) { entry.Response.TrySetCanceled(); }
        catch (BridgeException error) { entry.Response.TrySetResult(Error(entry, error.Error.Code, error.Error.Message, error.Error.Retryable)); }
        if (entry != executing) entry.Finished = true;
        return true;
    }
    void FinishError(Entry entry, string code, string message)
    {
        entry.Response.TrySetResult(Error(entry, code, message, code == "Busy" || code == "BridgeUnavailable")); entry.Finished = true;
    }
    BridgeResponse Error(Entry entry, string code, string message, bool retryable) => new BridgeResponse
    {
        ContractVersion = 1, RequestId = entry.RequestId, BridgeSessionId = session,
        Error = new BridgeError { Code = code, Message = message, RequestId = entry.RequestId, Retryable = retryable }
    };
    void Retire(Entry entry)
    {
        lock (gate)
        {
            if (!entry.Finished || !entry.AdmissionDone || entry.Retiring) return;
            entry.Retiring = true;
        }
        // Disposing can wait for a control callback needing gate. Never hold gate.
        // This source/registration is not lent to arbitrary executor callbacks.
        try { entry.Registration.Dispose(); entry.Quiesced.TrySetResult(true); }
        catch (Exception error) { entry.Quiesced.TrySetException(error); }
    }
    void Signal(bool force)
    {
        long observed;
        lock (gate)
        {
            if (stopped || draining || raising || waiting.Count == 0 || (signaled && !force)) return;
            raising = true; observed = generation;
        }
        DispatchSignalResult result;
        try { result = signal.Raise(); }
        catch (Exception) { result = DispatchSignalResult.Denied; }
        List<Entry>? rejected = null;
        lock (gate)
        {
            raising = false;
            if (result == DispatchSignalResult.Accepted || result == DispatchSignalResult.Pending)
                signaled = observed == generation && !stopped;
            else
            {
                signaled = false; rejected = new List<Entry>(waiting); waiting.Clear();
                foreach (var entry in rejected) { entry.Node = null; FinishError(entry, "Busy", "Revit could not schedule this request. Retry later."); }
            }
        }
        if (rejected != null) foreach (var entry in rejected) Retire(entry);
    }
}
