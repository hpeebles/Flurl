using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Flurl.Util
{
	internal static class ReflectionUtils
	{
		private static readonly ConcurrentDictionary<Type, (string, Func<object, object>)[]>
			PropertyGetterDelegatesByType =
				new ConcurrentDictionary<Type, (string, Func<object, object>)[]>();

		public static (string PropertyName, Func<object, object> GetterDelegate)[]
			GetAllPropertyGetterDelegates(Type type) {
			return PropertyGetterDelegatesByType.GetOrAdd(
				type,
				t => BuildPropertyGetterDelegates(t).ToArray());
		}

#if NETSTANDARD1_0
		private static IEnumerable<(string, Func<object, object>)> BuildPropertyGetterDelegates(Type type) {
			foreach (var property in type.GetRuntimeProperties()) {
				var getter = property.GetMethod;
				if (getter?.IsPublic != true)
					continue;

				var getterDelegate = BuildPropertyGetterDelegate(getter);

				yield return (property.Name, getterDelegate);
			}
		}
#else
		private static IEnumerable<(string, Func<object, object>)> BuildPropertyGetterDelegates(Type type) {
			foreach (var property in type.GetProperties()) {
				var getterDelegate = BuildPropertyGetterDelegate(property);

				if (getterDelegate != null)
					yield return (property.Name, getterDelegate);
			}
		}
#endif

		private static readonly ConcurrentDictionary<Type, Func<object, object>> KeyGetterDelegatesByType =
			new ConcurrentDictionary<Type, Func<object, object>>();

		private static readonly ConcurrentDictionary<Type, Func<object, object>> ValueGetterDelegatesByType =
			new ConcurrentDictionary<Type, Func<object, object>>();

		public static bool TryGetKeyAndValueGetterDelegates(
			Type type,
			out Func<object, object> keyGetterDelegate,
			out Func<object, object> valueGetterDelegate) {
			keyGetterDelegate = KeyGetterDelegatesByType.GetOrAdd(type, BuildKeyGetterDelegate);
			valueGetterDelegate = ValueGetterDelegatesByType.GetOrAdd(type, BuildValueGetterDelegate);
			return keyGetterDelegate != null && valueGetterDelegate != null;
		}

#if NETSTANDARD1_0
		private static Func<object, object> BuildKeyGetterDelegate(Type type) {
			var property =
 type.GetRuntimeProperty("Key") ?? type.GetRuntimeProperty("key") ?? type.GetRuntimeProperty("Name") ?? type.GetRuntimeProperty("name");

			var getter = property?.GetMethod;

			return getter?.IsPublic == true ? BuildPropertyGetterDelegate(getter) : null;
		}

		private static Func<object, object> BuildValueGetterDelegate(Type type) {
			var property = type.GetRuntimeProperty("Value") ?? type.GetRuntimeProperty("value");

			var getter = property?.GetMethod;

			return getter?.IsPublic == true ? BuildPropertyGetterDelegate(getter) : null;
		}

#else
		private static Func<object, object> BuildKeyGetterDelegate(Type type) {
			var property = type.GetProperty("Key") ?? type.GetProperty("key") ?? type.GetProperty("Name") ?? type.GetProperty("name");

			return BuildPropertyGetterDelegate(property);
		}

		private static Func<object, object> BuildValueGetterDelegate(Type type) {
			var property = type.GetProperty("Value") ?? type.GetProperty("value");

			return BuildPropertyGetterDelegate(property);
		}
#endif

		private static Func<object, object> BuildPropertyGetterDelegate(PropertyInfo property) {
			var getter = property?.GetGetMethod(false);

			if (getter == null)
				return null;

			if (property.DeclaringType.IsValueType) {
				return property.GetValue;
			}

			var buildDelegateMethod = typeof(ReflectionUtils)
				.GetMethod(nameof(BuildPropertyGetterDelegateInner), BindingFlags.NonPublic | BindingFlags.Static)
				?.MakeGenericMethod(property.DeclaringType, property.PropertyType);

			return (Func<object, object>)buildDelegateMethod.Invoke(null, new[] {getter});
		}

		private static Func<object, object> BuildPropertyGetterDelegateInner<T, TProp>(MethodInfo propertyGetter) {
			var func = (Func<T, TProp>)propertyGetter.CreateDelegate(typeof(Func<T, TProp>));

			return x => func((T)x);
		}
	}
}