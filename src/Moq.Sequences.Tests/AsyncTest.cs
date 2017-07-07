using NUnit.Framework;
using System.Threading.Tasks;

namespace Moq.Sequences.Tests
{
    public class AsyncTest
    {
        [SetUp]
        public void SwitchToAsyncContextMode()
        {
            Sequence.ContextMode = SequenceContextMode.Async;
        }

        [TearDown]
        public void ResetToThreadContextMode()
        {
            Sequence.ContextMode = SequenceContextMode.Thread;
        }

        [Test]
        public async Task Sequence_verification_works_after_await()
        {
            using (Sequence.Create())
            {
                var fooMock = new Mock<IFoo>();
                fooMock.Setup(f => f.Fooxiate()).InSequence();

                var barMock = new Mock<IBar>();
                barMock.Setup(b => b.Baronize()).InSequence();

                var sut = new SomeClass(fooMock.Object, barMock.Object);

                var result = await sut.DoMyStuffAsync();

                Assert.AreEqual("someString", result);
            }
        }

        [Test]
        public async Task Cannot_create_new_sequence_after_await_when_another_sequence_should_be_active()
        {
            using (Sequence.Create())
            {
                var fooMock = new Mock<IFoo>();
                fooMock.Setup(f => f.Fooxiate()).InSequence();

                var barMock = new Mock<IBar>();
                barMock.Setup(b => b.Baronize()).InSequence();

                var sut = new SomeClass(fooMock.Object, barMock.Object);

                var result = await sut.DoMyStuffAsync();

                Assert.Throws<SequenceUsageException>(() => Sequence.Create());
            }
        }

        public class SomeClass
        {
            private readonly IFoo _foo;
            private readonly IBar _bar;

            public SomeClass(IFoo foo, IBar bar)
            {
                _bar = bar;
                _foo = foo;
            }

            public async Task<string> DoMyStuffAsync()
            {
                return await Task.Run(() => DoMyStuff());
            }

            private string DoMyStuff()
            {
                _foo.Fooxiate();
                _bar.Baronize();
                return "someString";
            }
        }

        public interface IBar
        {
            void Baronize();
        }

        public interface IFoo
        {
            void Fooxiate();
        }
    }
}
