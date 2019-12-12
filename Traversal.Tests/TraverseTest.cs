﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Medallion.Collections
{
    public class TraverseTest
    {
        [Test]
        public void TestAlong()
        {
            Assert.Throws<ArgumentNullException>(() => Traverse.Along("a", null));

            var ex = new Exception("a", new Exception("b", new Exception("c")));

            CollectionAssert.AreEqual(
                new[] { ex, ex.InnerException, ex.InnerException.InnerException },
                Traverse.Along(ex, e => e.InnerException)
            );

            CollectionAssert.AreEqual(
                Enumerable.Empty<Exception>(),
                Traverse.Along(default(Exception), e => e.InnerException)
            );
        }

        [Test]
        public void TestDepthFirst()
        {
            Assert.Throws<ArgumentNullException>(() => Traverse.DepthFirst("a", null));

            CollectionAssert.AreEqual(
                actual: Traverse.DepthFirst("abcd", s => s.Length < 2 ? Enumerable.Empty<string>() : new[] { s.Substring(0, s.Length - 1), s.Substring(1) }),
                expected: new[]
                {
                    "abcd",
                    "abc",
                    "ab",
                    "a",
                    "b",
                    "bc",
                    "b",
                    "c",
                    "bcd",
                    "bc",
                    "b",
                    "c",
                    "cd",
                    "c",
                    "d"
                }
            );
        }

        [Test]
        public void TestBreadthFirst()
        {
            Assert.Throws<ArgumentNullException>(() => Traverse.BreadthFirst("a", null));

            CollectionAssert.AreEqual(
                actual: Traverse.BreadthFirst("abcd", s => s.Length < 2 ? Enumerable.Empty<string>() : new[] { s.Substring(0, s.Length - 1), s.Substring(1) }),
                expected: new[]
                {
                    "abcd",
                    "abc",
                    "bcd",
                    "ab",
                    "bc",
                    "bc",
                    "cd",
                    "a",
                    "b",
                    "b",
                    "c",
                    "b",
                    "c",
                    "c",
                    "d",
                }
            );
        }

        [Test]
        public void DepthFirstEnumeratorsAreLazyAndDisposeProperly()
        {
            var helper = new EnumeratorHelper();

            var sequence = Traverse.DepthFirst(10, i => helper.MakeEnumerator(i - 1));

            Assert.AreEqual(0, helper.IterateCount);
            Assert.AreEqual(0, helper.StartCount);
            Assert.AreEqual(0, helper.EndCount);

            using (var enumerator = sequence.GetEnumerator())
            {
                for (var i = 0; i < 10; ++i)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                }
                Assert.AreEqual(9, helper.IterateCount); // -1 for root
            }

            Assert.AreEqual(helper.EndCount, helper.StartCount);
        }

        [Test]
        public void BreadthFirstEnumeratorsAreLazyAndDisposeProperly()
        {
            var helper = new EnumeratorHelper();

            var sequence = Traverse.BreadthFirst(10, i => helper.MakeEnumerator(i - 1));

            Assert.AreEqual(0, helper.IterateCount);
            Assert.AreEqual(0, helper.StartCount);
            Assert.AreEqual(0, helper.EndCount);

            using (var enumerator = sequence.GetEnumerator())
            {
                for (var i = 0; i < 10; ++i)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                }
                Assert.AreEqual(9, helper.IterateCount); // -1 for root
            }

            Assert.AreEqual(helper.EndCount, helper.StartCount);
        }

        private class EnumeratorHelper
        {
            public int StartCount { get; private set; }
            public int EndCount { get; private set; }
            public int IterateCount { get; private set; }

            public IEnumerable<int> MakeEnumerator(int i)
            {
                ++this.StartCount;

                try
                {
                    for (var j = 0; j < i; ++j)
                    {
                        ++this.IterateCount;
                        yield return i;
                    }
                }
                finally
                {
                    ++this.EndCount;
                }
            }
        }
    }
}
