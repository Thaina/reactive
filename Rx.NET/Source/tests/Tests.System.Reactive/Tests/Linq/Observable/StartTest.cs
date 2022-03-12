// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using NUnit.Framework;
using Rx.Unity.Tests.Helper;

namespace ReactiveTests.Tests
{
    public partial class StartTest : ReactiveTest
    {

        [Test]
        public void Start_ArgumentChecking()
        {
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.Start(null));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.Start<int>(null));

            var someScheduler = new TestScheduler();
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.Start(null, someScheduler));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.Start<int>(null, someScheduler));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.Start(() => { }, null));
            ReactiveAssert.Throws<ArgumentNullException>(() => Observable.Start(() => 1, null));
        }


        [UnityEngine.TestTools.UnityTest]
        public IEnumerator Start_Action()
        {
            var done = false;
            var list = new List<Unit>();
            foreach(var unit in Observable.Start(() => { done = true; }).ToEnumerable())
            {
                list.Add(unit);
                yield return null;
            }
            Assert.That(list,Has.Count.EqualTo(1));
            Assert.True(done, "done");
        }

        [Test]
        public void Start_Action2()
        {
            var scheduler = new TestScheduler();

            var done = false;

            var res = scheduler.Start(() =>
                Observable.Start(() => { done = true; }, scheduler)
            );

            res.Messages.AssertEqual(
                OnNext(200, new Unit()),
                OnCompleted<Unit>(200)
            );

            Assert.True(done, "done");
        }

        [Test]
        public void Start_ActionError()
        {
            var ex = new Exception();

            var res = Observable.Start(() => { throw ex; }).Materialize().ToEnumerable();

            Assert.True(res.SequenceEqual(new[] {
                Notification.CreateOnError<Unit>(ex)
            }));
        }

        [Test]
        public void Start_Func()
        {
            var res = Observable.Start(() => 1).ToEnumerable();

            Assert.True(res.SequenceEqual(new[] {
                1
            }));
        }

        [Test]
        public void Start_Func2()
        {
            var scheduler = new TestScheduler();

            var res = scheduler.Start(() =>
                Observable.Start(() => 1, scheduler)
            );

            res.Messages.AssertEqual(
                OnNext(200, 1),
                OnCompleted<int>(200)
            );
        }

        [Test]
        public void Start_FuncError()
        {
            var ex = new Exception();

            var res = Observable.Start<int>(() => { throw ex; }).Materialize().ToEnumerable();

            Assert.True(res.SequenceEqual(new[] {
                Notification.CreateOnError<int>(ex)
            }));
        }

    }
}
