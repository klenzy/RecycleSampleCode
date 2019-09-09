using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Data;
using Data.Providers;
using Models;
using Models.Domain;
using Models.Domain.Files;
using Models.Enums;
using Models.Requests.Files;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Services
{
    public class FileService : IFileService
    {
        private IDataProvider _data = null;

        public FileService(IDataProvider data)
        {
            _data = data;
        }

        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USWest2;

        public async Task<List<FileResponse>> Upload(List<IFormFile> files, string domain, string accessKey, string secretKey)
        {
            List<FileResponse> responseList = null;
            if (files.Count > 0)
            {
                foreach (IFormFile file in files)
                {
                    FileResponse model = new FileResponse();
                    var filePath = System.IO.Path.GetTempFileName();
                    string keyName = "Recycle-" + Guid.NewGuid() + "_@" + file.FileName;
                    string fileType = System.IO.Path.GetExtension(keyName).Remove(0, 1);

                    if (fileType == "jpg" || fileType == "jpeg" || fileType == "png" || fileType == "gif")
                    {
                        model.FileTypeId = 1;
                    }
                    else
                    {
                        model.FileTypeId = GetFileType(fileType);
                    }

                    model.FileName = file.FileName;
                    try
                    {
                        BasicAWSCredentials credentials = new BasicAWSCredentials(accessKey, secretKey);

                        AmazonS3Client s3Client = new AmazonS3Client(credentials, bucketRegion);

                        var transferUtilityConfig = new TransferUtilityConfig
                        {
                            ConcurrentServiceRequests = 5
                        };

                        TransferUtility fileTransferUtility = new TransferUtility(s3Client);

                        using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                            fileTransferUtility.Upload(stream, "recycle", keyName);
                        }
                        model.Url = domain + keyName;
                        if (responseList == null)
                        {
                            responseList = new List<FileResponse>();
                        }

                        responseList.Add(model);
                    }
                    catch (AmazonS3Exception e)
                    {
                        throw (e);
                    }
                    catch (Exception e)
                    {
                        throw (e);
                    }
                }
                return responseList;
            }
            else
            {
                return null;
            }
        }

        public List<int> Add(List<FileAddRequest> modelList, int userId)
        {
            List<int> idList = null;
            int id = 0;
            string procName = "dbo.Files_Insert";
            foreach (FileAddRequest model in modelList)
            {
                _data.ExecuteNonQuery(procName,
               inputParamMapper: delegate (SqlParameterCollection col)
               {
                   col.AddWithValue("@CreatedBy", userId);
                   AddCommonParams(model, col);

                   SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                   idOut.Direction = ParameterDirection.Output;

                   col.Add(idOut);
               },
               returnParameters: delegate (SqlParameterCollection returnCollection)
               {
                   object oId = returnCollection["@Id"].Value;

                   int.TryParse(oId.ToString(), out id);
                   if (idList == null)
                   {
                       idList = new List<int>();
                   }

                   idList.Add(id);
               });
            }

            return idList;
        }

        public void Update(FileUpdateRequest model)
        {
            string procName = "[dbo].[Files_Update]";
            _data.ExecuteNonQuery(procName,

                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", model.Id);

                    AddCommonParams(model, col);
                },
            returnParameters: null);
        }

        public File Get(int id)
        {
            string procName = "[dbo].[Files_Select_ById]";

            File file = null;

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
            },
            delegate (IDataReader reader, short set)
            {
                file = MapFile(reader);
            }

            );
            return file;
        }

        public Paged<File> Get(int pageIndex, int pageSize)
        {
            Paged<File> pagedResult = null;

            List<File> result = null;

            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Files_SelectAll]",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@PageIndex", pageIndex);
                    parameterCollection.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    File aFile = MapFile(reader);
                    totalCount = reader.GetSafeInt32(6);

                    if (result == null)
                    {
                        result = new List<File>();
                    }
                    result.Add(aFile);
                }
            );
            if (result != null)
            {
                pagedResult = new Paged<File>(result, pageIndex, pageSize, totalCount);
            }

            return pagedResult;
        }

        public Paged<File> GetByCurrent(int userId, int pageIndex, int pageSize)
        {
            Paged<File> pagedResult = null;

            List<File> result = null;

            int totalCount = 0;

            _data.ExecuteCmd(
                "dbo.Files_Select_ByCreatedBy",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@CreatedBy", userId);
                    parameterCollection.AddWithValue("@PageIndex", pageIndex);
                    parameterCollection.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    File aFile = MapFile(reader);
                    totalCount = reader.GetSafeInt32(6);

                    if (result == null)
                    {
                        result = new List<File>();
                    }

                    result.Add(aFile);
                }

            );
            if (result != null)
            {
                pagedResult = new Paged<File>(result, pageIndex, pageSize, totalCount);
            }

            return pagedResult;
        }

        public Paged<File> Search(string query, int pageIndex, int pageSize)
        {
            Paged<File> pagedResult = null;

            List<File> result = null;

            int totalCount = 0;

            _data.ExecuteCmd(
                "dbo.Files_Search",
                inputParamMapper: delegate (SqlParameterCollection parameterCollection)
                {
                    parameterCollection.AddWithValue("@Query", query);
                    parameterCollection.AddWithValue("@PageIndex", pageIndex);
                    parameterCollection.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    File aFile = MapFile(reader);
                    totalCount = reader.GetSafeInt32(6);

                    if (result == null)
                    {
                        result = new List<File>();
                    }

                    result.Add(aFile);
                }

            );
            if (result != null)
            {
                pagedResult = new Paged<File>(result, pageIndex, pageSize, totalCount);
            }

            return pagedResult;
        }

        public void Delete(int id)
        {
            string procName = "[dbo].[Files_Delete_ById]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
            }, returnParameters: null);
        }

        private static File MapFile(IDataReader reader)
        {
            File File = new File();

            File.FileTypeId = new FileType();

            int startingIdex = 0;

            File.Id = reader.GetSafeInt32(startingIdex++);
            File.Name = reader.GetSafeString(startingIdex++);
            File.Url = reader.GetSafeString(startingIdex++);
            File.FileTypeId.Id = reader.GetSafeInt32(startingIdex++);

            File.CreatedBy = reader.GetSafeInt32(startingIdex++);
            File.DateCreated = reader.GetSafeDateTime(startingIdex++);

            return File;
        }

        private static void AddCommonParams(FileAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@Url", model.Url);
            col.AddWithValue("@EntityTypeId", model.EntityTypeId);
            col.AddWithValue("@Name", model.Name);
            col.AddWithValue("@FileTypeId", model.FileTypeId);
        }

        private int GetFileType(string fileType)
        {
            FileTypes type;
            Enum.TryParse(fileType, true, out type);
            switch (type)
            {
                case FileTypes.Image:
                    return (int)FileTypes.Image;

                case FileTypes.Pdf:
                    return (int)FileTypes.Pdf;

                case FileTypes.ppf:
                    return (int)FileTypes.ppf;

                case FileTypes.xls:
                    return (int)FileTypes.xls;

                default:
                    return (int)FileTypes.other;
            }
        }
    }
}