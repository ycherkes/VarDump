using System;

namespace VarDump.Visitor;

/// <summary>
/// Allows <see cref="ObjectDescriptionWriter"/> to request a collection-initializer body without
/// knowing how <see cref="ObjectVisitor"/> selects the visitor for the runtime value. This keeps
/// the special rendering mode explicit instead of storing transient state in <see cref="VisitContext"/>.
/// </summary>
internal interface ICollectionInitializerBodyDispatcher
{
    bool TryWriteCollectionInitializerBody(object value, VisitContext context);
}

/// <summary>
/// Identifies visitors that can emit only the braced initializer portion of a collection value.
/// The dispatcher uses this separate contract because an ordinary visit emits a complete value
/// expression, which is invalid for a get-only property populated through an <c>Add</c> initializer.
/// </summary>
internal interface ICollectionInitializerBodyWriter
{
    void WriteCollectionInitializerBody(object value, Type valueType, VisitContext context);
}
