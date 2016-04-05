﻿using NSpec;
using NSpec.Domain;
using NSpecSpecs.WhenRunningSpecs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSpecSpecs.describe_RunningSpecs
{
    [TestFixture]
    [Category("RunningSpecs")]
    [Category("Async")]
    public class describe_async_before_all : when_describing_async_hooks
    {
        class SpecClass : BaseSpecClass
        {
            void given_async_before_all_is_set()
            {
                beforeAllAsync = SetStateAsync;

                it["Should have final value"] = ShouldHaveFinalState;
            }

            void given_async_before_all_fails()
            {
                beforeAllAsync = FailAsync;

                it["Should fail"] = PassAlways;
            }

            void given_both_sync_and_async_before_all_are_set()
            {
                beforeAll = SetAnotherState;

                beforeAllAsync = SetStateAsync;

                it["Should not know what to expect"] = PassAlways;
            }

            void given_before_all_is_set_to_async_lambda()
            {
                beforeAll = async () => { await Task.Delay(0); };

                it["Should fail because beforeAll is set to async lambda"] = PassAlways;

                // No chance of error when (async) return value is explicitly typed. The following do not even compile:
                /*
                Func<Task> asyncTaggedDelegate = async () => { await Task.Delay(0); };
                Func<Task> asyncUntaggedDelegate = () => { return Task.Delay(0); };

                // set to async method
                beforeAll = SetStateAsync;

                // set to async tagged delegate
                beforeAll = asyncTaggedDelegate;

                // set to async untagged delegate
                beforeAll = asyncUntaggedDelegate;
                */
            }
        }

        [SetUp]
        public void setup()
        {
            Run(typeof(SpecClass));
        }

        [Test]
        public void async_before_all_waits_for_task_to_complete()
        {
            ExampleRunsWithExpectedState("Should have final value");
        }

        [Test]
        public void async_before_all_with_exception_fails()
        {
            ExampleRunsWithException("Should fail");
        }

        [Test]
        public void context_with_both_sync_and_async_before_always_fails()
        {
            ExampleRunsWithException("Should not know what to expect");
        }

        [Test]
        public void sync_before_all_set_to_async_lambda_fails()
        {
            ExampleRunsWithException("Should fail because beforeAll is set to async lambda");
        }
    }
}
