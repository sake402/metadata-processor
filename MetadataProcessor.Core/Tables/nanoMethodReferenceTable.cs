﻿//
// Copyright (c) .NET Foundation and Contributors
// Original work from Oleg Rakhmatulin.
// See LICENSE file in the project root for full license information.
//

using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace nanoFramework.Tools.MetadataProcessor
{
    /// <summary>
    /// Encapsulates logic for storing methods references list and writing
    /// this collected list into target assembly in .NET nanoFramework format.
    /// </summary>
    public sealed class nanoMethodReferenceTable :
        nanoReferenceTableBase<MethodReference>
    {
        /// <summary>
        /// Helper class for comparing two instances of <see cref="MethodReference"/> objects
        /// using <see cref="MethodReference.FullName"/> property as unique key for comparison.
        /// </summary>
        private sealed class MemberReferenceComparer : IEqualityComparer<MethodReference>
        {
            /// <inheritdoc/>
            public bool Equals(MethodReference lhs, MethodReference rhs)
            {
                return string.Equals(lhs.FullName, rhs.FullName, StringComparison.Ordinal);
            }

            /// <inheritdoc/>
            public int GetHashCode(MethodReference that)
            {
                return that.FullName.GetHashCode();
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="nanoMethodReferenceTable"/> object.
        /// </summary>
        /// <param name="items">List of member references in Mono.Cecil format.</param>
        /// <param name="context">
        /// Assembly tables context - contains all tables used for building target assembly.
        /// </param>
        public nanoMethodReferenceTable(
            IEnumerable<MethodReference> items,
            nanoTablesContext context)
            : base(items, new MemberReferenceComparer(), context)
        {
        }

        /// <summary>
        /// Gets method reference ID if possible (if method is external and stored in this table).
        /// </summary>
        /// <param name="methodReference">Method reference metadata in Mono.Cecil format.</param>
        /// <param name="referenceId">Method reference ID in .NET nanoFramework format.</param>
        /// <returns>Returns <c>true</c> if reference found, otherwise returns <c>false</c>.</returns>
        public bool TryGetMethodReferenceId(
            MethodReference methodReference,
            out ushort referenceId)
        {
            return TryGetIdByValue(methodReference, out referenceId);
        }

        /// <inheritdoc/>
        protected override void WriteSingleItem(
            nanoBinaryWriter writer,
            MethodReference item)
        {
            if (!_context.MinimizeComplete)
            {
                return;
            }

            ushort referenceId;
            _context.TypeReferencesTable.TryGetTypeReferenceId(item.DeclaringType, out referenceId);

            WriteStringReference(writer, item.Name);
            writer.WriteUInt16(referenceId);

            writer.WriteUInt16(_context.SignaturesTable.GetOrCreateSignatureId(item));
            writer.WriteUInt16(0); // padding
        }
    }
}
