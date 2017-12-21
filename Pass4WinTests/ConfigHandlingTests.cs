﻿namespace Pass4WinTests
{
    using System;
    using NUnit.Framework;
    using Pass4Win;
    using Shouldly;

    [TestFixture]
    public class ConfigHandlingTests
    {
        [SetUp]
        public void BeforeTest()
        {
            Setup.InitializeContainer();
        }

        [Test]
        public void ValueTypes()
        {
            var standardConfig = new ConfigHandling();

            int valueInteger = 5000;
            standardConfig["testInteger"] = valueInteger;

            DateTime valueDate = DateTime.Now;
            standardConfig["testDate"] = valueDate;

            char valueChar = 'x';
            standardConfig["testChar"] = valueChar;

            Assert.AreEqual(valueDate, standardConfig["testDate"]);
            Assert.AreEqual(valueInteger, standardConfig["testInteger"]);
            Assert.AreEqual(valueChar, standardConfig["testChar"]);
        }

        [Test]
        public void ReferenceTypes()
        {
            ConfigHandling standardConfig = new ConfigHandling();

            string referenceString = "referenced string";
            string[] referenceArray = { "referenced", "string"};

            standardConfig["testString"] = referenceString;
            standardConfig["testArray"] = referenceArray;

            Assert.AreEqual(referenceString, standardConfig["testString"]);
            Assert.AreEqual(referenceArray, standardConfig["testArray"]);
        }

        [Test]
        public void SavingLoading()
        {
            int value = 1024;
            {
                var standardConfig = new ConfigHandling {["test"] = value};
                standardConfig.Save();
            }

            {
                var standardConfig = new ConfigHandling();

                Assert.AreEqual(value, standardConfig["test"]);
            }
        }

        [Test]
        public void Delete()
        {
            var standardConfig = new ConfigHandling();
            standardConfig["test"] = 1024;
            standardConfig.Delete("test");
            Assert.IsEmpty(standardConfig["test"]);
        }
    }
}