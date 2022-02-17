using AdvertApi.Models;
using AdvertApi.Models.Messages;
using AdvertApi.Services;
using Amazon.SimpleNotificationService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AdvertApi.Controllers
{
    [Route("adverts/v1")]
    [ApiController]
    public class AdvertController : ControllerBase
    {
        private readonly IAdvertStorageService advertStorageService;
        private readonly IConfiguration configuration;

        public AdvertController(IAdvertStorageService advertStorageService, IConfiguration configuration)
        {
            this.advertStorageService = advertStorageService;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(400)]
        [ProducesResponseType(201, Type=typeof(CreateAdvertResponse))]
        public async Task<IActionResult> Create(AdvertModel model)
        {
            string recordId;
            try
            {
                recordId = await advertStorageService.Add(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return StatusCode(201, new CreateAdvertResponse { Id = recordId });
        }


        [HttpPost]
        [Route("Confirm")]
        [ProducesResponseType(400)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Confirm(ConfirmAdvertModel model)
        {
            bool confirmed;
            try
            {
                confirmed = await advertStorageService.Confirm(model);
                await RaiseAdvertConfirmedMessage(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return Ok();
        }

        /// <summary>
        /// Raise a event - send out a message to the SNS
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task RaiseAdvertConfirmedMessage(ConfirmAdvertModel model)
        {
            var topicArn = configuration.GetValue<string>("TopicArn");
            var dbModel = await advertStorageService.GetById(model.Id);

            using var client = new AmazonSimpleNotificationServiceClient();
            var message = new AdvertConfirmedMessage
            {
                Id = model.Id,
                Title = dbModel.Title
            };

            var messageJson = JsonConvert.SerializeObject(message);
            await client.PublishAsync(topicArn, messageJson);
        }
    }
}
