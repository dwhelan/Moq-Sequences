using System.Reflection;
using System.Text;

namespace Moq.Sequences
{
    public class Step
    {
        private readonly Times expectedCount;
        private readonly string action;
        private int actualCount;

        internal bool Started { get { return actualCount > 0; } }
        internal bool Complete { get; private set; }

        private static readonly MethodInfo getExceptionMessage = typeof(Times).GetMethod("GetExceptionMessage", BindingFlags.Instance | BindingFlags.NonPublic);

        internal Step(Times expectedCount, string action = "")
        {
            this.expectedCount = expectedCount;
            this.action = action;
        }

        internal void CountCall()
        {
            if (Complete)
                throw new SequenceException(this + " is not invokable because it has already completed.");

            var (_, maxCount) = expectedCount;
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
            var (minCount, maxCount) = expectedCount;
            return minCount <= actualCount && actualCount <= maxCount;
        }
        
        protected virtual string GetFailureMessage(string failMessage)
        {
            var message = new StringBuilder();
            message.AppendLine(failMessage);
            message.Append((string)getExceptionMessage.Invoke(expectedCount, new object[] { actualCount }));
            message.AppendLine(action);
            return message.ToString();
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