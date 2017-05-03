using System;

namespace Moq.Sequences
{
    public class SequenceUsageException : Exception
    {
        private const string Usage = @" Recommended usage:

    var mock = new Mock<Foo>();
    ...    
    using (Sequence.Create())
    {
        mock.Setup(_ => _.Method1()).InSequence();
        mock.Setup(_ => _.Method2()).InSequence(Times.AtMostOnce());
        ...
        using (Sequence.Loop())
        {
            mock.Setup(_ => _.Method3()).InSequence();
            mock.Setup(_ => _.Method4()).InSequence(Times.Exactly(3));
        }
        ...
    }";

        public SequenceUsageException(string message) : base(message + Usage)
        {            
        }
    }
}