// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Specialized;

namespace VarDump.CodeDom.Common;

internal class CodeObject
{
    private IDictionary _userData;

    public CodeObject() { }

    public IDictionary UserData => _userData ??= new ListDictionary();
}