/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using LogR.Common.Enums;
//using LogR.Common.Interfaces.Service.App;
using LogR.Common.Models.DBView;
using LogR.Common.Models.SearchCriteria;
using LogR.Common.Models.ViewModels;
using LogR.Web.Filters;
using Framework.Infrastructure.Models.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using LogR.Api.All.Service;
using Framework.Infrastructure.Models.Search;
using LogR.Common.Interfaces.Repository.DbAccess;
//using LogR.Common.Interfaces.Service.ObjectMapping;
using Framework.Infrastructure.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Framework.Infrastructure.Models;
using Framework.Infrastructure.Interfaces.Services;
using Framework.Infrastructure.Interfaces.Helpers;
using LogR.Api.All.Service.Wip;

namespace LogR.Api.All.Controllers
{
    public partial class WipController : Controller
    {
        [HttpGet]
        [SwaggerOperation(Tags = new[] { "ToTest" })]
        public async Task<ReturnListModel<EEntity123Model>> GetEEntity123s([FromQuery]EEntity123SearchCriteria search)
        {
            return await wipService.GetEEntity123sAsync(search);
        }

        [HttpGet]
        [SwaggerOperation(Tags = new[] { "ToTest" })]
        public async Task<ReturnModel<EEntity123Model>> GetEEntity123([FromQuery]Guid guid)
        {
            return await wipService.GetEEntity123ByGuidAsync(guid);
        }

        [HttpPost]
        [SwaggerOperation(Tags = new[] { "ToTest" })]
        public async Task<ReturnModel<EEntity123Model>> CreateEEntity123([FromBody]EEntity123CreateModel eentity123Model)
        {
            return await wipService.CreateEEntity123Async(eentity123Model);
        }

        [HttpPost]
        [SwaggerOperation(Tags = new[] { "ToTest" })]
        public async Task<ReturnModel<EEntity123Model>> UpdateEEntity123([FromBody]EEntity123UpdateModel eentity123Model)
        {
            return await wipService.UpdateEEntity123Async(eentity123Model);
        }

        [HttpDelete]
        [SwaggerOperation(Tags = new[] { "ToTest" })]
        public async Task<ReturnModel<bool>> MarkEEntity123AsDeleted([FromQuery]Guid guid)
        {
            return await wipService.MarkEEntity123AsDeletedByGuidAsync(guid);
        }

        [HttpPost]
        [SwaggerOperation(Tags = new[] { "ToTest" })]
        public async Task<ReturnModel<bool>> MarkEEntity123AsNotDeleted([FromQuery]Guid guid)
        {
            return await wipService.MarkEEntity123AsNotDeletedByGuidAsync(guid);
        }

    }
}
