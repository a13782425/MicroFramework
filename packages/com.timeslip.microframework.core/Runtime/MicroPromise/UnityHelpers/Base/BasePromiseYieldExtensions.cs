using MFramework.Core.CompilerServices;
using System.Collections;
using UnityEngine;

namespace MFramework.Core
{
    partial class PromiseYieldExtensions
    {
        /// <summary>
        /// Runs a <see cref="IEnumerator"/> that enumerator the <paramref name="enumerator"/>, and
        /// returns a <see cref="Promise"/> that will resolve after the <paramref name="enumerator"/> has completed.
        /// </summary>
        /// <param name="enumerator">The enumerator to wait for.</param>
        public static PromiseAwaiterVoid GetAwaiter(this IEnumerator enumerator)
            => new PromiseAwaiterVoid(InternalHelper.YieldInstructionRunner.WaitForInstruction(enumerator, null, default));

        /// <summary>
        /// Gets an enumerator wrapping the <paramref name="enumerator"/>, with cancelation.
        /// </summary>
        public static Promise WithCancelation(this IEnumerator enumerator, CancelationToken cancelationToken)
            => InternalHelper.YieldInstructionRunner.WaitForInstruction(enumerator, null, cancelationToken);

        /// <summary>
        /// Runs a <see cref="YieldInstruction"/> that yields the <paramref name="instruction"/>, and
        /// returns a <see cref="Promise"/> that will resolve after the <paramref name="instruction"/> has completed.
        /// </summary>
        /// <param name="instruction">The yield instruction to wait for.</param>
        public static PromiseAwaiterVoid GetAwaiter(this YieldInstruction instruction)
            => new PromiseAwaiterVoid(InternalHelper.YieldInstructionRunner.WaitForInstruction(instruction, null, default));

        /// <summary>
        /// Gets an enumerator wrapping the <paramref name="instruction"/>, with cancelation.
        /// </summary>
        public static Promise WithCancelation(this YieldInstruction instruction, CancelationToken cancelationToken)
            => InternalHelper.YieldInstructionRunner.WaitForInstruction(instruction, null, cancelationToken);

        /// <summary>
        /// Runs a <see cref="uint"/> that number of frames the <paramref name="number"/>, and
        /// returns a <see cref="Promise"/> that will resolve after the <paramref name="number"/> has completed.
        /// </summary>
        /// <param name="number">The number of frames to wait for.</param>
        public static AwaitInstructionAwaiter<PromiseYielder.Instructions.WaitFramesInstruction> GetAwaiter(this uint number)
            => new PromiseYielder.Instructions.WaitFramesInstruction(number).GetAwaiter();

        /// <summary>
        /// Runs a <see cref="int"/> that number of frames the <paramref name="number"/>, and
        /// returns a <see cref="Promise"/> that will resolve after the <paramref name="number"/> has completed.
        /// </summary>
        /// <param name="number">The number of frames to wait for.</param>
        public static AwaitInstructionAwaiter<PromiseYielder.Instructions.WaitFramesInstruction> GetAwaiter(this int number)
           => new PromiseYielder.Instructions.WaitFramesInstruction(number < 0 ? 0 : (uint)number).GetAwaiter();
    }
}
