using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sabio.Models.Domain.Comment;
using Sabio.Models.Requests.Comment;
using Sabio.Services;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using System;
using System.Collections.Generic;

namespace Sabio.Web.Api.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentApiController : BaseApiController
    {
        private ICommentService _service = null;
        private IAuthenticationService<int> _authService = null;

        public CommentApiController(ICommentService service, ILogger<CommentApiController> logger, IAuthenticationService<int> authService) : base(logger)
        {
            _service = service;
            _authService = authService;
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Add(CommentAddRequest model)
        {
            ObjectResult result = null;

            int userId = _authService.GetCurrentUserId();

            try
            {
                int id = _service.Add(model, userId);
                ItemResponse<int> response = new ItemResponse<int>() { Item = id };

                result = Created201(response);
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
        public ActionResult<ItemResponse<int>> Update(CommentUpdateRequest model)
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
                response = new ErrorResponse($"Generic Error: {ex.Message} D: ");
            }

            return StatusCode(iCode, response);
        }

        [HttpPut("{id:int}/{isDeleted:int}")]
        public ActionResult<ItemResponse<int>> Delete(int id, int isDeleted)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                _service.Delete(id, isDeleted);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message} D: ");
            }

            return StatusCode(iCode, response);
        }

        [HttpGet("{id:int}"), AllowAnonymous]
        public ActionResult<ItemResponse<Comment>> Get(int id)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                Comment comment = _service.Get(id);

                if (comment == null)
                {
                    iCode = 404;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Comment> { Item = comment };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message} D: ");
            }

            return StatusCode(iCode, response);
        }

        [HttpGet("byentity/{entityId:int}/{entityTypeId:int}"), AllowAnonymous]
        public ActionResult<ItemResponse<Comment>> Get(int entityId, int entityTypeId)
        {
            ActionResult result = null;
            try
            {
                List<Comment> list = _service.Get(entityId, entityTypeId);
                if (list == null)
                {
                    result = NotFound404(new ErrorResponse("Record Not Found"));
                }
                else
                {
                    ItemResponse<List<Comment>> response = new ItemResponse<List<Comment>>();
                    response.Item = list;
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


    }
}