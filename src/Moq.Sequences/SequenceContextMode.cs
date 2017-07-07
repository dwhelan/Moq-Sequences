using System;
using System.Collections.Generic;
#if FEATURE_CALLCONTEXT
using System.Runtime.Remoting.Messaging;
#elif FEATURE_ASYNCLOCAL
using System.Threading;
#endif
using System.Threading.Tasks;

namespace Moq.Sequences
{
    /// <summary>
    /// Determines how <see cref="Sequence"/>s behave in the presence of concurrency or asynchrony.
    /// You can choose a context mode by setting the static <see cref="Sequence.ContextMode"/> property.
    /// Note that this is a global setting; you cannot set a context mode for an individual sequence.
    /// </summary>
    public abstract class SequenceContextMode
    {
        /// <summary>
        /// With this mode, each sequence is restricted (and only visible) to the thread that created it.
        /// This context mode will not work well together with the Task Parallel Library (TPL) or
        /// <see langword="async"/> / <see langword="await"/>. However, for backwards compatibility, it
        /// is the default mode.
        /// </summary>
        public static SequenceContextMode Thread { get; } = new SequenceThreadContextMode();

        /// <summary>
        /// With this mode, sequences "flow" with the current call context, even across async boundaries.
        /// This means that you can create a sequence in an <see langword="async"/> method and it will
        /// still be accessible after any <see langword="await"/>s. Sequences will also flow from parent
        /// to child threads or <see cref="Task"/>s. This is the recommended mode if your code makes use
        /// of the Task Parallel Library (TPL) or <see langword="async"/> / <see langword="await"/>.
        /// You need to opt in to this mode, since it is not the default.
        /// </summary>
        public static SequenceContextMode Async { get; } = new SequenceAsyncContextMode();

        private SequenceContextMode()
        {
        }

        /// <summary>
        /// Gets or sets the currently active ambient sequence.
        /// </summary>
        internal abstract Sequence ActiveSequence { get; set; }

        private sealed class SequenceThreadContextMode : SequenceContextMode
        {
            [ThreadStatic]
            private static Sequence activeSequence;

            internal override Sequence ActiveSequence
            {
                get => SequenceThreadContextMode.activeSequence;
                set => SequenceThreadContextMode.activeSequence = value;
            }
        }

        private sealed class SequenceAsyncContextMode : SequenceContextMode
        {
#if FEATURE_ASYNCLOCAL
            private static AsyncLocal<Sequence> activeSequence = new AsyncLocal<Sequence>();

            internal override Sequence ActiveSequence
            {
                get => SequenceAsyncContextMode.activeSequence.Value;
                set => SequenceAsyncContextMode.activeSequence.Value = value;
            }
#elif FEATURE_CALLCONTEXT
            private const string activeSequenceDataId = "Moq.Sequences.SequenceContextMode+SequenceAsyncContextMode.ActiveSequence";

            internal override Sequence ActiveSequence
            {
                get => CallContext.LogicalGetData(activeSequenceDataId) as Sequence;
                set => CallContext.LogicalSetData(activeSequenceDataId, value);
            }
#endif
        }
    }
}
