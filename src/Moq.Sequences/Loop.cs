using System;
using System.Collections.Generic;
using System.Linq;

namespace Moq.Sequences
{
    public class Loop : Step, IDisposable
    {
        private readonly List<Step> steps = new List<Step>();

        internal IList<Step> Steps { get { return steps.AsReadOnly(); } }
        internal bool Disposed { get; private set; }

        internal Loop(Times times) : base(times)
        {
        }

        internal Loop CreateLoop(Times times)
        {
            var loop = new Loop(times);
            LastCreatedActiveLoop().steps.Add(loop);
            return loop;
        }

        internal Step CreateStep(object setup, Times expectedCalls)
        {
            var step = new Step(expectedCalls, "''" + setup + "'");
            LastCreatedActiveLoop().steps.Add(step);
            return step;
        }

        private Loop LastCreatedActiveLoop()
        {
            var lastCreatedLoop = (from loop in steps.OfType<Loop>() select loop).LastOrDefault(loop => loop.Disposed == false);
            return lastCreatedLoop == null ? this : lastCreatedLoop.LastCreatedActiveLoop();
        }

        internal virtual bool RecordCall(Step calledStep)
        {
            foreach (var step in steps)
            {
                if (step == calledStep)
                {
                    CountCall(calledStep);
                    return true;
                }

                if (step is Loop)
                {
                    var loop = (Loop) step;

                    if (loop.RecordCall(calledStep))
                        return true;
                }

                step.EnsureComplete(calledStep + " was called");
            }

            return false;
        }

        private void CountCall(Step calledStep)
        {
            if (EnteringLoopWith(calledStep))
            {
                if (Started)
                    ResetSteps();

                calledStep.CountCall();
                CountCall();
            }
            else
                calledStep.CountCall();
        }

        private bool EnteringLoopWith(Step calledStep)
        {
            return !Started || calledStep.Complete;
        }

        internal override void EnsureComplete(string context)
        {
            steps.ForEach(step => step.EnsureComplete(context));
            base.EnsureComplete(context);
        }

        internal override void Reset()
        {
            ResetSteps();
            base.Reset();
        }

        private void ResetSteps()
        {
            steps.ForEach(step => step.Reset());
        }

        protected override string GetFailureMessage(string errorMessage)
        {
            return base.GetFailureMessage(errorMessage).Replace("invocation on the mock", "loop to be executed");
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            DoDisposalChecks();
        }

        protected virtual void DoDisposalChecks()
        {
            if (steps.OfType<Loop>().Any(step => !(step).Disposed))
                throw new SequenceUsageException("You must dispose the Loop created via MockSequence.Loop().");
        }
    }
}