using Moq.Language.Flow;

namespace Moq.Sequences
{
    public static class SequenceExtensions
    {
        public static ISetup<T> InSequence<T>(this ISetup<T> setup) where T : class
        {
            return InSequence(setup, Times.Once());
        }

        public static ISetup<T> InSequence<T>(this ISetup<T> setup, Times expectedCalls) where T : class
        {
            AddStep(setup, expectedCalls);
            return setup;
        }

        public static ISetup<T, TResult> InSequence<T, TResult>(this ISetup<T, TResult> setup) where T : class
        {
            return InSequence(setup, Times.Once());
        }

        public static ISetup<T, TResult> InSequence<T, TResult>(this ISetup<T, TResult> setup, Times expectedCalls) where T : class
        {
            AddStep(setup, expectedCalls);
            return setup;
        }

        public static ISetupGetter<T, TProperty> InSequence<T, TProperty>(this ISetupGetter<T, TProperty> setup) where T : class
        {
            return InSequence(setup, Times.Once());
        }

        public static ISetupGetter<T, TProperty> InSequence<T, TProperty>(this ISetupGetter<T, TProperty> setup, Times expectedCalls) where T : class
        {
            AddStep(setup, expectedCalls);
            return setup;
        }

        internal static void AddStep<T>(ISetup<T> setup, Times expectedCalls) where T : class
        {
            var step = Sequence.Step(setup, expectedCalls);
            setup.Callback(() => Sequence.Record(step));
        }

        internal static void AddStep<T, TResult>(ISetup<T, TResult> setup, Times expectedCalls) where T : class
        {
            var step = Sequence.Step(setup, expectedCalls);
            setup.Callback(() => Sequence.Record(step));
        }

        internal static void AddStep<T, TResult>(ISetupGetter<T, TResult> setup, Times expectedCalls) where T : class
        {
            var step = Sequence.Step(setup, expectedCalls);
            setup.Callback(() => Sequence.Record(step));
        }
    }
}