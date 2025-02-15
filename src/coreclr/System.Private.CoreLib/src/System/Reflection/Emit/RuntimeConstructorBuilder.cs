// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace System.Reflection.Emit
{
    internal sealed class RuntimeConstructorBuilder : ConstructorBuilder
    {
        private readonly RuntimeMethodBuilder m_methodBuilder;
        internal bool m_isDefaultConstructor;

        #region Constructor

        internal RuntimeConstructorBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention,
            Type[]? parameterTypes, Type[][]? requiredCustomModifiers, Type[][]? optionalCustomModifiers, RuntimeModuleBuilder mod, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] RuntimeTypeBuilder type)
        {
            m_methodBuilder = new RuntimeMethodBuilder(name, attributes, callingConvention, null, null, null,
                parameterTypes, requiredCustomModifiers, optionalCustomModifiers, mod, type);

            type.m_listMethods!.Add(m_methodBuilder);

            m_methodBuilder.GetMethodSignature().InternalGetSignature(out _);

            _ = m_methodBuilder.MetadataToken; // Doubles as "CreateMethod" for MethodBuilder -- analogous to CreateType()
        }

        internal RuntimeConstructorBuilder(string name, MethodAttributes attributes, CallingConventions callingConvention,
            Type[]? parameterTypes, RuntimeModuleBuilder mod, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] RuntimeTypeBuilder type) :
            this(name, attributes, callingConvention, parameterTypes, null, null, mod, type)
        {
        }

        #endregion

        #region Internal
        internal override Type[] GetParameterTypes()
        {
            return m_methodBuilder.GetParameterTypes();
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private RuntimeTypeBuilder GetTypeBuilder()
        {
            return m_methodBuilder.GetTypeBuilder();
        }
        internal SignatureHelper GetMethodSignature()
        {
            return m_methodBuilder.GetMethodSignature();
        }
        #endregion

        #region Object Overrides
        public override string ToString()
        {
            return m_methodBuilder.ToString();
        }

        #endregion

        #region MemberInfo Overrides
        public override int MetadataToken => m_methodBuilder.MetadataToken;

        public override Module Module => m_methodBuilder.Module;

        public override Type? ReflectedType => m_methodBuilder.ReflectedType;

        public override Type? DeclaringType => m_methodBuilder.DeclaringType;

        public override string Name => m_methodBuilder.Name;

        #endregion

        #region MethodBase Overrides
        public override object Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture)
        {
            throw new NotSupportedException(SR.NotSupported_DynamicModule);
        }

        public override ParameterInfo[] GetParameters()
        {
            ConstructorInfo rci = GetTypeBuilder().GetConstructor(m_methodBuilder.m_parameterTypes!)!;
            return rci.GetParameters();
        }

        public override MethodAttributes Attributes => m_methodBuilder.Attributes;

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return m_methodBuilder.GetMethodImplementationFlags();
        }

        public override RuntimeMethodHandle MethodHandle => m_methodBuilder.MethodHandle;

        #endregion

        #region ConstructorInfo Overrides
        public override object Invoke(BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture)
        {
            throw new NotSupportedException(SR.NotSupported_DynamicModule);
        }

        #endregion

        #region ICustomAttributeProvider Implementation
        public override object[] GetCustomAttributes(bool inherit)
        {
            return m_methodBuilder.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return m_methodBuilder.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return m_methodBuilder.IsDefined(attributeType, inherit);
        }

        #endregion

        #region Public Members
        protected override ParameterBuilder DefineParameterCore(int iSequence, ParameterAttributes attributes, string? strParamName)
        {
            // Theoretically we shouldn't allow iSequence to be 0 because in reflection ctors don't have
            // return parameters. But we'll allow it for backward compatibility with V2. The attributes
            // defined on the return parameters won't be very useful but won't do much harm either.

            // MD will assert if we try to set the reserved bits explicitly
            attributes &= ~ParameterAttributes.ReservedMask;
            return m_methodBuilder.DefineParameter(iSequence, attributes, strParamName);
        }

        protected override ILGenerator GetILGeneratorCore(int streamSize)
        {
            if (m_isDefaultConstructor)
                throw new InvalidOperationException(SR.InvalidOperation_DefaultConstructorILGen);

            return m_methodBuilder.GetILGenerator(streamSize);
        }

        public override CallingConventions CallingConvention
        {
            get
            {
                if (DeclaringType!.IsGenericType)
                    return CallingConventions.HasThis;

                return CallingConventions.Standard;
            }
        }

        internal override Type GetReturnType()
        {
            return m_methodBuilder.ReturnType;
        }

        protected override void SetCustomAttributeCore(ConstructorInfo con, byte[] binaryAttribute)
        {
            m_methodBuilder.SetCustomAttribute(con, binaryAttribute);
        }

        protected override void SetCustomAttributeCore(CustomAttributeBuilder customBuilder)
        {
            m_methodBuilder.SetCustomAttribute(customBuilder);
        }

        protected override void SetImplementationFlagsCore(MethodImplAttributes attributes)
        {
            m_methodBuilder.SetImplementationFlags(attributes);
        }

        protected override bool InitLocalsCore
        {
            get => m_methodBuilder.InitLocals;
            set => m_methodBuilder.InitLocals = value;
        }

        #endregion
    }
}
