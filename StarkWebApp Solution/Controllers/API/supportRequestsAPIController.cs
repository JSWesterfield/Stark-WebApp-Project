using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using stark.Web.Controllers.Api;
using stark.Web.Domain;
using stark.Web.Models.Requests;
using stark.Web.Models.Responses;
using stark.Web.Services;
using stark.Web.Services.Interfaces;

namespace stark.Web.Controllers.Ap
{
    [RoutePrefix("api/support-requests")]
    public class SupportRequestsApiController : BaseApiController
    {
        private ISupportRequestsService _supportRequestsService;

        public SupportRequestsApiController(SupportRequestsService supportRequestService)
        {
            _supportRequestsService = supportRequestService;
        }

        //CREATE

        [Route(""), HttpPost]
        public HttpResponseMessage Create(SupportRequestCreateRequest model)
        {
            if (model == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,"Please input data to create a data request");
            }
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<int> generatedId = new ItemResponse<int>();
            generatedId.Item = _supportRequestsService.CreateSupportRequest(model);
            return Request.CreateResponse(HttpStatusCode.OK, generatedId);
        }

        //UPDATE

        [Route("{id:int}"), HttpPut]
        public HttpResponseMessage Update(SupportRequestUpdateRequest model, int id)
        {
            if (model == null)
            {
                ModelState.AddModelError("Id", "You must input id to update a support request.");
            }

            if (id != model.Id)
            {
                ModelState.AddModelError("id", "You cannot alter the id property.");
            }

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            _supportRequestsService.UpdateSupportRequest(model);
            SuccessResponse response = new SuccessResponse();
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        //GET ALL

        [Route(""), HttpGet]
        public HttpResponseMessage GetAll()
        {

            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemsResponse<SupportRequest> allSupportRequests = new ItemsResponse<SupportRequest>();
            allSupportRequests.Items = _supportRequestsService.GetAll();

            return Request.CreateResponse(HttpStatusCode.OK, allSupportRequests);
        }

        //GET BY ID

        [Route("{id:int}"), HttpGet]
        public HttpResponseMessage GetById(int id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<SupportRequest> response = new ItemResponse<SupportRequest>();
            response.Item = _supportRequestsService.GetById(id);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        //UPDATE RESPONSE   

        [Route("{id:int}/response"), HttpPut]
        public HttpResponseMessage UpdateResponse(UpdateSupportRequestResponseRequest model, int id)
        {
            if (model == null){
                ModelState.AddModelError("", "Please input userId & response");
            }
            if (id != model.Id)
            {
                ModelState.AddModelError("", "This ID is not available for reply");
            }
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            _supportRequestsService.UpdateResponse(model);
            SuccessResponse response = new SuccessResponse();
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


    }
}
