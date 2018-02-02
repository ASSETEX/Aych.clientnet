﻿using Moq;
using StackExchange.StacMan.Tests.Utilities;
using System;
using System.Linq;
using Xunit;

namespace StackExchange.StacMan.Tests
{

    public class ApiVersion21Tests
    {
        [Fact]
        public void Answer_tags_test()
        {
            var mock20 = new Mock<StacManClient>(null, "2.0");
            var mock21 = new Mock<StacManClient>(null, "2.1");

            //http://api.stackexchange.com/2.0/answers?pagesize=1&order=desc&sort=activity&site=stackoverflow&filter=!9hnGsz84b
            mock20.FakeGET(response: @"{""items"":[{""question_id"":17127032,""answer_id"":17127341,""creation_date"":1371325907,""last_activity_date"":1371325907,""score"":0,""is_accepted"":false,""owner"":{""user_id"":2486415,""display_name"":""user2486415"",""reputation"":10,""user_type"":""registered"",""profile_image"":""http://www.gravatar.com/avatar/8d5eb93a89200d4f7900f46dc2c26d3b?d=identicon&r=PG"",""link"":""http://stackoverflow.com/users/2486415/user2486415""}}],""quota_remaining"":98,""quota_max"":300,""has_more"":true}");
            //http://api.stackexchange.com/2.1/answers?pagesize=1&order=desc&sort=activity&site=stackoverflow&filter=!9hnGsz84b
            mock21.FakeGET(response: @"{""items"":[{""question_id"":17127032,""answer_id"":17127341,""creation_date"":1371325907,""last_activity_date"":1371325907,""score"":0,""is_accepted"":false,""owner"":{""user_id"":2486415,""display_name"":""user2486415"",""reputation"":10,""user_type"":""registered"",""profile_image"":""http://www.gravatar.com/avatar/8d5eb93a89200d4f7900f46dc2c26d3b?d=identicon&r=PG"",""link"":""http://stackoverflow.com/users/2486415/user2486415""},""tags"":[""javascript"",""jquery"",""css""]}],""quota_remaining"":97,""quota_max"":300,""has_more"":true}");

            var client20 = mock20.Object;
            var client21 = mock21.Object;

            var result20 = client20.Answers.GetAll("stackoverflow.com", pagesize: 1, order: Order.Desc, sort: Answers.Sort.Activity, filter: "!9hnGsz84b").Result;
            var result21 = client21.Answers.GetAll("stackoverflow.com", pagesize: 1, order: Order.Desc, sort: Answers.Sort.Activity, filter: "!9hnGsz84b").Result;

            Assert.True(result20.Success);
            Assert.True(result21.Success);

            var answer20 = result20.Data.Items.Single();
            var answer21 = result21.Data.Items.Single();

            Assert.Null(answer20.Tags);
            Assert.NotNull(answer21.Tags);
            Assert.Equal(3, answer21.Tags.Length);
        }

        [Fact]
        public void Merge_get_test()
        {
            var mock = new Mock<StacManClient>(null, "2.1");

            //http://api.stackexchange.com/2.1/users/1450259/merges
            mock.FakeGET(response: @"{""items"":[{""old_account_id"":2885329,""new_account_id"":1450259,""merge_date"":1371139987}],""quota_remaining"":89,""quota_max"":300,""has_more"":false}");

            var client = mock.Object;

            var result = client.Users.GetMerges(new int[] { 1450259 }).Result;
            Assert.True(result.Success);

            var merge = result.Data.Items.Single();
            Assert.Equal(2885329, merge.OldAccountId);
            Assert.Equal(1450259, merge.NewAccountId);
            Assert.Equal(1371139987L.ToDateTime(), merge.MergeDate);
        }

