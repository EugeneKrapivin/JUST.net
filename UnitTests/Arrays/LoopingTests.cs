﻿using NUnit.Framework;

namespace JUST.UnitTests.Arrays
{
    [TestFixture, Category("Loops")]
    public class LoopingTests
    {
        [Test]
        public void CurrentValue()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"current_value\": \"#currentvalue()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value\":1},{\"current_value\":2},{\"current_value\":3},{\"current_value\":4},{\"current_value\":5}]}", result);
        }

        [Test]
        public void CurrentIndex()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"current_index\": \"#currentindex()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"current_index\":0},{\"current_index\":1},{\"current_index\":2},{\"current_index\":3},{\"current_index\":4}]}", result);
        }

        [Test]
        public void LastIndex()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"last_index\": \"#lastindex()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"last_index\":4},{\"last_index\":4},{\"last_index\":4},{\"last_index\":4},{\"last_index\":4}]}", result);
        }

        [Test]
        public void LastValue()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.numbers)\": { \"last_value\": \"#lastvalue()\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NumbersArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value\":5},{\"last_value\":5},{\"last_value\":5},{\"last_value\":5},{\"last_value\":5}]}", result);
        }

        [Test]
        public void CurrentValueAtPath()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"current_value_at_path\":\"Norway\"},{\"current_value_at_path\":\"UK\"},{\"current_value_at_path\":\"Sweden\"}]}", result);
        }

        [Test]
        public void LastValueAtPath()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"last_value_at_path\": \"#lastvalueatpath($.country.language)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"last_value_at_path\":\"swedish\"},{\"last_value_at_path\":\"swedish\"},{\"last_value_at_path\":\"swedish\"}]}", result);
        }

        [Test]
        public void NestedLooping()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee)\": { \"Details\": { \"#loopwithincontext($.Details)\": { \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void ContextInputIsJToken()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"exists\": \"#exists($.country.name)\", \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"exists\":true,\"current_value_at_path\":\"Norway\"},{\"exists\":true,\"current_value_at_path\":\"UK\"},{\"exists\":true,\"current_value_at_path\":\"Sweden\"}]}", result);
        }

        [Test]
        public void GlobalContextInputRestored()
        {
            const string transformer = "{ \"iteration\": { \"#loop($.arrayobjects)\": { \"exists\": \"#exists($.country.name)\", \"current_value_at_path\": \"#currentvalueatpath($.country.name)\" } }, \"root\": \"#valueof($)\" }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.ObjectArray);

            Assert.AreEqual("{\"iteration\":[{\"exists\":true,\"current_value_at_path\":\"Norway\"},{\"exists\":true,\"current_value_at_path\":\"UK\"},{\"exists\":true,\"current_value_at_path\":\"Sweden\"}],\"root\":" + ExampleInputs.ObjectArray.Replace(" ", "") + "}", result);
        }

        [Test]
        public void NestedLoopingContextInput()
        {
            const string transformer = "{ \"hello\": { \"#loop($.NestedLoop.Organization.Employee)\": { \"Details\": { \"#loopwithincontext($.Details)\": { \"Exists\": \"#exists($.Country)\", \"IsIsland\": \"#ifcondition(#currentvalueatpath($.Country),Iceland,#toboolean(True),#toboolean(False))\", \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"Exists\":true,\"IsIsland\":true,\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"Exists\":true,\"IsIsland\":false,\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void FunctionAsLoopArgument()
        {
            const string transformer = "{ \"hello\": { \"#loop(#xconcat($.NestedLoop.,Organization,.Employee))\": { \"Details\": { \"#loopwithincontext(#concat($.,Details))\": { \"CurrentCountry\": \"#currentvalueatpath($.Country)\" } } } } }";

            var result = JsonTransformer.Transform(transformer, ExampleInputs.NestedArrays);

            Assert.AreEqual("{\"hello\":[{\"Details\":[{\"CurrentCountry\":\"Iceland\"}]},{\"Details\":[{\"CurrentCountry\":\"Denmark\"}]}]}", result);
        }

        [Test]
        public void PrimitiveTypeArrayResult()
        {
            const string input = "[{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" },{ \"id\": 2, \"name\": \"Person 2\", \"gender\": \"F\" },{ \"id\": 3, \"name\": \"Person 3\", \"gender\": \"M\" }]";
            const string transformer = "{ \"result\": { \"#loop([?(@.gender=='M')])\": \"#currentvalueatpath($.name)\" } }";

            var result = JsonTransformer.Transform(transformer, input, new JUSTContext {EvaluationMode = EvaluationMode.Strict});

            Assert.AreEqual("{\"result\":[\"Person 1\",\"Person 3\"]}", result);
        }

        [Test]
        public void ObjectTypeArrayResult()
        {
            const string input = "[{ \"id\": 1, \"name\": \"Person 1\", \"gender\": \"M\" },{ \"id\": 2, \"name\": \"Person 2\", \"gender\": \"F\" },{ \"id\": 3, \"name\": \"Person 3\", \"gender\": \"M\" }]";
            const string transformer = "{ \"result\": { \"#loop([?(@.gender=='M')])\": \"#currentvalue()\" } }";

            var result = JsonTransformer.Transform(transformer, input, new JUSTContext { EvaluationMode = EvaluationMode.Strict });

            Assert.AreEqual("{\"result\":[{\"id\":1,\"name\":\"Person 1\",\"gender\":\"M\"},{\"id\":3,\"name\":\"Person 3\",\"gender\":\"M\"}]}", result);
        }
    }
}
