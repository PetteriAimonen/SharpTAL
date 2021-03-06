﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using NUnit.Framework;

namespace SharpTAL.SharpTALTests.TALTests
{
	[TestFixture]
	public class TALReplaceTests
	{
		public static SharpTAL.ITemplateCache cache;
		public static Dictionary<string, object> globals;

		[TestFixtureSetUp]
		public void SetUpClass()
		{
			// Using FileSystemTemplateCache in this tests
			string cacheFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Template Cache");
			if (!Directory.Exists(cacheFolder))
			{
				Directory.CreateDirectory(cacheFolder);
			}
			cache = new FileSystemTemplateCache(cacheFolder, true, typeof(TALReplaceTests).Name + "_{key}.dll");
		}

		[TestFixtureTearDown]
		public void CleanupClass()
		{
		}

		[SetUp]
		public void SetUp()
		{
			string entry = @"Some structure: <b tal:content=""weblog/subject""></b>";
			Dictionary<string, string> weblog = new Dictionary<string, string>
            {
                { "subject", "Test subject" },
                { "entry",  entry }
            };
			globals = new Dictionary<string, object>();
			globals.Add("test", "testing");
			globals.Add("one", new List<object>() { 1 });
			globals.Add("two", new List<object>() { "one", "two" });
			globals.Add("three", new List<object>() { 1, "Two", 3 });
			globals.Add("weblog", weblog);
		}

		public static void RunTest(string template, string expected, string errMsg)
		{
			RunTest(template, expected, errMsg, null);
		}

		public static void RunTest(string template, string expected, string errMsg,
			Dictionary<string, string> inlineTemplates)
		{
			string actual = cache.RenderTemplate(template, globals, inlineTemplates);
			actual = actual.Replace("{", "{{").Replace("}", "}}");
			Assert.AreEqual(expected, actual, "{1} - {0}template: {2}{0}actual: {3}{0}expected: {4}",
				Environment.NewLine, errMsg, template, actual, expected);
		}

		[Test]
		public void TestContentNull()
		{
			RunTest(@"<html><p tal:replace=""null""></p></html>",
				"<html></html>",
				"Content of nothing did not remove tag.");
		}

		[Test]
		public void TestContentDefault()
		{
			RunTest(@"<html><p tal:replace=""default"">Original</p></html>",
				"<html><p>Original</p></html>",
				"Content of default did not evaluate to existing content without tags");
		}

		[Test]
		public void TestContentString()
		{
			RunTest(@"<html><p tal:replace=""test"">Original</p></html>",
				"<html>testing</html>",
				"Content of string did not evaluate to contain string");
		}

		[Test]
		public void TestContentStructure()
		{
			string entry = @"<p metal:define-macro=""entry"">Some structure: <b tal:content='weblog[""subject""]'></b></p>";
			Dictionary<string, string> inlineTemplates = new Dictionary<string, string>()
            {
                { "entry_macros",  entry }
            };
			Dictionary<string, object> weblog = new Dictionary<string, object>
            {
                { "subject", "Test subject" },
            };
			globals["weblog"] = weblog;
			RunTest(@"<html><p metal:use-macro='entry_macros.macros[""entry""]'>Original</p></html>",
				"<html><p>Some structure: <b>Test subject</b></p></html>",
				"Content of Structure did not evaluate to expected result",
				inlineTemplates);
		}
	}
}
