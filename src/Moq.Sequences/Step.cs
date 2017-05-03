using System.Reflection;

namespace Moq.Sequences
{
    public class Step
    {
        private readonly Times expectedCount;
        private readonly string action;
        private readonly int maxCount;
        private int actualCount;

        internal bool Started { get { return actualCount > 0; } }
        internal bool Complete { get; private set; }

        private static readonly FieldInfo to = typeof(Times).GetField("to", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo verify = typeof(Times).GetMethod("Verify", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo getExceptionMessage = typeof(Times).GetMethod("GetExceptionMessage", BindingFlags.Instance | BindingFlags.NonPublic);

        internal Step(Times expectedCount, string action = "")
        {
            this.expectedCount = expectedCount;
            this.action = action;
            maxCount = (int)to.GetValue(expectedCount);
        }

        internal void CountCall()
        {
            if (Complete)
                throw new SequenceException(this + " is not invokable because it has already completed.");

            if (++actualCount > maxCount)
                throw new SequenceException(GetFailureMessage("Exceeded maximum number of invocations."));
        }

        internal virtual void EnsureComplete(string context)
        {
            if (!Verified())
                throw new SequenceException(GetFailureMessage(context + " but invocations for " + this + " were not completed."));

            Complete = true;
        }

        private bool Verified()
        {
            return (bool) verify.Invoke(expectedCount, new[] { (object)actualCount });
        }
        
        protected virtual string GetFailureMessage(string failMessage)
        {
            return (string) getExceptionMessage.Invoke(expectedCount, new object[] { failMessage, ToString(), actualCount });
        }

        internal virtual void Reset()
        {
            actualCount = 0;
            Complete = false;
        }

        public override string ToString()
        {
            return action;
        }
    }
}