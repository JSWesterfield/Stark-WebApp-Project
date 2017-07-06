using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Sabio.Web.Controllers.Api;
using Sabio.Web.Domain;
using Sabio.Web.Models.Requests;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using Sabio.Web.Services.Interfaces;

namespace Sabio.Web.Controllers.Api
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
        public HttpResponseMessage UpdateResponse(UpdateSupportRequestResponseRequest model, int id) //taking a api controller class method of type httpresponsemessage and passing in a request model, along with an 'id' parameter of int type.
        {
            SupportRequest supportRequest = GetById(model.Id) //declares a variable of type domain model class and passes the model's property called 'id' into this, the above passes in a every prop(model) & that models id(int id).

            if (model.Response == null) //checks Response property of the supportRequest variable is not null.
            {
                ModelState.AddModelError("", "The response cannot be modified");
            }

            if (model == null) {
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

            _supportRequestsService.UpdateResponse(model); //binds service class interface with the signature SupportRequest UpdateResponse(model); But why is this at the end? why is it binding at the end?
            //_supportRequestsService.GetById(model.Id); //binds service class interface with the signature SupportRequest GetById(int id); Does GetById(int id) == GetByid(model.Id);
            SuccessResponse response = new SuccessResponse();
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