        /// <summary>
        /// A vectorized version of this method was introduced in 2.1:
        /// http://api.stackexchange.com/docs/change-log
        /// </summary>
        [Fact]
        public void User_top_answer_tags_vectorized_test()
        {
            var mock = new Mock<StacManClient>(null, "2.1");

            //http://api.stackexchange.com/2.1/users/1/top-answer-tags?pagesize=3&site=stackoverflow
            mock.FakeGETForUrlPattern("/1/", response: @"{""items"":[{""tag_name"":""c#"",""question_score"":79,""question_count"":1,""answer_score"":331,""answer_count"":21,""user_id"":1},{""tag_name"":""regex"",""question_score"":3,""question_count"":1,""answer_score"":243,""answer_count"":15,""user_id"":1},{""tag_name"":"".net"",""question_score"":98,""question_count"":3,""answer_score"":204,""answer_count"":18,""user_id"":1}],""quota_remaining"":75,""quota_max"":300,""has_more"":true}");
            //http://api.stackexchange.com/2.1/users/1%3B3/top-answer-tags?pagesize=3&site=stackoverflow.com
            mock.FakeGETForUrlPattern("stackoverflow\\.com", response: @"{""items"":[{""tag_name"":""c#"",""question_score"":79,""question_count"":1,""answer_score"":331,""answer_count"":21,""user_id"":1},{""tag_name"":""html"",""question_score"":0,""question_count"":0,""answer_score"":315,""answer_count"":3,""user_id"":3},{""tag_name"":""jquery"",""question_score"":0,""question_count"":0,""answer_score"":309,""answer_count"":2,""user_id"":3}],""quota_remaining"":76,""quota_max"":300,""has_more"":true}");

            var client = mock.Object;

            var result = client.Users.GetTopAnswerTags("stackoverflow", 1, pagesize: 3).Result;
            var resultVectorized = client.Users.GetTopAnswerTags("stackoverflow.com", new int[] { 1, 3 }, pagesize: 3).Result;

            Assert.True(result.Success);
            Assert.True(resultVectorized.Success);

            Assert.Equal(1, result.Data.Items.Select(i => i.UserId).Distinct().Count());
            Assert.Equal(2, resultVectorized.Data.Items.Select(i => i.UserId).Distinct().Count());
        }

        [Fact]
        public void Api_version_mismatch_test()
        {
            var mock = new Mock<StacManClient>(null, "2.0");

            var client = mock.Object;

            Assert2.Throws<InvalidOperationException>(() => client.Users.GetTopAnswerTags("stackoverflow.com", new int[] { 1, 3 }, pagesize: 3));
        }

        [Fact]
        public void Notice_test()
        {
            var mock = new Mock<StacManClient>(null, "2.1");

            //http://api.stackexchange.com/2.1/questions/7399584?order=desc&sort=activity&site=stackoverflow&filter=!9hnGsqOrt
            mock.FakeGET(response: @"{""items"":[{""question_id"":7399584,""last_edit_date"":1317126047,""creation_date"":1315906028,""last_activity_date"":1323375260,""score"":7,""answer_count"":1,""accepted_answer_id"":7632327,""bounty_amount"":50,""title"":""Sql Server 2008 R2 DC Inserts Performance Change"",""tags"":[""sql-server"",""sql-server-2008"",""sql-server-2008-r2"",""performance-testing"",""sql-server-performance""],""view_count"":341,""owner"":{""user_id"":546051,""display_name"":""Falcon"",""reputation"":323,""user_type"":""registered"",""profile_image"":""http://www.gravatar.com/avatar/5d914c55df57402dadc984105382d0a0?d=identicon&r=PG"",""link"":""http://stackoverflow.com/users/546051/falcon"",""accept_rate"":78},""link"":""http://stackoverflow.com/questions/7399584/sql-server-2008-r2-dc-inserts-performance-change"",""is_answered"":true,""notice"":{""body"":""<p>I would like to know why there is a change in performance after 1.5M inserts. Where is the change in the graph from one pattern to another coming from. I expect some kind of a prove.</p>"",""creation_date"":1316505623,""owner_user_id"":546051}}],""quota_remaining"":296,""quota_max"":300,""has_more"":false}");

            var client = mock.Object;

            var result = client.Questions.GetByIds("stackoverflow", new int[] { 7399584 }, order: Order.Desc, sort: Questions.Sort.Activity, filter: "!9hnGsqOrt").Result;
            Assert.True(result.Success);

            var question = result.Data.Items.Single();
            Assert.NotNull(question.Notice);
            Assert.Equal("<p>I would like to know why there is a change in performance after 1.5M inserts. Where is the change in the graph from one pattern to another coming from. I expect some kind of a prove.</p>", question.Notice.Body);
            Assert.Equal(1316505623L.ToDateTime(), question.Notice.CreationDate);
            Assert.Equal(546051, question.Notice.OwnerUserId);
        }

