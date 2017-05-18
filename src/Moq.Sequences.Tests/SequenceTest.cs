using System.Threading;

using NUnit.Framework;
using System.Threading.Tasks;

namespace Moq.Sequences.Tests
{
    public class SequenceTest
    {
        private Mock<I> mock;

        [SetUp]
        public void SetUp()
        {
            mock = new Mock<I>();
        }

        [Test]
        public void Loop_with_prior_missing_call()
        {
            using (Sequence.Create())
            {
                mock.Setup(_ => _.Method1()).InSequence();

                using (Sequence.Loop(Times.Exactly(1)))
                {
                    mock.Setup(_ => _.Method2()).InSequence();
                }

                Assert.Throws<SequenceException>(() => mock.Object.Method2());
            }
        }

        [Test]
        public void Loop_steps_with_non_loop_steps()
        {
            using (Sequence.Create())
            {
                mock.Setup(_ => _.Method1()).InSequence();

                using (Sequence.Loop(Times.Exactly(1)))
                {
                    mock.Setup(_ => _.Method2()).InSequence();
                }

                mock.Setup(_ => _.Method3()).InSequence();

                mock.Object.Method1();
                mock.Object.Method2();
                mock.Object.Method3();
            }
        }

        [Test]
        public void Nested_loops()
        {
            using (Sequence.Create())
            {
                mock.Setup(_ => _.Method1()).InSequence();

                using (Sequence.Loop(Times.Exactly(1)))
                {
                    mock.Setup(_ => _.Method2()).InSequence();

                    using (Sequence.Loop(Times.Exactly(1)))
                    {
                        mock.Setup(_ => _.Method3()).InSequence();
                    }
                }

                mock.Object.Method1();
                mock.Object.Method2();
                mock.Object.Method3();
            }
        }

        [Test]
        public void Empty_sequence_is_ignored()
        {
            using (Sequence.Create())
            {
            }
        }

        [Test]
        public void Explicit_MockSequence_Dispose_is_ignored()
        {
            using (var sequence = Sequence.Create())
            {
                sequence.Dispose();
                sequence.Dispose();
                sequence.Dispose();
            }
        }

        [Test]
        public void Method_call_in_sequence()
        {
            using (Sequence.Create())
            {
                mock.Setup(_ => _.Method1()).InSequence();
                mock.Setup(_ => _.Method2()).InSequence();

                mock.Object.Method1();
                mock.Object.Method2();
            }
        }

        [Test]
        public void Method_call_out_of_sequence()
        {
            using (Sequence.Create())
            {
                mock.Setup(_ => _.Method1()).InSequence(Times.AtMostOnce());
                mock.Setup(_ => _.Method2()).InSequence();

                mock.Object.Method2();
                Assert.Throws<SequenceException>(() => mock.Object.Method1());
            }
        }

        [Test]
        public void Method_call_before_previous_one_verified()
        {
            using (Sequence.Create())
            {
                mock.Setup(_ => _.Method1()).InSequence();
                mock.Setup(_ => _.Method2()).InSequence();

                Assert.Throws<SequenceException>(() => mock.Object.Method2());
            }
        }

        [Test]
        public void Property_get_in_sequence()
        {
            using (Sequence.Create())
            {
                mock.SetupGet(_ => _.Property1).InSequence().Returns(0);
                mock.SetupGet(_ => _.Property2).InSequence().Returns(0);

                var foo = mock.Object.Property1;
                foo = mock.Object.Property2;
            }
        }

        [Test]
        public void Property_get_out_of_sequence()
        {
            using (Sequence.Create())
            {
                mock.SetupGet(_ => _.Property1).InSequence(Times.AtMostOnce()).Returns(0);
                mock.SetupGet(_ => _.Property2).InSequence().Returns(0);

                int foo = mock.Object.Property2;
                Assert.Throws<SequenceException>(() => foo = mock.Object.Property1);
            }
        }

        [Test]
        public void Property_get_before_previous_one_verified()
        {
            using (Sequence.Create())
            {
                mock.SetupGet(_ => _.Property1).InSequence().Returns(0);
                mock.SetupGet(_ => _.Property2).InSequence().Returns(0);

                int foo;
                Assert.Throws<SequenceException>(() => foo = mock.Object.Property2);
            }
        }

        [Test]
        public void Property_get_via_Setup()
        {
            using (Sequence.Create())
            {
                mock.Setup(_ => _.Property1).InSequence().Returns(0);
                mock.Setup(_ => _.Property2).InSequence().Returns(0);

                var foo = mock.Object.Property1;
                foo = mock.Object.Property2;
            }
        }

