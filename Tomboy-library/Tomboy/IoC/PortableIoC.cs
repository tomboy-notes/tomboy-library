using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

/*******************************************************************************

PortableIoC

Copyright (c) 2012 Jeremy Likness

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*********************************************************************************/

/* BEGIN FILE: BaseIoCRegistration.cs */
namespace PortableIoC
{
    /// <summary>
    /// Base class to track a registration instance
    /// </summary>
    internal abstract class BaseIoCRegistration : IIoCRegistration
    {
        /// <summary>
        /// The shared instance
        /// </summary>
        private object _instance;

        /// <summary>
        /// Lock
        /// </summary>
        private readonly object _mutex = new object();

        /// <summary>
        /// Delegate that describes how to create a new instance
        /// </summary>
        private readonly Func<IPortableIoC, object> _creation;

        /// <summary>
        /// Constructor takes in the instructions to create the instance
        /// </summary>
        /// <param name="create">A delegate for instance creation</param>
        internal BaseIoCRegistration(Func<IPortableIoC, object> create)
        {
            _creation = create;
            HasInstance = false;
        }

        /// <summary>
        /// Destroy the shared instance
        /// </summary>
        public bool DestroyInstance()
        {
            if (!HasInstance)
            {
                return false;
            }
            
            Monitor.Enter(_mutex);

            try
            {
                if (!HasInstance)
                {
                    return false;
                }

                _instance = null;
                HasInstance = false;
                return true;
            }
            finally
            {
                Monitor.Exit(_mutex);
            }
        }

        /// <summary>
        /// The type this registration implements
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Whether a shared instance has been created or not
        /// </summary>
        public bool HasInstance { get; private set; }

        /// <summary>
        /// Get the instance
        /// </summary>
        /// <param name="ioc">The ioc container</param>
        /// <param name="createNew">True for a non-shared instance</param>
        /// <returns>The resolved instance</returns>
        public object GetInstance(IPortableIoC ioc, bool createNew)
        {
            if (createNew)
            {
                return _creation(ioc);
            }

            if (HasInstance)
            {
                return _instance;
            }

            Monitor.Enter(_mutex);

            try
            {
                if (HasInstance)
                {
                    return _instance;
                }

                _instance = _creation(ioc);
                HasInstance = true;

                return _instance;
            }
            finally
            {
                Monitor.Exit(_mutex);    
            }            
        }
    }
}
/* END FILE: BaseIoCRegistration.cs */

/* BEGIN FILE: IIocRegistration.cs */
namespace PortableIoC
{
	internal interface IIoCRegistration
	{
		/// <summary>
		/// True if a shared instance has been created
		/// </summary>
		bool HasInstance { get; }

		/// <summary>
		/// Resolves the instance of a type
		/// </summary>
		/// <param name="ioc">The ioc container for chained dependencies</param>
		/// <param name="createNew">True for a non-shared instance</param>
		/// <returns>The resolved instance</returns>
		object GetInstance(IPortableIoC ioc, bool createNew);

		/// <summary>
		/// Destroy the shared instance 
		/// </summary>
		bool DestroyInstance();

		/// <summary>
		/// The type this registration represents
		/// </summary>
		string Type { get; }
	}

	// ReSharper disable TypeParameterCanBeVariant
	internal interface IIoCRegistration<T> : IIoCRegistration
		// ReSharper restore TypeParameterCanBeVariant
	{
		T GetTypedInstance(IPortableIoC ioc, bool createNew);
	}
}
/* END FILE: IIocRegistration.cs */

/* BEGIN FILE: IocRegistration.cs */
namespace PortableIoC
{
	/// <summary>
	/// Represents the registration of a resolution
	/// </summary>
	/// <typeparam name="T">The type to be resolved</typeparam>
	internal class IocRegistration<T> : BaseIoCRegistration, IIoCRegistration<T>
	{
		private readonly int _hashCode;
		private readonly string _typeName;

		/// <summary>
		/// Constructor takes typed and passes through to base
		/// </summary>
		/// <param name="create">Create the </param>
		public IocRegistration(Func<IPortableIoC, T> create) : base(ioc => create(ioc))
		{
			var type = typeof (T).FullName;

			if (string.IsNullOrEmpty(type))
			{
				throw new InvalidOperationException("Cannot create a non-typed registration.");
			}

			_hashCode = type.GetHashCode();
			_typeName = type;
		}

