using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sabio.Models;
using Sabio.Models.Domain;
using Sabio.Models.Requests.FAQs;
using Sabio.Services;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using System;

namespace Sabio.Web.Api.Controllers
{
    [Route("api/faqs")]
    [ApiController]
    public class FAQApiController : BaseApiController
    {
        private IFAQService _service = null;
        private IAuthenticationService<int> _authService = null;

        public FAQApiController(IFAQService service, ILogger<FAQApiController> logger, IAuthenticationService<int> authService) : base(logger)
        {
            _service = service;
            _authService = authService;
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Add(FAQAddRequest model)
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
        public ActionResult<ItemResponse<int>> Update(FAQUpdateRequest model)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                _service.Update(model, _authService.GetCurrentUserId());

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
        public ActionResult<ItemResponse<FAQ>> Get(int id)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                FAQ faq = _service.Get(id);

                if (faq == null)
                {
                    iCode = 404;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<FAQ> { Item = faq };
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

        [HttpGet, AllowAnonymous]
        public ActionResult<ItemResponse<FAQ>> Get(int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try
            {
                Paged<FAQ> paged = _service.Get(pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Record Not Found"));
                }
                else
                {
                    ItemResponse<Paged<FAQ>> response = new ItemResponse<Paged<FAQ>>();
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
        public ActionResult<ItemResponse<FAQ>> GetByCurrent(int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try
            {
                Paged<FAQ> paged = _service.GetByCurrent(_authService.GetCurrentUserId(), pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Record Not Found"));
                }
                else
                {
                    ItemResponse<Paged<FAQ>> response = new ItemResponse<Paged<FAQ>>();
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
        public ActionResult<ItemResponse<FAQ>> Delete(int id)
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