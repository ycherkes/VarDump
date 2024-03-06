﻿using System;
using System.Linq;

namespace VarDump.Visitor.Descriptors.Implementation;

internal sealed class ConcatenatedObjectDescriptor(IObjectDescriptor first, IObjectDescriptor second) : IObjectDescriptor
{
    public IObjectDescription GetObjectDescription(object @object, Type objectType)
    {
        var firstInfo = first.GetObjectDescription(@object, objectType);
        var secondInfo = second.GetObjectDescription(@object, objectType);

        return new ObjectDescription
        {
            Type = secondInfo.Type ?? firstInfo.Type,
            Members = firstInfo.Members.Concat(secondInfo.Members),
            ConstructorParameters = firstInfo.ConstructorParameters.Concat(secondInfo.ConstructorParameters)
        };
    }
}