		/// <summary>
		/// Grabs the typed implementation of the base instance
		/// </summary>
		/// <param name="ioc">The ioc container</param>
		/// <param name="createNew">True for a non-shared instance</param>
		/// <returns>The resolved instance</returns>
		public T GetTypedInstance(IPortableIoC ioc, bool createNew)
		{
			return (T) GetInstance(ioc, createNew);
		}

		/// <summary>
		/// The type this registration represents
		/// </summary>
		public override string Type
		{
			get { return _typeName; }
		}

		/// <summary>
		/// Equals will sort/filter based on type
		/// </summary>
		/// <param name="obj">The other object</param>
		/// <returns>True if the same type</returns>
		public override bool Equals(object obj)
		{
			return obj is IocRegistration<T> &&
				((IocRegistration<T>) obj).Type.Equals(Type);
		}

		/// <summary>
		/// Get the hash code of the type
		/// </summary>
		/// <returns>The type</returns>
		public override int GetHashCode()
		{
			return _hashCode;
		}

	}
}
/* END FILE: IocRegistration.cs */

/* BEGIN FILE: IPortableIoC.cs */
namespace PortableIoC
{
	/// <summary>
	/// Lightweight, portable Inversion of Control container that will work on
	/// .NET Framework 4.x, Silverlight 4.0 and 5.0, WPF, Windows Phone 7.x and later,
	/// and Windows Store (Windows 8) applications
	/// </summary>
	public interface IPortableIoC
	{
		/// <summary>
		/// Register a type to be created with an alias to create that includes an
		/// instance of the IoC container
		/// </summary>
		/// <typeparam name="T">The type that is going to be implemented</typeparam>
		/// <param name="label">A unique label that allows multiple implementations of the same type</param>
		/// <param name="creation">An expression to create a new instance of the type</param>
		void Register<T>(Func<IPortableIoC, T> creation, string label = "");

		/// <summary>
		/// Resolve the implementation of a type (interface, abstract class, etc.)
		/// </summary>
		/// <typeparam name="T">The type to resolve the implementation for</typeparam>
		/// <param name="label">A unique label that allows multiple implementations of the same type</param>
		/// <returns>The implementation (defaults to a shared implementation)</returns>
		T Resolve<T>(string label = "");

		/// <summary>
		/// Overload to resolve the implementation of a type with the option to create
		/// a non-shared instance
		/// </summary>
		/// <typeparam name="T">The type to resolve</typeparam>
		/// <param name="label">A unique label that allows multiple implementations of the same type</param>
		/// <param name="createNew">True for a non-shared instance</param>
		/// <returns>The implementation of the type</returns>
		T Resolve<T>(bool createNew, string label = "");

		/// <summary>
		/// Test to see whether it is possible to resolve a type
		/// </summary>
		/// <typeparam name="T">The type to test for</typeparam>
		/// <param name="label">A unique label that allows for multiple implementations of the same type</param>
		/// <returns>True if an implementation exists for the type</returns>
		bool CanResolve<T>(string label = "");

		/// <summary>
		/// Attempt to resolve an instance
		/// </summary>
		/// <typeparam name="T">The type to resolve</typeparam>
		/// <param name="instance">The instance, if it is possible to create one</param>
		/// <param name="label">A unique label that allows for multiple implementations of the same type</param>
		/// <returns>True if the resolution succeeded</returns>
		bool TryResolve<T>(out T instance, string label = "");

		/// <summary>
		/// Attempt to resolve an instance
		/// </summary>
		/// <typeparam name="T">The type to resolve</typeparam>
		/// <param name="instance">The instance, if it is possible to create one</param>
		/// <param name="createNew">Set to true to create a non-shared instance</param>
		/// <param name="label">A unique label that allows for multiple implementations of the same type</param>
		/// <returns>True if the resolution succeeded</returns>
		bool TryResolve<T>(out T instance, bool createNew, string label = "");

		/// <summary>
		/// Unregister a definition. 
		/// </summary>
		/// <typeparam name="T">The type to destroy</typeparam>
		/// <param name="label">An optional label to allow multiple implementations of the same type</param>
		/// <returns>True if the definition existed</returns>
		/// <remarks>This will destroy references to any shared instances for the type</remarks>
		bool Unregister<T>(string label = "");

