// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using VarDump.Visitor.Descriptors;

namespace VarDump.CodeDom.Common;

internal class CodeCollectionTypeReference : CodeTypeReference
{
    public CodeCollectionTypeReference(Type type, CodeTypeReferenceOptions codeTypeReferenceOption) : base(type, codeTypeReferenceOption)
    {
    }

    public CodeCollectionTypeReference(ITypeDescriptor typeDescriptor, CodeTypeReferenceOptions codeTypeReferenceOption) : base(typeDescriptor, codeTypeReferenceOption)
    {
    }

}