        [Test]
        public void Property_set_in_sequence()
        {
            using (Sequence.Create())
            {
                mock.SetupSet(_ => _.Property1 = 0).InSequence();
                mock.SetupSet(_ => _.Property2 = 0).InSequence();

                mock.Object.Property1 = 0;
                mock.Object.Property2 = 0;
            }
        }

        [Test]
        public void Property_set_out_of_sequence()
        {
            using (Sequence.Create())
            {
                mock.SetupSet(_ => _.Property1 = 0).InSequence(Times.AtMostOnce());
                mock.SetupSet(_ => _.Property2 = 0).InSequence();

                mock.Object.Property2 = 0;
                Assert.Throws<SequenceException>(() => mock.Object.Property1 = 0);
            }
        }

        [Test]
        public void Property_set_before_previous_one_verified()
        {
            using (Sequence.Create())
            {
                mock.SetupSet(_ => _.Property1 = 0).InSequence();
                mock.SetupSet(_ => _.Property2 = 0).InSequence();

                Assert.Throws<SequenceException>(() => mock.Object.Property2 = 0);
            }
        }

        [Test]
        public void Multiple_mocks_in_same_sequence()
        {
            var mock1 = new Mock<I>();
            var mock2 = new Mock<I>();

            using (Sequence.Create())
            {
                mock1.Setup(_ => _.Method1()).InSequence();
                mock2.Setup(_ => _.Method1()).InSequence();

                Assert.Throws<SequenceException>(() => mock2.Object.Method1());
            }
        }

        [Test]
        public void Enforce_active_sequence_for_Setup_InSequence_extension()
        {
            Assert.Throws<SequenceUsageException>(() => mock.Setup(_ => _.Method1()).InSequence());
        }

        [Test]
        public void Enforce_active_sequence_for_mock_calls_within_sequences()
        {
            using (Sequence.Create())
            {
                mock.Setup(_ => _.Method1()).InSequence(Times.AtMostOnce());
            }

            Assert.Throws<SequenceUsageException>(() => mock.Object.Method1());
        }

        [Test]
        public void Enforce_mock_calls_outside_sequences_if_mock_is_strict()
        {
            var strictMock = new Mock<I>(MockBehavior.Strict);

            using (Sequence.Create())
            {
            }

            Assert.Throws<MockException>(() => strictMock.Object.Method1());
        }

        [Test]
        public void Enforce_active_sequence_for_loop()
        {
            Assert.Throws<SequenceUsageException>(() => Sequence.Loop());
        }

        [Test]
        public void Enforce_only_one_active_sequence()
        {
            using (Sequence.Create())
            {
                Assert.Throws<SequenceUsageException>(() => Sequence.Create());
            }
        }

        [Test]
        public void Enforce_that_loops_must_be_disposed()
        {
            Assert.Throws<SequenceUsageException>(delegate
            {
                using (Sequence.Create())
                {
                    Sequence.Loop();
                }
            });
        }

        [Test]
        public void Enforce_that_all_steps_in_a_sequence_must_be_completed()
        {
            Assert.Throws<SequenceException>(delegate
            {
                using (Sequence.Create())
                {
                    mock.Setup(_ => _.Method1()).InSequence();
                }
            });
        }

        [Test]
        public void Support_multiple_sequences_in_different_threads()
        {
            var mock1 = new Mock<I>();
            var mock2 = new Mock<I>();

            using (Sequence.Create())
            {
                mock1.Setup(_ => _.Method1()).InSequence();
                mock1.Setup(_ => _.Method2()).InSequence();

                var calledOnThread = new AutoResetEvent(false);
                var calledOnMain = new AutoResetEvent(false);

                new Thread(() =>
                {
                    using (Sequence.Create())
                    {
                        mock2.Setup(_ => _.Method1()).InSequence();
                        mock2.Setup(_ => _.Method2()).InSequence();

                        calledOnMain.WaitOne();
                        mock2.Object.Method1();
                        calledOnThread.Set();

                        calledOnMain.WaitOne();
                        mock2.Object.Method2();
                        calledOnThread.Set();
                    }
                }).Start();

                mock1.Object.Method1();
                calledOnMain.Set();

                calledOnThread.WaitOne();
                mock1.Object.Method2();
                calledOnMain.Set();
            }
        }

        public interface I
        {
            void Method1();
            void Method2();
            void Method3();
            void Method4(int foo);
            int Property1 { get; set; }
            int Property2 { get; set; }
        }
    }
}