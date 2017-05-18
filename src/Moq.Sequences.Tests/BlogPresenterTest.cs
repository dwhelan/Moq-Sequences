using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Moq.Sequences.Tests
{
    public class BlogPresenterTest
    {
        [Test]
        public void Should_show_each_post_once_with_most_recent_first()
        {
            var olderPost = new Post { DateTime = new DateTime(2010, 1, 1) };
            var newerPost = new Post { DateTime = new DateTime(2010, 1, 2) };
            var posts = new List<Post> { newerPost, olderPost };
            var mockView = new Mock<BlogView>();

            var viewOrder = 0;

            mockView.Setup(v => v.ShowPost(newerPost)).Callback(() => Assert.That(viewOrder++, Is.EqualTo(0)));
            mockView.Setup(v => v.ShowPost(olderPost)).Callback(() => Assert.That(viewOrder++, Is.EqualTo(1)));

            new BlogPresenter(mockView.Object).Show(posts);

            mockView.Verify(v => v.ShowPost(newerPost), Times.Once());
            mockView.Verify(v => v.ShowPost(olderPost), Times.Once());
        }

        [Test]
        public void Should_show_each_post_with_most_recent_first_using_sequences()
        {
            var olderPost = new Post { DateTime = new DateTime(2010, 1, 1) };
            var newerPost = new Post { DateTime = new DateTime(2010, 1, 2) };
            var posts = new List<Post> { newerPost, olderPost };

            var mockView = new Mock<BlogView>();

            using (Sequence.Create())
            {
                mockView.Setup(v => v.ShowPost(newerPost)).InSequence();
                mockView.Setup(v => v.ShowPost(olderPost)).InSequence();

                new BlogPresenter(mockView.Object).Show(posts);
            }
        }
    }

    public class Post
    {
        public DateTime DateTime { get; set; }
    }

    public class BlogPresenter
    {
        private readonly BlogView view;

        public BlogPresenter(BlogView view)
        {
            this.view = view;
        }

        public void Show(IEnumerable<Post> posts)
        {
            foreach (var post in posts.OrderByDescending(post => post.DateTime))
                view.ShowPost(post);
        }
    }

    public interface BlogView
    {
        void ShowPost(Post post);
    }
}
