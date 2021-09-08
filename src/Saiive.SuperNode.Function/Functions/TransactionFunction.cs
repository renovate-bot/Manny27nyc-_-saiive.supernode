using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Saiive.SuperNode.Abstaction;
using Saiive.SuperNode.Model;
using Saiive.SuperNode.Model.Requests;

namespace Saiive.SuperNode.Function.Functions
{
    public class TransactionFunction : BaseFunction
    {
        public TransactionFunction(ILogger<AddressFunctions> logger, ChainProviderCollection chainProviderCollection, IServiceProvider serviceProvider) : base(logger, chainProviderCollection, serviceProvider)
        {
        }

        [FunctionName("GetTransactionById")]
        [OpenApiOperation(operationId: "GetTransactionById", tags: new[] { "Transaction" })]
        [OpenApiParameter(name: "network", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "coin", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "txId", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TransactionModel), Description = "The OK response")]
        public async Task<IActionResult> GetTransactionById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/{network}/{coin}/tx/id/{txId}")] HttpRequestMessage req,
            string network, string coin, string txId,
            ILogger log)
        {
            try
            {
                var obj = await ChainProviderCollection.GetInstance(coin).TransactionProvider.GetTransactionById(network, txId);

                return new OkObjectResult(obj);
            }
            catch (Exception e)
            {
                Logger.LogError($"{e}");
                return new BadRequestObjectResult(new ErrorModel(e.Message));
            }
        }

        [FunctionName("GetTransactionsByBlock")]
        [OpenApiOperation(operationId: "GetTransactionsByBlock", tags: new[] { "Transaction" })]
        [OpenApiParameter(name: "network", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "coin", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "block", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<TransactionModel>))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorModel))]
        public async Task<IActionResult> GetTransactionsByBlock(
               [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/{network}/{coin}/tx/block/{block}")] HttpRequest req,
               string coin, string network, string block)
        {

            try
            {
                var obj = await ChainProviderCollection.GetInstance(coin).TransactionProvider.GetTransactionsByBlock(network, block);
                return new OkObjectResult(obj);
            }
            catch (Exception e)
            {
                Logger.LogError($"{e}");
                return new BadRequestObjectResult(new ErrorModel(e.Message));
            }
        }

        [FunctionName("GetTransactionsByBlockHeight")]
        [OpenApiOperation(operationId: "GetTransactionsByBlockHeight", tags: new[] { "Transaction" })]
        [OpenApiParameter(name: "network", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "coin", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "height", In = ParameterLocation.Path, Required = true, Type = typeof(int))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<BlockTransactionModel>))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorModel))]
        public Task<IActionResult> GetTransactionsByBlockHeight(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/{network}/{coin}/tx/height/{height}")] HttpRequest req,
            string coin, string network, int height)
        {
            return GetTransactionsByBlockHeight(req, coin, network, height, true);
        }


        [FunctionName("GetTransactionsByBlockHeightDetails")]
        [OpenApiOperation(operationId: "GetTransactionsByBlockHeight", tags: new[] { "Transaction" })]
        [OpenApiParameter(name: "network", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "coin", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "address", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType:typeof(object), Example = typeof(List<BlockTransactionModel>))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorModel))]
        public async Task<IActionResult> GetTransactionsByBlockHeight(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/{network}/{coin}/tx/height/{height}/{includeDetails}")] HttpRequest req,
            string coin, string network, int height, bool includeDetails)
        {
            try
            {
                var obj = await ChainProviderCollection.GetInstance(coin).TransactionProvider.GetTransactionsByBlockHeight(network, height, includeDetails);
                return new OkObjectResult(obj);
            }
            catch (Exception e)
            {
                Logger.LogError($"{e}");
                return new BadRequestObjectResult(new ErrorModel(e.Message));
            }
        }

        [FunctionName("SendRawTransaction")]
        [OpenApiOperation(operationId: "SendRawTransaction", tags: new[] { "Transaction" })]
        [OpenApiParameter(name: "network", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiParameter(name: "coin", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TransactionRequest), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Example = typeof(TransactionResponse))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(ErrorModel))]
        public async Task<IActionResult> SendRawTransaction(
              [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/{network}/{coin}/tx/raw")] TransactionRequest req,
              string coin, string network)
        {
            try
            {
                var obj = await ChainProviderCollection.GetInstance(coin).TransactionProvider.SendRawTransaction(network, req);
                var ret = new TransactionResponse()
                {
                    TxId = obj
                };
                return new OkObjectResult(ret);
            }
            catch (ArgumentException e)
            {
                var currentBlock = await ChainProviderCollection.GetInstance(coin).BlockProvider.GetCurrentHeight(network);

                Logger.LogError("{coin}+{network}: Error commiting tx to blockchain ({response} for {txHex}) @ {blockHeight} block", coin, network, e.Message, req.RawTx, currentBlock.Height);
                return new BadRequestObjectResult(new ErrorModel($"{e.Message}"));
            }
        }
    }
}
