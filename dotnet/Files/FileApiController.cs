using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using Models.Domain;
using Models.Domain.Files;
using Models.Requests.Files;
using Services;
using Web.Controllers;
using Web.Core.Configs;
using Web.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Api.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FileApiController : BaseApiController
    {
        private IFileService _service = null;
        private IAuthenticationService<int> _authService = null;
        private IOptions<AWSConfig> _aws = null;
        private AWSConfig _config;

        public FileApiController(IFileService service, ILogger<FileApiController> logger, IOptions<AWSConfig> aws, IAuthenticationService<int> authService) : base(logger)
        {
            _service = service;
            _authService = authService;
            _config = aws.Value;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            ObjectResult result = null;

            try
            {
                List<FileResponse> urlList = await _service.Upload(files, _config.Domain, _config.AccessKey, _config.Secret);

                ItemsResponse<FileResponse> response = new ItemsResponse<FileResponse>() { Items = urlList };

                result = StatusCode(201, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                ErrorResponse response = new ErrorResponse(ex.Message);

                result = StatusCode(500, response);
            }

            return result;
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Add(List<FileAddRequest> modelList)
        {
            ObjectResult result = null;

            int userId = _authService.GetCurrentUserId();

            try
            {
                List<int> id = _service.Add(modelList, userId);
                ItemsResponse<int> response = new ItemsResponse<int>() { Items = id };

                result = StatusCode(201, response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                ErrorResponse response = new ErrorResponse(ex.Message);

                result = StatusCode(500, response);
            }

            return result;
        }

        [HttpPut("{id:int}")]
        public ActionResult<ItemResponse<int>> Update(FileUpdateRequest model)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                _service.Update(model);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(iCode, response);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<File>> Get(int id)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                File file = _service.Get(id);

                if (file == null)
                {
                    iCode = 404;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<File> { Item = file };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(iCode, response);
        }

        [HttpGet]
        public ActionResult<ItemResponse<File>> Get(int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try
            {
                Paged<File> paged = _service.Get(pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Record Not Found"));
                }
                else
                {
                    ItemResponse<Paged<File>> response = new ItemResponse<Paged<File>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }

        [HttpGet("current")]
        public ActionResult<ItemResponse<File>> GetByCurrent(int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try
            {
                Paged<File> paged = _service.GetByCurrent(_authService.GetCurrentUserId(), pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Record Not Found"));
                }
                else
                {
                    ItemResponse<Paged<File>> response = new ItemResponse<Paged<File>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }

        [HttpGet("search")]
        public ActionResult<ItemResponse<File>> Search(string query, int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try
            {
                Paged<File> paged = _service.Search(query, pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Record Not Found"));
                }
                else
                {
                    ItemResponse<Paged<File>> response = new ItemResponse<Paged<File>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }

        [HttpDelete("{id:int}")]
        public ActionResult<ItemResponse<File>> Delete(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.Delete(id);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }
    }
}