using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public abstract class SerializableCallback<TReturn> : SerializableCallbackBase<TReturn> {
	public TReturn Invoke() {
		if (func == null) Cache();
		if (_dynamic) {
			InvokableCallback<TReturn> call = func as InvokableCallback<TReturn>;
			return call.Invoke();
		} else {
			return func.Invoke(Args);
		}
	}

	protected override void Cache() {
		if (_target == null || string.IsNullOrEmpty(_methodName)) {
			func = new InvokableCallback<TReturn>(null, null);
		} else {
			if (_dynamic) {
				func = new InvokableCallback<TReturn>(target, methodName);
			} else {
				func = GetPersistentMethod();
			}
		}
	}
}

public abstract class SerializableCallback<T0, TReturn> : SerializableCallbackBase<TReturn> {
	public TReturn Invoke(T0 arg0) {
		if (func == null) Cache();
		if (_dynamic) {
			InvokableCallback<T0, TReturn> call = func as InvokableCallback<T0, TReturn>;
			return call.Invoke(arg0);
		} else {
			return func.Invoke(Args);
		}
	}

	protected override void Cache() {
		if (_target == null || string.IsNullOrEmpty(_methodName)) {
			func = new InvokableCallback<T0, TReturn>(null, null);
		} else {
			if (_dynamic) {
				func = new InvokableCallback<T0, TReturn>(target, methodName);
			} else {
				func = GetPersistentMethod();
			}
		}
	}
}

[Serializable]
public class Condition : SerializableCallback<bool> { }