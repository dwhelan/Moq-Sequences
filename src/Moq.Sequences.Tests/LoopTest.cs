using NUnit.Framework;

namespace Moq.Sequences.Tests
{
    public class LoopTest
    {
        [Test]
        public void A_new_loop_should_be_added_directly_if_it_contains_no_child_loops()
        {
            using (var loop = new Loop(Times.Once()))
            {
                using (var child = loop.CreateLoop(Times.Once()))
                {
                    Assert.That(loop.Steps.Contains(child));
                }
            }
        }

        [Test]
        public void A_new_loop_should_be_added_to_most_recently_created_child_loop()
        {
            using (var loop = new Loop(Times.Once()))
            {
                using (var child1 = loop.CreateLoop(Times.Once()))
                {
                    using (loop.CreateLoop(Times.Once()))
                    {
                    }

                    using (var child2 = loop.CreateLoop(Times.Once()))
                    {
                        Assert.That(child1.Steps.Contains(child2));

                        using (var child3 = loop.CreateLoop(Times.Once()))
                        {
                            Assert.That(child2.Steps.Contains(child3));
                        }
                    }
                }
            }
        }

        [Test]
        public void A_new_step_should_be_added_directly_if_it_contains_no_child_loops()
        {
            using (var loop = new Loop(Times.Once()))
            {
                var step = loop.CreateStep("", Times.Once());

                Assert.That(loop.Steps.Contains(step));
            }
        }

        [Test]
        public void A_new_step_should_be_added_to_most_recently_created_child_loop()
        {
            using (var loop = new Loop(Times.Once()))
            {
                using (loop.CreateLoop(Times.Once()))
                {
                    using (loop.CreateLoop(Times.Once()))
                    {
                    }

                    using (var child2 = loop.CreateLoop(Times.Once()))
                    {
                        var step2 = loop.CreateStep("", Times.Once());
                        Assert.That(child2.Steps.Contains(step2));

                        using (var child3 = loop.CreateLoop(Times.Once()))
                        {
                            var step3 = loop.CreateStep("", Times.Once());
                            Assert.That(child3.Steps.Contains(step3));
                        }
                    }
                }
            }
        }

        [Test]
        public void RecordCall_should_throw_when_step_count_exceeded()
        {
            using (var loop = new Loop(Times.Once()))
            {
                var step = loop.CreateStep("", Times.Exactly(2));

                loop.RecordCall(step);
                loop.RecordCall(step);

                Assert.Throws<SequenceException>(() => loop.RecordCall(step));
            }
        }

        [Test]
        public void RecordCall_should_throw_when_loop_count_exceeded()
        {
            using (var loop = new Loop(Times.Exactly(2)))
            {
                var step1 = loop.CreateStep("", Times.Once());
                var step2 = loop.CreateStep("", Times.Once());

                loop.RecordCall(step1); loop.RecordCall(step2);
                loop.RecordCall(step1); loop.RecordCall(step2);

                Assert.Throws<SequenceException>(() => loop.RecordCall(step1));
            }
        }

        [Test]
        public void RecordCall_should_throw_if_steps_called_out_of_sequence()
        {
            using (var loop = new Loop(Times.Once()))
            {
                loop.CreateStep("", Times.Once());
                var step = loop.CreateStep("", Times.Once());

                Assert.Throws<SequenceException>(() => loop.RecordCall(step));
            }
        }

        [Test]
        public void RecordCall_should_throw_when_child_loop_count_exceeded()
        {
            using (var loop = new Loop(Times.Once()))
            {
                Step step1;
                Step step2;

                using (loop.CreateLoop(Times.Exactly(2)))
                {
                    step1 = loop.CreateStep("", Times.Once());
                    step2 = loop.CreateStep("", Times.Once());
                }

                loop.RecordCall(step1); loop.RecordCall(step2);
                loop.RecordCall(step1); loop.RecordCall(step2);

                Assert.Throws<SequenceException>(() => loop.RecordCall(step1));
            }
        }

        [Test]
        public void Record_call_should_ensure_all_previous_steps_completed()
        {
            using (var loop = new Loop(Times.Once()))
            {
                var stepBefore = loop.CreateStep("", Times.AtMostOnce());

                Loop nested;
                Step stepNested;

                using (nested = loop.CreateLoop(Times.Once()))
                {
                    stepNested = loop.CreateStep("", Times.Once());
                }

                var stepAfter = loop.CreateStep("", Times.Once());
                
                loop.RecordCall(stepNested);
                Assert.That(stepBefore.Complete, Is.True);
                Assert.That(loop.Complete, Is.False);
                Assert.That(stepNested.Complete, Is.False);
                Assert.That(stepAfter.Complete, Is.False);

                loop.RecordCall(stepAfter);

                Assert.That(stepBefore.Complete, Is.True);
                Assert.That(nested.Complete, Is.True);
                Assert.That(stepNested.Complete, Is.True);
                Assert.That(stepAfter.Complete, Is.False);
            }
        }

        [Test]
        public void EnsureComplete_should_throw_if_minimum_count_not_met()
        {
            using (var loop = new Loop(Times.Once()))
            {
                Assert.Throws<SequenceException>(() => loop.EnsureComplete(""));
                Assert.That(loop.Complete, Is.False);
            }
        }

        [Test]
        public void EnsureComplete_should_throw_if_maximum_count_not_met()
        {
            using (var loop = new Loop(Times.AtLeastOnce()))
            {
                Assert.Throws<SequenceException>(() => loop.EnsureComplete(""));
                Assert.That(loop.Complete, Is.False);
            }
        }

        [Test]
        public void EnsureComplete_should_not_throw_if_minimum_and_maximum_count_met()
        {
            using (var loop = new Loop(Times.Never()))
            {
                loop.EnsureComplete("");
                Assert.That(loop.Complete, Is.True);
            }
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
        public void Dispose_should_do_nothing_if_called_multiple_times()
        {
            using (var loop = new Loop(Times.Once()))
            {
                loop.Dispose();
                loop.Dispose();
            }
        }
    }
}