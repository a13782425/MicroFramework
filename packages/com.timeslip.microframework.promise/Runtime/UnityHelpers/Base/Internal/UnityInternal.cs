#if PROTO_PROMISE_DEBUG_ENABLE || (!PROTO_PROMISE_DEBUG_DISABLE && DEBUG)
#define PROMISE_DEBUG
#else
#undef PROMISE_DEBUG
#endif

#pragma warning disable IDE0074 // Use compound assignment
#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MFramework.Core.Promise
{
    partial class Internal
    {
        static partial void Assert(bool condition, string argName, int skipFrames);

#if PROMISE_DEBUG
        static partial void Assert(bool condition, string argName, int skipFrames)
        {
            if (!condition)
            {
                throw new PromiseInvalidArgumentException(argName, "Assert defeat", GetFormattedStacktrace(skipFrames + 1));
            }
        }
#endif

    }
}