		/// <summary>
		/// Destroy the shared instance of a type 
		/// </summary>
		/// <typeparam name="T">The type to destroy</typeparam>
		/// <param name="label">An optional label to allow multiple implementations of the same type</param>
		/// <returns>True if a shared instance existed</returns>
		/// <remarks>This will allow for a new instance to become shared if the resolve call is made later</remarks>
		bool Destroy<T>(string label = "");
	}
}
/* END FILE: IPortableIoC.cs */

/* BEGIN FILE: PortableIoC.cs */
namespace PortableIoC
{
	public class PortableIoc : IPortableIoC
	{
		private static readonly string DefaultLabel = Guid.NewGuid().ToString();

		private const string InvalidRegistration =
			"A definition for label {0} and type {1} already exists. Unregister the definition first.";

		private const string InvalidResolution =
			"Cannot resolve type {0} for label {1}.";

		private readonly IDictionary<string, IList<IIoCRegistration>> _containers =
			new Dictionary<string, IList<IIoCRegistration>>();

		private readonly object _mutex = new object();

		#region IPortableIoC Members

		/// <summary>
		/// Register a type to be created with an alias to create that includes an
		/// instance of the IoC container
		/// </summary>
		/// <typeparam name="T">The type that is going to be implemented</typeparam>
		/// <param name="label">A unique label that allows multiple implementations of the same type</param>
		/// <param name="creation">An expression to create a new instance of the type</param>
		public void Register<T>(Func<IPortableIoC, T> creation, string label = "")
		{
			label = string.IsNullOrEmpty(label) ? DefaultLabel : label;

			RequireThat.IsNotNull(creation, "creation");

			CheckContainer(label);

			Monitor.Enter(_mutex);

			try
			{
				if (Exists(label, typeof(T)))
				{
					throw new InvalidOperationException(
						string.Format(
						InvalidRegistration,
						label,
						typeof(T).FullName));
				}
				var iocRegistration = new IocRegistration<T>(creation);
				_containers[label].Add(iocRegistration);
			}
			finally
			{
				Monitor.Exit(_mutex);
			}
		}

		/// <summary>
		/// Resolve the implementation of a type (interface, abstract class, etc.)
		/// </summary>
		/// <typeparam name="T">The type to resolve the implementation for</typeparam>
		/// <param name="label">A unique label that allows multiple implementations of the same type</param>
		/// <returns>The implementation (defaults to a shared implementation)</returns>
		public T Resolve<T>(string label = "")
		{
			label = string.IsNullOrEmpty(label) ? DefaultLabel : label;

			return Resolve<T>(false, label);
		}

		/// <summary>
		/// Overload to resolve the implementation of a type with the option to create
		/// a non-shared instance
		/// </summary>
		/// <typeparam name="T">The type to resolve</typeparam>
		/// <param name="label">A unique label that allows multiple implementations of the same type</param>
		/// <param name="createNew">True for a non-shared instance</param>
		/// <returns>The implementation of the type</returns>
		public T Resolve<T>(bool createNew, string label = "")
		{
			label = string.IsNullOrEmpty(label) ? DefaultLabel : label;

			CheckContainer(label);

			if (!Exists(label, typeof(T)))
			{
				throw new InvalidOperationException(
					string.Format(
					InvalidResolution,
					typeof(T).FullName,
					label));
			}

			lock (_mutex)
			{
				var iocRegistration = _containers[label].FirstOrDefault(ioc => ioc.Type == typeof (T).FullName)
					as IocRegistration<T>;

				if (iocRegistration == null)
				{
					throw new InvalidOperationException(
						string.Format(
						InvalidResolution,
						typeof (T).FullName,
						label));
				}

				return iocRegistration.GetTypedInstance(this, createNew);
			}
		}

		/// <summary>
		/// Test to see whether it is possible to resolve a type
		/// </summary>
		/// <typeparam name="T">The type to test for</typeparam>
		/// <param name="label">A unique label that allows for multiple implementations of the same type</param>
		/// <returns>True if an implementation exists for the type</returns>
		public bool CanResolve<T>(string label = "")
		{
			label = string.IsNullOrEmpty(label) ? DefaultLabel : label;

			return Exists(label, typeof(T));
		}

		/// <summary>
		/// Attempt to resolve an instance
		/// </summary>
		/// <typeparam name="T">The type to resolve</typeparam>
		/// <param name="instance">The instance, if it is possible to create one</param>
		/// <param name="label">A unique label that allows for multiple implementations of the same type</param>
		/// <returns>True if the resolution succeeded</returns>
		public bool TryResolve<T>(out T instance, string label = "")
		{
			label = string.IsNullOrEmpty(label) ? DefaultLabel : label;
			T passThroughInstance;
			var result = TryResolve(out passThroughInstance, false, label);
			instance = passThroughInstance;
			return result;
		}

