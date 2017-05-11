﻿using FluentAssertions;
using NUnit.Framework;

namespace NSpec.Tests.WhenRunningSpecs.BeforeAndAfter
{
    [TestFixture]
    public class inheritance : when_running_specs
    {
        class BaseSpec : sequence_spec
        {
            void before_all()
            {
                sequence = "A";
            }

            void before_each()
            {
                sequence += "C";
            }

            void after_each()
            {
                sequence += "F";
            }

            void after_all()
            {
                sequence += "H";
            }
        }

        class DerivedSpec : BaseSpec
        {
            void a_context()
            {
                beforeAll = () => sequence += "B";

                before = () => sequence += "D";
                specify = () => Assert.That(true, Is.True);
                after = () => sequence += "E";

                afterAll = () => sequence += "G";
            }
        }

        [SetUp]
        public void setup()
        {
            Run(typeof(DerivedSpec));
        }

        [Test]
        public void before_alls_at_every_level_run_before_before_eaches_from_the_outside_in()
        {
            DerivedSpec.sequence.Should().StartWith("ABCD");
        }

        [Test]
        public void after_alls_at_every_level_run_after_after_eaches_from_the_inside_out()
        {
            DerivedSpec.sequence.Should().EndWith("EFGH");
        }
    }
}