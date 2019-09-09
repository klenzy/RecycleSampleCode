using Data;
using Data.Providers;
using Models.Domain;
using Models.Domain.Comment;
using Models.Requests.Comment;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Services
{
    public class CommentService : ICommentService
    {
        private IDataProvider _data = null;

        public CommentService(IDataProvider data)
        {
            _data = data;
        }

        public int Add(CommentAddRequest model, int userId)
        {
            int id = 0;

            string procName = "dbo.Comments_Insert";
            _data.ExecuteNonQuery(procName,

                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col);

                    col.AddWithValue("@CreatedBy", userId);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;

                    col.Add(idOut);
                },

                returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;

                    int.TryParse(oId.ToString(), out id);
                });
            return id;
        }

        public void Update(CommentUpdateRequest model)
        {
            string procName = "[dbo].[Comments_Update]";
            _data.ExecuteNonQuery(procName,

                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", model.Id);

                    AddCommonParams(model, col);
                },
            returnParameters: null);
        }

        public Comment Get(int id)
        {
            Dictionary<int, List<Comment>> replyCollection = null;
            List<Comment> resultList = null;

            string procName = "[dbo].[Comments_SelectAll_Id]";

            _data.ExecuteCmd(procName, inputParamMapper: null
                , singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    Comment aComment = MapComment(reader);

                    if (resultList == null)
                    {
                        resultList = new List<Comment>();
                    }
                    if (replyCollection == null)
                    {
                        replyCollection = new Dictionary<int, List<Comment>>();
                    }
                    if (replyCollection.ContainsKey(aComment.ParentId))
                    {
                        replyCollection[aComment.ParentId].Add(aComment);
                    }
                    else
                    {
                        replyCollection.Add(aComment.ParentId, new List<Comment> { aComment });
                    }
                    resultList.Add(aComment);
                });

            if (resultList != null)
            {
                foreach (List<Comment> commentsList in replyCollection.Values)
                {
                    foreach (Comment comment in commentsList)
                    {
                        if (replyCollection.ContainsKey(comment.Id))
                        {
                            comment.Replies = replyCollection[comment.Id];
                        }
                    }
                }
            }

            return resultList[id - 1];
        }

        public List<Comment> Get(int entityId, int entityTypeId)
        {
            Dictionary<int, List<Comment>> replyCollection = null;

            List<Comment> result = null;

            _data.ExecuteCmd(
                "[dbo].[Comments_SelectAll]",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@EntityTypeId", entityTypeId);
                    parameterCollection.AddWithValue("@EntityId", entityId);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    switch (set)
                    {
                        case 0:
                            Comment replyComment = MapComment(reader);
                            if (replyCollection == null)
                            {
                                replyCollection = new Dictionary<int, List<Comment>>();
                            }
                            if (replyCollection.ContainsKey(replyComment.ParentId))
                            {
                                replyCollection[replyComment.ParentId].Add(replyComment);
                            }
                            else
                            {
                                replyCollection.Add(replyComment.ParentId, new List<Comment> { replyComment });
                            }
                            break;

                        case 1:
                            Comment comment = MapComment(reader);

                            if (result == null)
                            {
                                result = new List<Comment>();
                            }
                            if (replyCollection == null)
                            {
                                replyCollection = new Dictionary<int, List<Comment>>();
                            }
                            if (replyCollection.ContainsKey(comment.Id))
                            {
                                comment.Replies = replyCollection[comment.Id];
                            }
                            result.Add(comment);
                            break;
                    }
                }
            );

            if (result != null)
            {
                foreach (List<Comment> commentsList in replyCollection.Values)
                {
                    foreach (Comment comment in commentsList)
                    {
                        if (replyCollection.ContainsKey(comment.Id))
                        {
                            comment.Replies = replyCollection[comment.Id];
                        }
                    }
                }
            }

            return result;
        }

        public void Delete(int id, int IsDeleted)
        {
            string procName = "[dbo].[Comments_DeleteByStatusId]";
            _data.ExecuteNonQuery(procName,

                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                    col.AddWithValue("@IsDeleted", IsDeleted);
                },
            returnParameters: null);
        }

        private static Comment MapComment(IDataReader reader)
        {
            Comment Comment = new Comment();

            int startingIndex = 0;

            Comment.Id = reader.GetSafeInt32(startingIndex++);
            Comment.Subject = reader.GetSafeString(startingIndex++);
            Comment.Text = reader.GetSafeString(startingIndex++);
            Comment.ParentId = reader.GetSafeInt32(startingIndex++);
            Comment.EntityTypeId = reader.GetSafeInt32(startingIndex++);
            Comment.EntityId = reader.GetSafeInt32(startingIndex++);
            Comment.DateCreated = reader.GetSafeUtcDateTime(startingIndex++);
            Comment.DateModified = reader.GetSafeUtcDateTime(startingIndex++);
            Comment.IsDeleted = reader.GetSafeBool(startingIndex++);
            Comment.CreatedBy = new CommentProfile();
            Comment.CreatedBy.UserId = reader.GetSafeInt32(startingIndex++);
            Comment.CreatedBy.FirstName = reader.GetSafeString(startingIndex++);
            Comment.CreatedBy.LastName = reader.GetSafeString(startingIndex++);
            Comment.CreatedBy.Mi = reader.GetSafeString(startingIndex++);
            Comment.CreatedBy.AvatarUrl = reader.GetSafeString(startingIndex++);

            return Comment;
        }

        private static void AddCommonParams(CommentAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@Subject", model.Subject);
            col.AddWithValue("@Text", model.Text);
            col.AddWithValue("@ParentId", model.ParentId);
            col.AddWithValue("@EntityTypeId", model.EntityTypeId);
            col.AddWithValue("@EntityId", model.EntityId);
        }
    }
}