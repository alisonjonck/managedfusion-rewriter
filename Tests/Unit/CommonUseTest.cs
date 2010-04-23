﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ManagedFusion.Rewriter.Test
{
	[TestClass]
	public class CommonUseTest : BaseTest
	{
		[TestMethod]
		public void AddTrailingSlash()
		{
			var target = CreateRuleSet(@"
RewriteRule ^([^.?]+[^.?/])$  $1/ [R=301,L]");

			var url = new Uri("http://somesite.com/pass");
			var context = CreateHttpContext(url);

			string expected = "http://somesite.com/pass/";
			Uri resultUrl = target.RunRules(context, url);
			string result = context.Response.RedirectLocation;

			Assert.IsNull(resultUrl);
			Assert.AreEqual(expected, result);

			url = new Uri("http://somesite.com/fail/");
			context = CreateHttpContext(url);

			resultUrl = target.RunRules(context, url);
			result = context.Response.RedirectLocation;

			Assert.IsNull(resultUrl);
			Assert.IsNull(result);
		}

		/// <summary>
		/// Forces the host to use WWW.
		/// </summary>
		[TestMethod]
		public void ForceHostToUseWww()
		{
			var target = CreateRuleSet(@"
RewriteCond %{HTTP_HOST} !^(www).*$ [NC]
RewriteRule ^(.*)$ http://www.%1$1 [R=301]");

			var url = new Uri("http://somesite.com/pass");
			var context = CreateHttpContext(url).SetServerVariables(new Dictionary<string, string> { 
				{ "HTTP_HOST", url.GetComponents(UriComponents.Host, UriFormat.SafeUnescaped) } 
			});

			string expected = "http://www.somesite.com/pass";
			Uri resultUrl = target.RunRules(context, url);
			string result = context.Response.RedirectLocation;

			Assert.IsNull(resultUrl);
			Assert.AreEqual(expected, result);

			url = new Uri("http://www.somesite.com/fail");
			context = CreateHttpContext(url).SetServerVariables(new Dictionary<string, string> { 
				{ "HTTP_HOST", url.GetComponents(UriComponents.Host, UriFormat.SafeUnescaped) } 
			});

			resultUrl = target.RunRules(context, url);
			result = context.Response.RedirectLocation;

			Assert.IsNull(resultUrl);
			Assert.IsNull(result);
		}

		[TestMethod]
		public void WordPress()
		{
			var target = CreateRuleSet(@"
RewriteRule . /index.php [L]");

			var url = new Uri("http://somesite.com/");
			var context = CreateHttpContext(url).SetServerVariables(new Dictionary<string,string> {
				{ "REQUEST_FILENAME", @"D:\Hosting\4905925\html" }
			});

			Uri expected = new Uri("http://somesite.com/index.php");
			Uri result = target.RunRules(context, url);

			Assert.AreEqual(expected, result);

			url = new Uri("http://somesite.com/2007/10/start-of-something-big/");
			context = CreateHttpContext(url).SetServerVariables(new Dictionary<string, string> {
				{ "REQUEST_FILENAME", @"D:\Hosting\4905925\html\2007\10\start-of-something-big\" }
			});

			expected = new Uri("http://somesite.com/index.php");
			result = target.RunRules(context, url);

			Assert.AreEqual(expected, result);
		}
	}
}
