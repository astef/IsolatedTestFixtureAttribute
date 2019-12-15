﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;

namespace AlcAttr
{
    /// <summary>
    /// Marks the class as a TestFixture.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class IsolatedTestFixtureAttribute : NUnitAttribute, IFixtureBuilder2, ITestFixtureData
    {
        private readonly NUnitTestFixtureBuilder _builder = new NUnitTestFixtureBuilder();

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public IsolatedTestFixtureAttribute() : this(new object[0]) { }

        /// <summary>
        /// Construct with a object[] representing a set of arguments.
        /// The arguments may later be separated into type arguments and constructor arguments.
        /// </summary>
        /// <param name="arguments"></param>
        public IsolatedTestFixtureAttribute(params object[] arguments)
        {
            RunState = RunState.Runnable;
            Arguments = arguments ?? new object[] { null };
            TypeArgs = new Type[0];
            Properties = new PropertyBag();
        }

        #endregion

        #region ITestData Members

        /// <summary>
        /// Gets or sets the name of the test.
        /// </summary>
        /// <value>The name of the test.</value>
        public string TestName { get; set; }

        /// <summary>
        /// Gets or sets the RunState of this test fixture.
        /// </summary>
        public RunState RunState { get; private set; }

        /// <summary>
        /// The arguments originally provided to the attribute
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Properties pertaining to this fixture
        /// </summary>
        public IPropertyBag Properties { get; }

        #endregion

        #region ITestFixtureData Members

        /// <summary>
        /// Get or set the type arguments. If not set
        /// explicitly, any leading arguments that are
        /// Types are taken as type arguments.
        /// </summary>
        public Type[] TypeArgs { get; set; }

        #endregion

        #region Other Properties

        /// <summary>
        /// Descriptive text for this fixture
        /// </summary>
        public string Description
        {
            get { return Properties.Get(PropertyNames.Description) as string; }
            set { Properties.Set(PropertyNames.Description, value); }
        }

        /// <summary>
        /// The author of this fixture
        /// </summary>
        public string Author
        {
            get { return Properties.Get(PropertyNames.Author) as string; }
            set { Properties.Set(PropertyNames.Author, value); }
        }

        /// <summary>
        /// The type that this fixture is testing
        /// </summary>
        public Type TestOf
        {
            get { return _testOf; }
            set
            {
                _testOf = value;
                Properties.Set(PropertyNames.TestOf, value.FullName);
            }
        }
        private Type _testOf;

        /// <summary>
        /// Gets or sets the ignore reason. May set RunState as a side effect.
        /// </summary>
        /// <value>The ignore reason.</value>
        public string Ignore
        {
            get { return IgnoreReason; }
            set { IgnoreReason = value; }
        }

        /// <summary>
        /// Gets or sets the reason for not running the fixture.
        /// </summary>
        /// <value>The reason.</value>
        public string Reason
        {
            get { return this.Properties.Get(PropertyNames.SkipReason) as string; }
            set { this.Properties.Set(PropertyNames.SkipReason, value); }
        }

        /// <summary>
        /// Gets or sets the ignore reason. When set to a non-null
        /// non-empty value, the test is marked as ignored.
        /// </summary>
        /// <value>The ignore reason.</value>
        public string IgnoreReason
        {
            get { return Reason; }
            set
            {
                RunState = RunState.Ignored;
                Reason = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="NUnit.Framework.TestFixtureAttribute"/> is explicit.
        /// </summary>
        /// <value>
        /// <c>true</c> if explicit; otherwise, <c>false</c>.
        /// </value>
        public bool Explicit
        {
            get { return RunState == RunState.Explicit; }
            set { RunState = value ? RunState.Explicit : RunState.Runnable; }
        }

        /// <summary>
        /// Gets and sets the category for this fixture.
        /// May be a comma-separated list of categories.
        /// </summary>
        public string Category
        {
            get
            {
                //return Properties.Get(PropertyNames.Category) as string;
                IList catList = Properties[PropertyNames.Category];
                if (catList == null)
                    return null;

                switch (catList.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        return catList[0] as string;
                    default:
                        var cats = new string[catList.Count];
                        int index = 0;
                        foreach (string cat in catList)
                            cats[index++] = cat;

                        return string.Join(",", cats);
                }
            }
            set
            {
                foreach (string cat in value.Split(new char[] { ',' }))
                    Properties.Add(PropertyNames.Category, cat);
            }
        }

        #endregion

        #region IFixtureBuilder Members

        /// <summary>
        /// Builds a single test fixture from the specified type.
        /// </summary>
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo)
        {
            ITypeInfo isolatedType = GetIsolated(typeInfo);
            TestSuite isolated = _builder.BuildFrom(isolatedType, EmptyFilter.Instance, this);
            yield return isolated;
        }

        #endregion

        #region IFixtureBuilder2 Members

        /// <summary>
        /// Builds a single test fixture from the specified type.
        /// </summary>
        /// <param name="typeInfo">The type info of the fixture to be used.</param>
        /// <param name="filter">Filter used to select methods as tests.</param>
        public IEnumerable<TestSuite> BuildFrom(ITypeInfo typeInfo, IPreFilter filter)
        {
            ITypeInfo isolatedType = GetIsolated(typeInfo);
            TestSuite isolated = _builder.BuildFrom(isolatedType, filter, this);
            yield return isolated;
        }

        #endregion

        private static ITypeInfo GetIsolated(ITypeInfo typeInfo)
        {
            var asmLocation = typeInfo.Type.Assembly.Location;

            var alc = new IsolatedTestLoadContext(asmLocation);

            Assembly asm = alc.LoadFromAssemblyPath(asmLocation);

            Type? isolatedType = asm.GetType(typeInfo.Type.FullName, true, false)!;

            return new TypeWrapper(isolatedType);
        }

        private class EmptyFilter : IPreFilter
        {
            internal static readonly EmptyFilter Instance = new EmptyFilter();

            public bool IsMatch(Type type) => true;

            public bool IsMatch(Type type, MethodInfo method) => true;
        }

        private sealed class IsolatedTestLoadContext : AssemblyLoadContext
        {
            private readonly AssemblyDependencyResolver _resolver;

            internal IsolatedTestLoadContext(string assemblyLocation) :
                base($"{nameof(IsolatedTestLoadContext)}.{Guid.NewGuid():N}", true)
            {
                _resolver = new AssemblyDependencyResolver(assemblyLocation);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                if (assemblyName.Name == "nunit.framework")
                {
                    // avoid "duplicating" NUnit classes 
                    return null;
                }
                var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
                return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
            }
        }
    }
}
