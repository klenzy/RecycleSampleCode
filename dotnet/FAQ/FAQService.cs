using Data;
using Data.Providers;
using Models;
using Models.Domain;
using Models.Requests.FAQs;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Services
{
    public class FAQService : IFAQService
    {
        private IDataProvider _data = null;

        public FAQService(IDataProvider data)
        {
            _data = data;
        }

        public int Add(FAQAddRequest model, int userId)
        {
            int id = 0;

            string procName = "dbo.FAQs_Insert";
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

        public void Update(FAQUpdateRequest model, int userId)
        {
            string procName = "[dbo].[FAQs_Update]";
            _data.ExecuteNonQuery(procName,

                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", model.Id);

                    AddCommonParams(model, col);
                    col.AddWithValue("@ModifiedBy", userId);
                },
            returnParameters: null);
        }

        public FAQ Get(int id)
        {
            string procName = "[dbo].[FAQs_Select_ById]";

            FAQ faq = null;

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
            },
            delegate (IDataReader reader, short set)
            {
                faq = MapFAQ(reader);
            }

            );
            return faq;
        }

        public Paged<FAQ> Get(int pageIndex, int pageSize)
        {
            Paged<FAQ> pagedResult = null;

            List<FAQ> result = null;

            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[FAQs_SelectAll]",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@PageIndex", pageIndex);
                    parameterCollection.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    FAQ aFAQ = MapFAQ(reader);
                    totalCount = reader.GetSafeInt32(9);

                    if (result == null)
                    {
                        result = new List<FAQ>();
                    }
                    result.Add(aFAQ);
                }
            );
            if (result != null)
            {
                pagedResult = new Paged<FAQ>(result, pageIndex, pageSize, totalCount);
            }

            return pagedResult;
        }

        public Paged<FAQ> GetByCurrent(int userId, int pageIndex, int pageSize)
        {
            Paged<FAQ> pagedResult = null;

            List<FAQ> result = null;

            int totalCount = 0;

            _data.ExecuteCmd(
                "dbo.FAQs_Select_ByCreatedBy",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@CreatedBy", userId);
                    parameterCollection.AddWithValue("@PageIndex", pageIndex);
                    parameterCollection.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    FAQ aFAQ = MapFAQ(reader);
                    totalCount = reader.GetSafeInt32(9);

                    if (result == null)
                    {
                        result = new List<FAQ>();
                    }

                    result.Add(aFAQ);
                }

            );
            if (result != null)
            {
                pagedResult = new Paged<FAQ>(result, pageIndex, pageSize, totalCount);
            }

            return pagedResult;
        }

        public void Delete(int id)
        {
            string procName = "[dbo].[FAQs_Delete_ById]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
            }, returnParameters: null);
        }

        private static FAQ MapFAQ(IDataReader reader)
        {
            FAQ FAQ = new FAQ();

            int startingIdex = 0;

            FAQ.Id = reader.GetSafeInt32(startingIdex++);
            FAQ.Question = reader.GetSafeString(startingIdex++);
            FAQ.Answer = reader.GetSafeString(startingIdex++);
            FAQ.CategoryId = reader.GetSafeInt32(startingIdex++);
            FAQ.SortOrder = reader.GetSafeInt32(startingIdex++);
            FAQ.DateCreated = reader.GetSafeUtcDateTime(startingIdex++);
            FAQ.DateModified = reader.GetSafeUtcDateTime(startingIdex++);
            FAQ.CreatedBy = reader.GetSafeInt32(startingIdex++);
            FAQ.ModifiedBy = reader.GetSafeInt32(startingIdex++);

            return FAQ;
        }

        private static void AddCommonParams(FAQAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@Question", model.Question);
            col.AddWithValue("@Answer", model.Answer);
            col.AddWithValue("@CategoryId", model.CategoryId);
            col.AddWithValue("@SortOrder", model.SortOrder);
        }
    }
}