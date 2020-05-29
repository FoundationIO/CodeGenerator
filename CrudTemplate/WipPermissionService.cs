/**
Copyright (c) 2016 Foundation.IO (https://github.com/foundationio). All rights reserved.

This work is licensed under the terms of the BSD license.
For a copy, see <https://opensource.org/licenses/BSD-3-Clause>.
**/
using Framework.Infrastructure.Exceptions;
using Framework.Infrastructure.Interfaces.DbAccess;
using Framework.Infrastructure.Interfaces.Helpers;
using Framework.Infrastructure.Logging;
using Framework.Infrastructure.Models.Result;
using Framework.Infrastructure.Utils;
using LogR.Api.All.Controllers;
using LogR.Api.All.Repository.Wip;
using LogR.Common.Enums;
using LogR.Common.Models.DB;
using LogR.Common.Models.DBView;
using LogR.Common.Models.ViewModels;
using LogR.Common.Models.SearchCriteria;
using LogR.Repository.Common;
using LogR.Service.App;
using LogR.Service.ObjectMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.AccessControl;

namespace LogR.Service.App
{
    public partial class PermissionService
    {
        public async Task<(bool, bool, string)> GetCanManageEEntity123PermissionParamsAsync(long? tenantId = null, long? tenantEntityId = null)
        {
            return await GetCanManageEEntity123PermissionParamsInternalAsync(tenantId, tenantEntityId, UserPermissionTypes.ManageTenantDependents, UserPermissionTypes.ConfigureSystem, UserPermissionTypes.ConfigureTenant);
        }

        public async Task<(bool, bool, string)> GetCanViewEEntity123PermissionParamsAsync(long? tenantId = null, long? tenantEntityId = null)
        {
            return await GetCanManageEEntity123PermissionParamsInternalAsync(tenantId, tenantEntityId, UserPermissionTypes.ViewTenantDependents, UserPermissionTypes.ManageTenantDependents, UserPermissionTypes.ConfigureSystem, UserPermissionTypes.ConfigureTenant);
        }

        private async Task<(bool, bool, string)> GetCanManageEEntity123PermissionParamsInternalAsync(long? tenantId, long? tenantEntityId, params UserPermissionTypes[] permissions)
        {
            var (resultValue, canAssign, errorStr, ignoreFurtherCheck) = await GetTenantParamsValidationAsync(tenantId, tenantEntityId);

            if (resultValue == false)
                return (resultValue, canAssign, errorStr);

            if (ignoreFurtherCheck)
                return (resultValue, canAssign, errorStr);

            if (!await DoesUserHavePermissionCurrentRole(permissions))
                return (false, false, "No permission to access this resource");

            return (resultValue, canAssign, errorStr);
        }

    }
}
