using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using stark.Fundamentals.Models.Requests;
using stark.Web.Services;
using System.Web.Http;
using stark.Web.Models.Responses;
using stark.Web.Models;

namespace stark.Fundamentals.Controllers.api
{
    //
    [RoutePrefix("api/features")]
    public class FeaturesApiController : ApiController
    {

        //CREATE       
        [Route(), HttpPost]
        public HttpResponseMessage Add(FeatureCreateRequest model) // "public method named "Add" that returns an HtttpResponseMessage, that takes one parameter named "model" of type FeatureAddRequest"
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            //instantiate a a 'feature' entity service object
            FeaturesService feature = new FeaturesService();
            ItemResponse <int> response = new ItemResponse<int>();
            response.Item = feature.Create(model);  //uses featuresService
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
        
        //UPDATE 
        [Route("{Id:int}"), HttpPut]
        public HttpResponseMessage Updates(FeatureUpdateRequest model)  //binding to update request model.
        {
            // check data validation
            if (!ModelState.IsValid) //ModelState == collection of errors, is NOT valid...
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState); //...return a error response.
            }

            //invoke the service method and return a SuccessResponse model, no item response is being returned like the above Create() method. 
            FeaturesService feature = new FeaturesService();    // invoke the entity's service method
            feature.Update(model);                              // feature to return a model.
            return Request.CreateResponse(HttpStatusCode.OK, true);
        }
           
        //READ ALL      No route, since it only needs the RoutePrefix
        [Route(), HttpGet]                 
        public HttpResponseMessage GetAll()
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemsResponse<Feature> response = new ItemsResponse<Feature>();
            //response.Items = FeaturesService.GetAll();
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        ////DELETE        [DELETE/DELETE]
        //[Route("{id:int}"), HttpDelete]
        //public HttpResponseMessage Delete()
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }

        //    ItemsResponse<FeaturesService> resp = new ItemsResponse<FeaturesSer>();
        //    resp.Items = FeaturesService.Delete();

        //    return Request.CreateResponse(HttpStatusCode.OK, resp);
        //}
    }
}