		/// <summary>
		/// Attempt to resolve an instance
		/// </summary>
		/// <typeparam name="T">The type to resolve</typeparam>
		/// <param name="instance">The instance, if it is possible to create one</param>
		/// <param name="createNew">Set to true to create a non-shared instance</param>
		/// <param name="label">A unique label that allows for multiple implementations of the same type</param>
		/// <returns>True if the resolution succeeded</returns>
		public bool TryResolve<T>(out T instance, bool createNew, string label = "")
		{
			label = string.IsNullOrEmpty(label) ? DefaultLabel : label;

			if (!Exists(label, typeof(T)))
			{
				instance = default(T);
				return false;
			}

			try
			{
				instance = Resolve<T>(createNew, label);
				return true;
			}
			catch
			{
				instance = default(T);
				return false;
			}
		}

		/// <summary>
		/// Unregister a definition. 
		/// </summary>
		/// <typeparam name="T">The type to destroy</typeparam>
		/// <param name="label">An optional label to allow multiple implementations of the same type</param>
		/// <returns>True if the definition existed</returns>
		/// <remarks>This will destroy references to any shared instances for the type</remarks>
		public bool Unregister<T>(string label = "")
		{
			label = string.IsNullOrEmpty(label) ? DefaultLabel : label;

			if (!Exists(label, typeof(T)))
			{
				return false;
			}

			Monitor.Enter(_mutex);

			try
			{
				if (!Exists(label, typeof(T)))
				{
					return false;
				}
				var registration = _containers[label].First(c => c.Type == typeof (T).FullName);
				_containers[label].Remove(registration);
				return true;
			}
			finally
			{
				Monitor.Exit(_mutex);
			}
		}

		/// <summary>
		/// Destroy the shared instance of a type 
		/// </summary>
		/// <typeparam name="T">The type to destroy</typeparam>
		/// <param name="label">An optional label to allow multiple implementations of the same type</param>
		/// <returns>True if a shared instance existed</returns>
		/// <remarks>This will allow for a new instance to become shared if the resolve call is made later</remarks>
		public bool Destroy<T>(string label = "")
		{
			label = string.IsNullOrEmpty(label) ? DefaultLabel : label;

			if (!Exists(label, typeof(T)))
			{
				return false;
			}

			Monitor.Enter(_mutex);

			try
			{
				var registration = _containers[label].FirstOrDefault(c => c.Type == typeof(T).FullName);
				return registration != null && registration.DestroyInstance();
			}
			finally
			{
				Monitor.Exit(_mutex);
			}
		}

		#endregion

		/// <summary>
		/// Ensure a container exists for the given label
		/// </summary>
		/// <param name="label">A label to sub-divide containers</param>
		private void CheckContainer(string label)
		{
			RequireThat.IsNotNull(label, "label");

			if (_containers.ContainsKey(label))
			{
				return;
			}

			Monitor.Enter(_mutex);

			try
			{
				if (_containers.ContainsKey(label))
				{
					return;
				}

				_containers.Add(label, new List<IIoCRegistration>());

				// every container can resolve the IOC parent
				var iocRegistration = new IocRegistration<IPortableIoC>(ioc => this);
				_containers[label].Add(iocRegistration);
			}
			finally
			{
				Monitor.Exit(_mutex);
			}
		}

		private bool Exists(string label, Type t)
		{
			RequireThat.IsNotNull(label, "label");
			RequireThat.IsNotNull(t, "t");

			if (!_containers.ContainsKey(label))
			{
				return false;
			}

			var type = t.FullName;

			lock (_mutex)
			{
				return _containers[label].Any(ioc => ioc.Type.Equals(type));
			}
		}
	}
}
/* END FILE: PortableIoC.cs */

/* BEGIN FILE: RequireThat.cs */
namespace PortableIoC
{
	/// <summary>
	/// Helper for parameter validation
	/// </summary>
	public static class RequireThat
	{
		/// <summary>
		/// Require that a value is not null
		/// </summary>
		/// <typeparam name="T">The type of the value</typeparam>
		/// <param name="value">The value</param>
		/// <param name="parameterName">The name of the parameter that passed the value</param>
		public static void IsNotNull<T>(T value, string parameterName) where T: class
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
		}
	}
} 
/* END FILE: RequireThat.cs */