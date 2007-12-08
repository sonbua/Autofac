﻿// This software is part of the Autofac IoC container
// Copyright (c) 2007 Nicholas Blumhardt
// nicholas.blumhardt@gmail.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using Autofac.Component.Activation;
using System;
using System.Globalization;
using System.Text;
using Autofac.Component;
using System.Collections.Generic;

namespace Autofac.Builder
{
    /// <summary>
    /// Register a component to be created through reflection.
    /// </summary>
	class ReflectiveRegistrar : ConcreteRegistrar<IReflectiveRegistrar>, IReflectiveRegistrar
	{
        Type _implementor;
        IConstructorSelector _ctorSelector = new MostParametersConstructorSelector();
        IDictionary<string, object> _additionalCtorArgs = new Dictionary<string, object>();
        IDictionary<string, object> _explicitProperties = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionRegistrar"/> class.
        /// </summary>
		public ReflectiveRegistrar(Type implementor)
			: base(implementor)
		{
            Enforce.ArgumentNotNull(implementor, "implementor");
            _implementor = implementor;
		}

        #region IReflectiveRegistrar Members

        /// <summary>
        /// Enforce that the specific constructor with the provided signature is used.
        /// </summary>
        /// <param name="ctorSignature">The types that designate the constructor to use.</param>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        public IReflectiveRegistrar UsingConstructor(params Type[] ctorSignature)
        {
            Enforce.ArgumentNotNull(ctorSignature, "ctorSignature");
            if (null == _implementor.GetConstructor(ctorSignature))
            {
                var sig = new StringBuilder();
                var first = true;
                foreach (var t in ctorSignature)
                {
                    if (first)
                        first = false;
                    else
                        sig.Append(", ");
                    sig.Append(t.FullName);
                }

                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    ReflectiveRegistrarResources.TypeDoesNotProvideCtor,
                    _implementor, sig));
            }
            _ctorSelector = new SpecificConstructorSelector(ctorSignature);
            return this;
        }

        /// <summary>
        /// Associates constructor parameters with default values.
        /// </summary>
        /// <param name="additionalCtorArgs">The named values to apply to the constructor.
        /// These may be overriden by supplying any/all values to the IContext.Resolve() method.</param>
        /// <returns>
        /// A registrar allowing registration to continue.
        /// </returns>
        public IReflectiveRegistrar WithArguments(params Parameter[] additionalCtorArgs)
        {
            Enforce.ArgumentNotNull(additionalCtorArgs, "additionalCtorArgs");
            
            var newArgs = new Dictionary<string, object>();
            foreach (var nv in additionalCtorArgs)
                newArgs.Add(nv.Name, nv.Value);

            _additionalCtorArgs = newArgs;

            return this;
        }

        /// <summary>
        /// Provide explicit property values to be set on the new object.
        /// </summary>
        /// <param name="explicitProperties"></param>
        /// <returns></returns>
        /// <remarks>Note, supplying a null value will not prevent property injection if
        /// property injection is done through an OnActivating handler.</remarks>
        public IReflectiveRegistrar WithProperties(params Parameter[] explicitProperties)
        {
            Enforce.ArgumentNotNull(explicitProperties, "explicitProperties");

            var newProps = new Dictionary<string, object>();
            foreach (var nv in explicitProperties)
                newProps.Add(nv.Name, nv.Value);

            _explicitProperties = newProps;

            return this;
        }

        #endregion

        /// <summary>
        /// Creates the activator for the registration.
        /// </summary>
        /// <returns>An activator.</returns>
        protected override IActivator CreateActivator()
        {
            return new ReflectionActivator(
                _implementor,
                _additionalCtorArgs,
                _explicitProperties,
                _ctorSelector);
        }

        /// <summary>
        /// Returns this instance, correctly-typed.
        /// </summary>
        /// <value></value>
        protected override IReflectiveRegistrar Syntax
        {
            get { return this; }
        }
    }
}