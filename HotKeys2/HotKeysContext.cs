﻿using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Toolbelt.Blazor.HotKeys2;

/// <summary>
/// Current active hotkeys set.
/// </summary>
public partial class HotKeysContext : IDisposable
{
    /// <summary>
    /// The collection of Hotkey entries.
    /// </summary>
    public List<HotKeyEntry> Keys { get; } = new List<HotKeyEntry>();

    private readonly Task<IJSObjectReference> _AttachTask;

    private readonly ILogger _Logger;

    /// <summary>
    /// Initialize a new instance of the HotKeysContext class.
    /// </summary>
    internal HotKeysContext(Task<IJSObjectReference> attachTask, ILogger logger)
    {
        this._AttachTask = attachTask;
        this._Logger = logger;
    }

    // ===============================================================================================

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Action action, string description = "", string excludeSelector = "input, textarea")
    => this.AddInternal(ModKey.None, key, _ => { action(); return ValueTask.CompletedTask; }, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Action<HotKeyEntryByKey> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(ModKey.None, key, arg => { action(arg); return ValueTask.CompletedTask; }, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<ValueTask> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(ModKey.None, key, _ => action(), description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Key key, Func<HotKeyEntryByKey, ValueTask> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(ModKey.None, key, arg => action(arg), description, excludeSelector, action.Target as IHandleEvent);

    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Action action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(modifiers, key, _ => { action(); return ValueTask.CompletedTask; }, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Action<HotKeyEntryByKey> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(modifiers, key, arg => { action(arg); return ValueTask.CompletedTask; }, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<ValueTask> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(modifiers, key, _ => action(), description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModKey modifiers, Key key, Func<HotKeyEntryByKey, ValueTask> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(modifiers, key, action, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey by key.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="ownerOfAction">The owner of the action. If the owner is disposed, the hotkey will be removed automatically.</param>
    /// <returns>This context.</returns>
    private HotKeysContext AddInternal(ModKey modifiers, Key key, Func<HotKeyEntryByKey, ValueTask> action, string description, string excludeSelector, IHandleEvent? ownerOfAction)
    {
        lock (this.Keys) this.Keys.Add(this.Register(new HotKeyEntryByKey(this._Logger, modifiers, key, excludeSelector, description, action, ownerOfAction)));
        return this;
    }

    // ===============================================================================================

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Action action, string description = "", string excludeSelector = "input, textarea")
    => this.AddInternal(ModCode.None, code, arg => { action(); return ValueTask.CompletedTask; }, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Action<HotKeyEntryByCode> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(ModCode.None, code, arg => { action(arg); return ValueTask.CompletedTask; }, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<ValueTask> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(ModCode.None, code, _ => action(), description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter the key without any modifiers on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(Code code, Func<HotKeyEntryByCode, ValueTask> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(ModCode.None, code, action, description, excludeSelector, action.Target as IHandleEvent);

    // -----------------------------------------------------------------------------------------------

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Action action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(modifiers, code, arg => { action(); return ValueTask.CompletedTask; }, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Action<HotKeyEntryByCode> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(modifiers, code, arg => { action(arg); return ValueTask.CompletedTask; }, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<ValueTask> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(modifiers, code, _ => action(), description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Add(ModCode modifiers, Code code, Func<HotKeyEntryByCode, ValueTask> action, string description = "", string excludeSelector = "input, textarea")
        => this.AddInternal(modifiers, code, action, description, excludeSelector, action.Target as IHandleEvent);

    /// <summary>
    /// Add a new hotkey entry to this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey by code.</param>
    /// <param name="action">The callback action that will be invoked when user enter modifiers + key combination on the browser.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <param name="ownerOfAction">The instance of a Razor component that is an owner of the callback action method.</param>
    /// <returns>This context.</returns>
    private HotKeysContext AddInternal(ModCode modifiers, Code code, Func<HotKeyEntryByCode, ValueTask> action, string description, string excludeSelector, IHandleEvent? ownerOfAction)
    {
        lock (this.Keys) this.Keys.Add(this.Register(new HotKeyEntryByCode(this._Logger, modifiers, code, excludeSelector, description, action, ownerOfAction)));
        return this;
    }

    // ===============================================================================================


    private HotKeyEntry Register(HotKeyEntry hotKeyEntry)
    {
        this._AttachTask.ContinueWith(t =>
        {
            if (t.IsCompleted && !t.IsFaulted)
            {
                return t.Result.InvokeAsync<int>(
                    "Toolbelt.Blazor.HotKeys2.register",
                    hotKeyEntry._ObjectRef, hotKeyEntry.Mode, hotKeyEntry._Modifiers, hotKeyEntry._KeyEntry, hotKeyEntry.ExcludeSelector).AsTask();
            }
            else
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TrySetException(t.Exception?.InnerExceptions ?? new[] { new Exception() }.AsEnumerable());
                return tcs.Task;
            }
        })
        .Unwrap()
        .ContinueWith(t =>
        {
            if (!t.IsCanceled && !t.IsFaulted) { hotKeyEntry.Id = t.Result; }
        });
        return hotKeyEntry;
    }

    private void Unregister(HotKeyEntry hotKeyEntry)
    {
        if (hotKeyEntry.Id == -1) return;

        this._AttachTask.ContinueWith(t =>
        {
            if (t.IsCompleted && !t.IsFaulted)
            {
                return t.Result.InvokeVoidAsync("Toolbelt.Blazor.HotKeys2.unregister", hotKeyEntry.Id).AsTask();
            }
            else
            {
                var tcs = new TaskCompletionSource<int>();
                tcs.TrySetException(t.Exception?.InnerExceptions ?? new[] { new Exception() }.AsEnumerable());
                return tcs.Task as Task;
            }
        })
        .ContinueWith(t =>
        {
            hotKeyEntry.Dispose();
        });
    }

    /// <summary>
    /// Remove one or more hotkey entries from this context.
    /// </summary>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="exclude">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(Key key, string description = "", string exclude = "input, textarea") =>
        this.Remove(ModKey.None, key, description, exclude);

    /// <summary>
    /// Remove one or more hotkey entries from this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="key">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(ModKey modifiers, Key key, string description = "", string excludeSelector = "input, textarea")
    {
        var keyEntry = key.ToString();
        return this.Remove(keys => keys
            .OfType<HotKeyEntryByKey>()
            .Where(
                k => k.Modifiers == modifiers &&
                k.Key.ToString() == keyEntry &&
                k.Description == description &&
                k.ExcludeSelector == excludeSelector));
    }

    /// <summary>
    /// Remove one or more hotkey entries from this context.
    /// </summary>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(Code code, string description = "", string excludeSelector = "input, textarea")
        => this.Remove(ModCode.None, code, description, excludeSelector);

    /// <summary>
    /// Remove one or more hotkey entries from this context.
    /// </summary>
    /// <param name="modifiers">The combination of modifier keys flags.</param>
    /// <param name="code">The identifier of hotkey.</param>
    /// <param name="description">The description of the meaning of this hot key entry.</param>
    /// <param name="excludeSelector">The CSS selector for HTML elements that will not allow hotkey to work.</param>
    /// <returns>This context.</returns>
    public HotKeysContext Remove(ModCode modifiers, Code code, string description = "", string excludeSelector = "input, textarea")
    {
        var keyEntry = code.ToString();
        return this.Remove(keys => keys
            .OfType<HotKeyEntryByCode>()
            .Where(
                k => k.Modifiers == modifiers &&
                k.Code.ToString() == keyEntry &&
                k.Description == description &&
                k.ExcludeSelector == excludeSelector));
    }

    private HotKeysContext Remove(Func<IEnumerable<HotKeyEntry>, IEnumerable<HotKeyEntry>> filter)
    {
        var entries = filter.Invoke(this.Keys).ToArray();
        foreach (var entry in entries)
        {
            this.Unregister(entry);
            lock (this.Keys) this.Keys.Remove(entry);
        }
        return this;
    }

    /// <summary>
    /// Deactivate the hot key entry contained in this context.
    /// </summary>
    public void Dispose()
    {
        foreach (var entry in this.Keys)
        {
            this.Unregister(entry);
        }
        this.Keys.Clear();
    }
}