using NUnit.Framework;

namespace Moq.Sequences.Tests
{
    public class StepTest
    {
        [Test]
        public void Should_throw_immediately_once_call_count_exceeds_maximum()
        {
            var step = new Step(Times.AtMostOnce());

            step.CountCall();
            Assert.Throws<SequenceException>(step.CountCall);
        }

        [Test]
        public void Should_not_be_complete_if_call_count_less_than_minimum()
        {
            var step = new Step(Times.AtLeastOnce());

            Assert.Throws<SequenceException>(() => step.EnsureComplete(""));
        }

        [Test]
        public void Should_throw_if_CountCall_invoked_after_step_is_complete()
        {
            var step = new Step(Times.AtMostOnce());

            step.EnsureComplete("");

            Assert.Throws<SequenceException>(step.CountCall);
        }

        [Test]
        public void Should_only_be_started_after_first_call()
        {
            var step = new Step(Times.AtMostOnce());

            Assert.That(step.Started, Is.False);

            step.CountCall();

            Assert.That(step.Started, Is.True);
        }

        [Test]
        public void EnsureComplete_should_throw_if_minimum_count_not_met()
        {
            var step = new Step(Times.Once());

            Assert.Throws<SequenceException>(() => step.EnsureComplete(""));
            Assert.That(step.Complete, Is.False);
        }

        [Test]
        public void EnsureComplete_should_throw_if_maximum_count_not_met()
        {
            var step = new Step(Times.AtLeastOnce());

            Assert.Throws<SequenceException>(() => step.EnsureComplete(""));
            Assert.That(step.Complete, Is.False);
        }

        [Test]
        public void EnsureComplete_should_not_throw_if_minimum_and_maximum_count_met()
        {
            var step = new Step(Times.Once());
            step.CountCall();

            step.EnsureComplete("");
            Assert.That(step.Complete, Is.True);
        }

        [Test]
        public void Dispose_should_throw_if_child_loops_not_disposed()
        {
            Assert.Throws<SequenceUsageException>(delegate
            {
                using (var loop = new Loop(Times.Once()))
                {
                    loop.CreateLoop(Times.Exactly(2));
                }
            });
        }

        [Test]
        public void Reset_should_clear_call_count_and_Complete_status()
        {
            var step = new Step(Times.Once());
            step.CountCall();

            step.EnsureComplete("");

            step.Reset();
            Assert.That(step.Complete, Is.False);
            step.CountCall();
        }

        [Test]
        public void Reset_should_make_Started_property_true()
        {
            var step = new Step(Times.AtMostOnce());
            step.CountCall();
            Assert.That(step.Started, Is.True);

            step.Reset();

            Assert.That(step.Started, Is.False);
        }
    }
}