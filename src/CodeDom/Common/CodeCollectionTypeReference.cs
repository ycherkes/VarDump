// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace VarDumpExtended.CodeDom.Common;

public class CodeCollectionTypeReference : CodeTypeReference
{
    public CodeCollectionTypeReference(Type type, CodeTypeReferenceOptions codeTypeReferenceOption) : base(type, codeTypeReferenceOption)
    {
    }

}