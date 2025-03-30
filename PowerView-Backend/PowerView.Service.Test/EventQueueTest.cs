using System;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace PowerView.Service.Test
{
    [TestFixture]
    public class EventQueueTest
    {
        private EventQueue target;
        private ManualResetEventSlim waitHandle;

        [SetUp]
        public void SetUp()
        {
            target = new EventQueue(new NullLogger<EventQueue>());
            waitHandle = new ManualResetEventSlim();
        }

        [TearDown]
        public void TearDown()
        {
            target.Dispose();
            waitHandle.Dispose();
        }

        [Test]
        public void ConstructorThrows()
        {
            // Arrange

            // Act & Assert
            Assert.That(() => new EventQueue(null), Throws.ArgumentNullException);
        }

        [Test]
        public void EnqueueFirstOneItem()
        {
            // Arrange

            // Act
            target.InsertFirst(() => { waitHandle.Set(); });

            // Assert
            AssertWaitHandle();
        }

        [Test]
        public void EnqueueOneItem()
        {
            // Arrange

            // Act
            target.Enqueue(() => { waitHandle.Set(); });

            // Assert
            AssertWaitHandle();
        }

        [Test]
        public void EnqueueOneItemThatThrowsAndOneNormal()
        {
            // Arrange

            // Act
            target.Enqueue(() => { throw new ApplicationException("This is intentional"); });
            target.Enqueue(() => { waitHandle.Set(); });

            // Assert
            AssertWaitHandle();
        }

        [Test]
        public void EnqueueManyItemsWhileProcessing()
        {
            // Arrange

            // Act
            for (var i = 0; i < 100; i++)
            {
                target.Enqueue(() => { Thread.Sleep(5); });
                Thread.Sleep(1); // Some kind of suspension to cause overlap with EventQueue processing
            }
            target.Enqueue(() => { waitHandle.Set(); });

            // Assert
            AssertWaitHandle();
        }

        [Test]
        public void DisposeWhileProcessing()
        {
            // Arrange
            for (var i = 0; i < 100; i++)
            {
                target.Enqueue(() => { Thread.Sleep(5); });
            }
            bool lastCompleted = false;
            target.Enqueue(() => { lastCompleted = true; });

            // Act
            target.Dispose();

            // Assert
            Assert.That(lastCompleted, Is.False);
        }


        private void AssertWaitHandle()
        {
            Assert.That(waitHandle.Wait(1000), Is.True, "WaitHandle not set within expected timeframe");
        }

    }
}