        [Fact]
        public void Reputation_history_test()
        {
            var mock = new Mock<StacManClient>(null, "2.1");

            //http://api.stackexchange.com/2.1/users/2749/reputation-history?pagesize=3&site=stackoverflow
            mock.FakeGET(response: @"{""items"":[{""user_id"":2749,""creation_date"":1370863493,""post_id"":5149758,""reputation_change"":10,""reputation_history_type"":""post_upvoted""},{""user_id"":2749,""creation_date"":1370754616,""post_id"":10731127,""reputation_change"":10,""reputation_history_type"":""post_upvoted""},{""user_id"":2749,""creation_date"":1369860751,""post_id"":16821800,""reputation_change"":2,""reputation_history_type"":""asker_accepts_answer""}],""quota_remaining"":289,""quota_max"":300,""has_more"":true}");

            var client = mock.Object;

            var result = client.Users.GetReputationHistory("stackoverflow", new int[] { 2749 }, pagesize: 3).Result;
            Assert.True(result.Success);

            var second = result.Data.Items[1];
            Assert.Equal(2749, second.UserId);
            Assert.Equal(1370754616L.ToDateTime(), second.CreationDate);
            Assert.Equal(10731127, second.PostId);
            Assert.Equal(10, second.ReputationChange);
            Assert.Equal(ReputationHistories.ReputationHistoryType.PostUpvoted, second.ReputationHistoryType);
        }

        [Fact]
        public void Comment_add_test()
        {
            var mock = new Mock<StacManClient>(null, "2.1");

            //https://api.stackexchange.com/2.1/posts/4490791/comments/add
            mock.FakePOST(response: @"{""items"":[{""post_id"":4490791,""creation_date"":1371346039,""edited"":false,""owner"":{""user_id"":2749,""display_name"":""Emmett"",""reputation"":5451,""profile_image"":""http://www.gravatar.com/avatar/129bc58fc3f1e3853cdd3cefc75fe1a0?d=identicon&r=PG"",""link"":""http://stackoverflow.com/users/2749/emmett"",""accept_rate"":76}}],""quota_remaining"":9997,""quota_max"":10000,""has_more"":false}");

            var client = mock.Object;

            var result = client.Posts.AddComment("stackoverflow", "access_token_123", 4490791, "This is a comment that I'm adding via the API!", preview: true).Result;
            Assert.True(result.Success);

            var comment = result.Data.Items.Single();
            Assert.Equal(4490791, comment.PostId);
        }

        [Fact]
        public void Comment_delete_test()
        {
            var mock = new Mock<StacManClient>(null, "2.1");

            //https://api.stackexchange.com/2.1/comments/4721972/delete
            mock.FakePOST(response: @"{""items"":[],""quota_remaining"":9992,""quota_max"":10000,""has_more"":false}");

            var client = mock.Object;

            var result = client.Comments.Delete("stackoverflow", "access_token_123", 4721972).Result;
            Assert.True(result.Success);

            Assert.Equal(0, result.Data.Items.Length);
            Assert.Equal(10000, result.Data.QuotaMax);
        }
    }
}
